using GenericOutbox.DataAccess.Entities;
using GenericOutbox.DataAccess.Models;

namespace GenericOutbox.DataAccess.Services;

public interface IOutboxDataAccess
{
    Task<OutboxEntityDispatchModel[]> GetOutboxRecords(int maxCount);
    Task CommitExecutionResult(OutboxEntity outboxEntity, ExecutionResult executionResult);
}