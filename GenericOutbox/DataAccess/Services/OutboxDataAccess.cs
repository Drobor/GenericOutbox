using GenericOutbox.DataAccess.Entities;
using GenericOutbox.DataAccess.Models;
using GenericOutbox.Enums;
using Microsoft.EntityFrameworkCore;

namespace GenericOutbox.DataAccess.Services;

public class OutboxDataAccess<TDbContext>(TDbContext dbContext, OutboxOptions outboxOptions) : IOutboxDataAccess
    where TDbContext : DbContext
{
    private static readonly OutboxRecordStatus[] s_unlockedStatuses = new[]
    {
        OutboxRecordStatus.ReadyToExecute,
        OutboxRecordStatus.WaitingForRetry,
        OutboxRecordStatus.Completed
    };

    private int _rollingOutboxQueryType = 0; //0 = non-locked, 1 = locked

    public async Task CommitExecutionResult(OutboxEntity outboxEntity, ExecutionResult executionResult)
    {
        var now = DateTime.UtcNow;

        await dbContext
            .Set<OutboxEntity>()
            .Where(x => x.Id == outboxEntity.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.RetriesCount, r => r.RetriesCount + 1)
                .SetProperty(r => r.Status, executionResult.Status)
                .SetProperty(r => r.RetryTimeoutUtc, executionResult.RetryTimeoutUtc)
                .SetProperty(r => r.LastUpdatedUtc, now)
                .SetProperty(r => r.HandlerLock, (Guid?)null)
            );
    }

    public async Task<OutboxEntityDispatchModel[]> GetOutboxRecords(int maxCount)
    {
        Guid lockId = Guid.NewGuid();

        var recordsToUpdateCount = _rollingOutboxQueryType switch
        {
            0 => await ReserveNonLockedRetryOutboxRecords(lockId, maxCount),
            1 => await ReserveNonLockedOutboxRecords(lockId, maxCount),
            2 => await ReserveLockedRetryOutboxRecords(lockId, maxCount),
            3 => await ReserveLockedOutboxRecords(lockId, maxCount),
            _ => await ReserveStuckInProgressRecords(lockId, maxCount)
        };

        var stuckInProgress = _rollingOutboxQueryType == 4;

        if (recordsToUpdateCount < maxCount)
            _rollingOutboxQueryType = (_rollingOutboxQueryType + 1) % 5;

        if (recordsToUpdateCount == 0)
            return Array.Empty<OutboxEntityDispatchModel>();

        var result = await dbContext
            .Set<OutboxEntity>()
            .AsNoTracking()
            .Where(x => x.HandlerLock == lockId)
            .ToArrayAsync();

        return result
            .Select(x => new OutboxEntityDispatchModel(x, stuckInProgress))
            .ToArray();
    }

    private async Task<int> ReserveLockedOutboxRecords(Guid handlerLockId, int maxCount)
    {
        var now = DateTime.UtcNow;

        return await dbContext
            .Set<OutboxEntity>()
            .Where(r =>
                r.HandlerLock == null
                && r.Status == OutboxRecordStatus.ReadyToExecute
                && dbContext
                    .Set<OutboxEntity>()
                    .Where(x => x.Lock != null
                                && !dbContext.Set<OutboxEntity>().Any(y =>
                                    y.Lock == x.Lock && !s_unlockedStatuses.Contains(y.Status))
                                // Note: Using explicit subquery instead of x.Parent.Status navigation property
                                // because EF Core 10 ExecuteUpdateAsync cannot properly translate queries with navigation properties
                                && (x.ParentId == null || dbContext.Set<OutboxEntity>().Any(p =>
                                    p.Id == x.ParentId && p.Status == OutboxRecordStatus.Completed))
                                && x.Status == OutboxRecordStatus.ReadyToExecute
                                && x.Version == outboxOptions.Version
                                && x.HandlerLock == null)
                    .GroupBy(x => x.Lock)
                    .Select(x => x.OrderBy(x => x.Id).FirstOrDefault().Id)
                    .OrderBy(x => x)
                    .Take(maxCount)
                    .Contains(r.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, OutboxRecordStatus.InProgress)
                .SetProperty(r => r.HandlerLock, handlerLockId)
                .SetProperty(r => r.LastUpdatedUtc, now)
            );
    }

    private async Task<int> ReserveLockedRetryOutboxRecords(Guid handlerLockId, int maxCount)
    {
        var now = DateTime.UtcNow;

        return await dbContext
            .Set<OutboxEntity>()
            .Where(
                r =>
                    r.HandlerLock == null
                    && r.Status == OutboxRecordStatus.WaitingForRetry
                    && dbContext
                        .Set<OutboxEntity>()
                        .Where(
                            x => x.Lock != null
                                 && !dbContext.Set<OutboxEntity>().Any(y => y.Lock == x.Lock && !s_unlockedStatuses.Contains(y.Status))
                                 && x.RetryTimeoutUtc < now
                                 && x.Version == outboxOptions.Version
                                 && x.HandlerLock == null)
                        .GroupBy(x => x.Lock)
                        .Select(x => x.OrderBy(x => x.Id).FirstOrDefault().Id)
                        .OrderBy(x => x)
                        .Take(maxCount)
                        .Contains(r.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, OutboxRecordStatus.InProgress)
                .SetProperty(r => r.HandlerLock, handlerLockId)
                .SetProperty(r => r.LastUpdatedUtc, now)
            );
    }

    private async Task<int> ReserveNonLockedOutboxRecords(Guid handlerLockId, int maxCount)
    {
        var now = DateTime.UtcNow;

        return await dbContext
            .Set<OutboxEntity>()
            .Where(x =>
                x.RetryTimeoutUtc < now
                && x.Version == outboxOptions.Version
                && x.HandlerLock == null
                && x.Lock == null)
            .OrderBy(x => x.Id)
            .Take(maxCount)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, OutboxRecordStatus.InProgress)
                .SetProperty(r => r.HandlerLock, handlerLockId)
                .SetProperty(r => r.LastUpdatedUtc, now)
            );
    }

    private async Task<int> ReserveNonLockedRetryOutboxRecords(Guid handlerLockId, int maxCount)
    {
        var now = DateTime.UtcNow;

        return await dbContext
            .Set<OutboxEntity>()
            .Where(
                // Note: Using explicit subquery instead of x.Parent.Status navigation property
                // because EF Core 10 ExecuteUpdateAsync cannot properly translate queries with navigation properties
                x => (x.ParentId == null || dbContext.Set<OutboxEntity>()
                         .Any(p => p.Id == x.ParentId && p.Status == OutboxRecordStatus.Completed))
                     && x.Status == OutboxRecordStatus.ReadyToExecute
                     && x.Version == outboxOptions.Version
                     && x.HandlerLock == null
                     && x.Lock == null)
            .OrderBy(x => x.Id)
            .Take(maxCount)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, OutboxRecordStatus.InProgress)
                .SetProperty(r => r.HandlerLock, handlerLockId)
                .SetProperty(r => r.LastUpdatedUtc, now)
            );
    }

    private async Task<int> ReserveStuckInProgressRecords(Guid handlerLockId, int maxCount)
    {
        if (outboxOptions.InProgressRecordTimeout == null)
            return 0;

        var stuckCutoff = DateTime.UtcNow - outboxOptions.InProgressRecordTimeout;
        var now = DateTime.UtcNow;

        return await dbContext
            .Set<OutboxEntity>()
            .Where(
                r =>
                    r.Status == OutboxRecordStatus.InProgress
                    && r.LastUpdatedUtc < stuckCutoff)
            .OrderBy(x => x.Id)
            .Take(maxCount)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.HandlerLock, handlerLockId)
                .SetProperty(r => r.LastUpdatedUtc, now)
            );
        ;
    }
}