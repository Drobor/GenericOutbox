using GenericOutbox.DataAccess.Entities;

namespace GenericOutbox.DataAccess.Services;

public interface IOutboxDataAccess
{
    Task FailRecord(OutboxEntity outboxEntity);
    Task SendRecordToRetry(OutboxEntity outboxEntity, TimeSpan retryInterval);
    Task<OutboxEntity[]> GetOutboxRecords(int count);
    Task CompleteRecord(OutboxEntity outboxEntity);
}