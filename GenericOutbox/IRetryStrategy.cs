using GenericOutbox.DataAccess.Entities;

namespace GenericOutbox;

public interface IRetryStrategy
{
    ExecutionResult HandleError(Exception ex, OutboxEntity entity);
}