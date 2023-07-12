using GenericOutbox.DataAccess.Entities;

namespace GenericOutbox;

public interface IRetryStrategy
{
    TimeSpan? ShouldRetry(Exception ex, OutboxEntity entity);
}