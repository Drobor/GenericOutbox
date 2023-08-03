using GenericOutbox.DataAccess.Entities;

namespace GenericOutbox;

public interface IOutboxHook
{
    Task Execute(OutboxEntity outboxEntity);
}