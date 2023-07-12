namespace GenericOutbox;

public interface IOutboxActionHandlerFactory
{
    public IOutboxActionHandler? TryGet(IServiceProvider serviceProvider, string actionName);
}