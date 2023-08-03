using GenericOutbox.DataAccess.Entities;

namespace GenericOutbox;

public class HooksProvider
{
    public HooksProvider(IEnumerable<IOutboxHook> hooks)
    {
        this.Hooks = hooks;
    }

    public IEnumerable<IOutboxHook> Hooks { get; private set; } 
}