using GenericOutbox;
using GenericOutbox.DataAccess.Entities;

namespace PersonService;

public class TestHook : IOutboxHook
{
    public async Task Execute(OutboxEntity outboxEntity)
    {
        Console.WriteLine($"TestHook. Action: {outboxEntity.Action}");
    }
}