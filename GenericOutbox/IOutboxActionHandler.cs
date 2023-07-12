using GenericOutbox.DataAccess.Entities;

namespace GenericOutbox;

public interface IOutboxActionHandler
{
    IRetryStrategy RetryStrategy { get; }
    Task Handle(OutboxEntity entity);
}