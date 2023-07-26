using GenericOutbox.DataAccess.Entities;

namespace GenericOutbox.DataAccess.Services;

public interface IOutboxDataAccess
{
    Task FailRecord(OutboxEntity outboxEntity);
    Task SendRecordToRetry(OutboxEntity outboxEntity, TimeSpan retryInterval);
    Task<OutboxEntity[]> GetOutboxRecords(int maxCount);
    Task CompleteRecord(OutboxEntity outboxEntity);
}