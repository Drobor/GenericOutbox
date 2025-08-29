using GenericOutbox.DataAccess.Entities;
using GenericOutbox.DataAccess.Models;
using GenericOutbox.Enums;
using Microsoft.EntityFrameworkCore;

namespace GenericOutbox.DataAccess.Services;

public class OutboxDataAccess<TDbContext> : IOutboxDataAccess where TDbContext : DbContext
{
    private static readonly OutboxRecordStatus[] s_unlockedStatuses = new[]
    {
        OutboxRecordStatus.ReadyToExecute,
        OutboxRecordStatus.WaitingForRetry,
        OutboxRecordStatus.Completed
    };

    private readonly TDbContext _dbContext;
    private readonly OutboxOptions _outboxOptions;

    private int _rollingOutboxQueryType = 0; //0 = non-locked, 1 = locked

    public OutboxDataAccess(TDbContext dbContext, OutboxOptions outboxOptions)
    {
        _dbContext = dbContext;
        _outboxOptions = outboxOptions;
    }

    public async Task CommitExecutionResult(OutboxEntity outboxEntity, ExecutionResult executionResult)
    {
        var now = DateTime.UtcNow;

        await _dbContext
            .Set<OutboxEntity>()
            .Where(x => x.Id == outboxEntity.Id)
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(r => r.RetriesCount, r => r.RetriesCount + 1)
                    .SetProperty(r => r.Status, executionResult.Status)
                    .SetProperty(r => r.RetryTimeoutUtc, r => executionResult.RetryTimeoutUtc)
                    .SetProperty(r => r.LastUpdatedUtc, r => now)
                    .SetProperty(r => r.HandlerLock, r => null));
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

        var result = await _dbContext
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

        return await _dbContext
            .Set<OutboxEntity>()
            .Where(
                r =>
                    r.HandlerLock == null
                    && r.Status == OutboxRecordStatus.ReadyToExecute
                    && _dbContext
                        .Set<OutboxEntity>()
                        .Where(
                            x => x.Lock != null
                                 && !_dbContext.Set<OutboxEntity>().Any(y => y.Lock == x.Lock && !s_unlockedStatuses.Contains(y.Status))
                                 && (x.ParentId == null || x.Parent.Status == OutboxRecordStatus.Completed)
                                 && x.Status == OutboxRecordStatus.ReadyToExecute
                                 && x.Version == _outboxOptions.Version
                                 && x.HandlerLock == null)
                        .GroupBy(x => x.Lock)
                        .Select(x => x.OrderBy(x => x.Id).FirstOrDefault().Id)
                        .Take(maxCount)
                        .Contains(r.Id))
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(r => r.Status, r => OutboxRecordStatus.InProgress)
                    .SetProperty(r => r.HandlerLock, r => handlerLockId)
                    .SetProperty(r => r.LastUpdatedUtc, r => now));
    }

    private async Task<int> ReserveLockedRetryOutboxRecords(Guid handlerLockId, int maxCount)
    {
        var now = DateTime.UtcNow;

        return await _dbContext
            .Set<OutboxEntity>()
            .Where(
                r =>
                    r.HandlerLock == null
                    && r.Status == OutboxRecordStatus.WaitingForRetry
                    && _dbContext
                        .Set<OutboxEntity>()
                        .Where(
                            x => x.Lock != null
                                 && !_dbContext.Set<OutboxEntity>().Any(y => y.Lock == x.Lock && !s_unlockedStatuses.Contains(y.Status))
                                 && x.RetryTimeoutUtc < now
                                 && x.Version == _outboxOptions.Version
                                 && x.HandlerLock == null)
                        .GroupBy(x => x.Lock)
                        .Select(x => x.OrderBy(x => x.Id).FirstOrDefault().Id)
                        .Take(maxCount)
                        .Contains(r.Id))
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(r => r.Status, r => OutboxRecordStatus.InProgress)
                    .SetProperty(r => r.HandlerLock, r => handlerLockId)
                    .SetProperty(r => r.LastUpdatedUtc, r => now));
    }

    private async Task<int> ReserveNonLockedOutboxRecords(Guid handlerLockId, int maxCount)
    {
        var now = DateTime.UtcNow;

        return await _dbContext
            .Set<OutboxEntity>()
            .Where(
                x =>
                    x.RetryTimeoutUtc < now
                    && x.Version == _outboxOptions.Version
                    && x.HandlerLock == null
                    && x.Lock == null)
            .Take(maxCount)
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(r => r.Status, r => OutboxRecordStatus.InProgress)
                    .SetProperty(r => r.HandlerLock, r => handlerLockId)
                    .SetProperty(r => r.LastUpdatedUtc, r => now));
    }

    private async Task<int> ReserveNonLockedRetryOutboxRecords(Guid handlerLockId, int maxCount)
    {
        var now = DateTime.UtcNow;

        return await _dbContext
            .Set<OutboxEntity>()
            .Where(
                x => (x.ParentId == null || x.Parent.Status == OutboxRecordStatus.Completed)
                     && x.Status == OutboxRecordStatus.ReadyToExecute
                     && x.Version == _outboxOptions.Version
                     && x.HandlerLock == null
                     && x.Lock == null)
            .Take(maxCount)
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(r => r.Status, r => OutboxRecordStatus.InProgress)
                    .SetProperty(r => r.HandlerLock, r => handlerLockId)
                    .SetProperty(r => r.LastUpdatedUtc, r => now));
    }

    private async Task<int> ReserveStuckInProgressRecords(Guid handlerLockId, int maxCount)
    {
        if (_outboxOptions.InProgressRecordTimeout == null)
            return 0;

        var now = DateTime.UtcNow;

        var stuckCutoff = DateTime.UtcNow - _outboxOptions.InProgressRecordTimeout;

        return await _dbContext
            .Set<OutboxEntity>()
            .Where(
                r =>
                    r.Status == OutboxRecordStatus.InProgress
                    && r.LastUpdatedUtc < stuckCutoff)
            .Take(maxCount)
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(r => r.HandlerLock, r => handlerLockId)
                    .SetProperty(r => r.LastUpdatedUtc, r => now));
    }
}