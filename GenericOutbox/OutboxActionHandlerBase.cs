using GenericOutbox.DataAccess.Entities;

namespace GenericOutbox;

public abstract class OutboxActionHandlerBase<TArg> : IOutboxActionHandler
{
    private readonly ISerializer _serializer;
    private readonly IOutboxDataStorageService _outboxDataStorageService;

    public IRetryStrategy RetryStrategy { get; }

    protected OutboxActionHandlerBase(ISerializer serializer, IRetryStrategy retryStrategy, IOutboxDataStorageService outboxDataStorageService)
    {
        _serializer = serializer;
        RetryStrategy = retryStrategy;
        _outboxDataStorageService = outboxDataStorageService;
    }

    public async Task Handle(OutboxEntity entity)
    {
        await Handle(_serializer.Deserialize<TArg>(entity.Payload));
    }

    protected abstract Task Handle(TArg payload);
}

public abstract class OutboxActionHandlerBase<TArg, TResult> : IOutboxActionHandler
{
    private readonly ISerializer _serializer;
    private readonly IOutboxDataStorageService _outboxDataStorageService;

    public IRetryStrategy RetryStrategy { get; }

    protected OutboxActionHandlerBase(ISerializer serializer, IRetryStrategy retryStrategy, IOutboxDataStorageService outboxDataStorageService)
    {
        _serializer = serializer;
        RetryStrategy = retryStrategy;
        _outboxDataStorageService = outboxDataStorageService;
    }

    public async Task Handle(OutboxEntity entity)
    {
        var result = await Handle(_serializer.Deserialize<TArg>(entity.Payload));
        await _outboxDataStorageService.StoreOutboxStepResult(entity.Action, result);
    }

    protected abstract Task<TResult> Handle(TArg payload);
}