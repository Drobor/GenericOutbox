using Microsoft.Extensions.DependencyInjection;

namespace GenericOutbox;

internal class OutboxActionOutboxActionHandlerFactory : IOutboxActionHandlerFactory
{
    private readonly Dictionary<string, Type> _handlers;

    public OutboxActionOutboxActionHandlerFactory(Dictionary<string, Type> handlers)
    {
        _handlers = handlers;
    }


    public IOutboxActionHandler? TryGet(IServiceProvider serviceProvider, string actionName)
    {
        if (!_handlers.TryGetValue(actionName, out var handlerType))
            return null;

        return (IOutboxActionHandler)ActivatorUtilities.CreateInstance(serviceProvider, handlerType);
    }
}