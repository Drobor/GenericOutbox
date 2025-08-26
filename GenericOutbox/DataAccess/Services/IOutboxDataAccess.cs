using GenericOutbox.DataAccess.Entities;

namespace GenericOutbox.DataAccess.Services;

public interface IOutboxDataAccess
{
    Task<OutboxEntity[]> GetOutboxRecords(int maxCount);
    Task CommitExecutionResult(OutboxEntity outboxEntity, ExecutionResult executionResult);
}