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
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(r => r.RetriesCount, r => r.RetriesCount + 1)
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
            0 => await ReserveRetryOutboxRecords(lockId, maxCount),
            1 => await ReserveOutboxRecords(lockId, maxCount),
            _ => await ReserveStuckInProgressRecords(lockId, maxCount)
        };

        var stuckInProgress = _rollingOutboxQueryType == 2;

        if (recordsToUpdateCount < maxCount)
            _rollingOutboxQueryType = (_rollingOutboxQueryType + 1) % 3;

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

    private async Task<int> ReserveOutboxRecords(Guid handlerLockId, int maxCount)
    {
        var now = DateTime.UtcNow;

        return await dbContext
            .Set<OutboxEntity>()
            .Where(
                r =>
                    r.HandlerLock == null
                    && r.Status == OutboxRecordStatus.ReadyToExecute
                    && dbContext
                        .Set<OutboxEntity>()
                        .Where(
                            x => !dbContext
                                     .Set<OutboxEntity>()
                                     .Any(y => y.Lock == x.Lock && !s_unlockedStatuses.Contains(y.Status))
                                 && x.Status == OutboxRecordStatus.ReadyToExecute
                                 && x.Version == outboxOptions.Version
                                 && x.HandlerLock == null)
                        .GroupBy(x => x.Lock)
                        .Select(x => x.OrderBy(x => x.Id).FirstOrDefault().Id)
                        .OrderBy(x => x)
                        .Take(maxCount)
                        .Contains(r.Id))
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(r => r.Status, OutboxRecordStatus.InProgress)
                    .SetProperty(r => r.HandlerLock, handlerLockId)
                    .SetProperty(r => r.LastUpdatedUtc, now)
            );
    }

    private async Task<int> ReserveRetryOutboxRecords(Guid handlerLockId, int maxCount)
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
                            x => !dbContext.Set<OutboxEntity>().Any(y => y.Lock == x.Lock && !s_unlockedStatuses.Contains(y.Status))
                                 && x.RetryTimeoutUtc < now
                                 && x.Version == outboxOptions.Version
                                 && x.HandlerLock == null)
                        .GroupBy(x => x.Lock)
                        .Select(x => x.OrderBy(x => x.Id).FirstOrDefault().Id)
                        .OrderBy(x => x)
                        .Take(maxCount)
                        .Contains(r.Id))
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(r => r.Status, OutboxRecordStatus.InProgress)
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
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(r => r.HandlerLock, handlerLockId)
                    .SetProperty(r => r.LastUpdatedUtc, now)
            );
    }
}
