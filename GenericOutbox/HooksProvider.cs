using GenericOutbox.DataAccess.Entities;

namespace GenericOutbox;

public class HooksProvider
{
    public HooksProvider(Func<OutboxEntity, Task>[] hooks)
    {
        this.Hooks = hooks;
    }

    public Func<OutboxEntity, Task>[] Hooks { get; private set; } 
}