using GenericOutbox.DataAccess.Entities;
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

    public OutboxDataAccess(TDbContext dbContext, OutboxOptions outboxOptions)
    {
        _dbContext = dbContext;
        _outboxOptions = outboxOptions;
    }

    public async Task CompleteRecord(OutboxEntity outboxEntity)
    {
        var now = DateTime.UtcNow;

        await _dbContext
            .Set<OutboxEntity>()
            .Where(x => x.Id == outboxEntity.Id)
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(r => r.Status, OutboxRecordStatus.Completed)
                    .SetProperty(r => r.LastUpdatedUtc, r => now)
                    .SetProperty(r => r.HandlerLock, r => null)
                    .SetProperty(r => r.RetryTimeoutUtc, r => null)); //todo: handleResponse?
    }

    public async Task FailRecord(OutboxEntity outboxEntity)
    {
        var now = DateTime.UtcNow;

        await _dbContext
            .Set<OutboxEntity>()
            .Where(x => x.Id == outboxEntity.Id)
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(r => r.Status, OutboxRecordStatus.Failed)
                    .SetProperty(r => r.LastUpdatedUtc, r => now)
                    .SetProperty(r => r.HandlerLock, r => null)
                    .SetProperty(r => r.RetryTimeoutUtc, r => null));
    }

    public async Task SendRecordToRetry(OutboxEntity outboxEntity, TimeSpan retryInterval)
    {
        var now = DateTime.UtcNow;
        var retryTime = now + retryInterval;

        await _dbContext
            .Set<OutboxEntity>()
            .Where(x => x.Id == outboxEntity.Id)
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(r => r.RetriesCount, r => r.RetriesCount + 1)
                    .SetProperty(r => r.Status, OutboxRecordStatus.WaitingForRetry)
                    .SetProperty(r => r.RetryTimeoutUtc, r => retryTime)
                    .SetProperty(r => r.LastUpdatedUtc, r => now)
                    .SetProperty(r => r.HandlerLock, r => null));
    }

    public async Task<OutboxEntity[]> GetOutboxRecords(int count)
    {
        Guid lockId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var recordsWithLock = _dbContext.Set<OutboxEntity>()
            .Where(
                r => _dbContext
                    .Set<OutboxEntity>()
                    .Where(
                        x => x.Lock != null
                             && !_dbContext.Set<OutboxEntity>().Any(y => y.Lock == x.Lock && !s_unlockedStatuses.Contains(y.Status)))
                    .GroupBy(x => x.Lock)
                    .Select(x => x.FirstOrDefault().Id)
                    .Contains(r.Id));

        var recordsWithoutLock = _dbContext
            .Set<OutboxEntity>()
            .Where(x => x.Lock == null);

        var recordsToUpdateCount = await recordsWithLock.Union(recordsWithoutLock)
            .Where(
                x => (x.ParentId == null || x.Parent.Status == OutboxRecordStatus.Completed)
                     && x.Status == OutboxRecordStatus.ReadyToExecute
                     && x.Version == _outboxOptions.Version
                     && x.HandlerLock == null
                     ||
                     x.RetryTimeoutUtc < now
                     && x.Version == _outboxOptions.Version
                     && x.HandlerLock == null)
            .Take(count)
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(r => r.Status, r => OutboxRecordStatus.InProgress)
                    .SetProperty(r => r.HandlerLock, r => lockId));

        if (recordsToUpdateCount == 0)
            return Array.Empty<OutboxEntity>();

        return await _dbContext
            .Set<OutboxEntity>()
            .AsNoTracking()
            .Where(x => x.HandlerLock == lockId)
            .ToArrayAsync();
    }
}