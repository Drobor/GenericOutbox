using System.Threading.Channels;
using GenericOutbox.DataAccess.Entities;
using GenericOutbox.DataAccess.Services;
using GenericOutbox.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GenericOutbox;

public class OutboxDispatcherHostedService : IHostedService
{
    private static readonly RetryStrategy s_defaultRetryStrategy = new RetryStrategy();

    private readonly OutboxOptions _outboxOptions;
    private readonly IOutboxActionHandlerFactory _outboxActionHandlerFactory;
    private readonly ILogger<OutboxDispatcherHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;

    private readonly Channel<OutboxEntity> _recordsChannel;
    private readonly CancellationToken _cancellationToken;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly List<Task> _workers;

    private int _waitingHandlersCount;

    public OutboxDispatcherHostedService(IServiceProvider serviceProvider, ILogger<OutboxDispatcherHostedService> logger, OutboxOptions outboxOptions, IOutboxActionHandlerFactory outboxActionHandlerFactory)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _outboxOptions = outboxOptions;
        _outboxActionHandlerFactory = outboxActionHandlerFactory;

        _recordsChannel = Channel.CreateUnbounded<OutboxEntity>();
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
        _workers = new List<Task>();

        _waitingHandlersCount = _outboxOptions.HandlerThreadsCount;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();

        _workers.Add(Task.Run(DispatcherLoop));

        for (int i = 0; i < _outboxOptions.HandlerThreadsCount; i++)
            _workers.Add(Task.Run(HandlerLoop));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        await Task.WhenAll(_workers);
    }

    private async Task DispatcherLoop()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var outboxDataAccess = scope.ServiceProvider.GetRequiredService<IOutboxDataAccess>();

            while (!_cancellationToken.IsCancellationRequested)
            {
                var recordsToReadCount = _waitingHandlersCount + _outboxOptions.BufferizedOutboxRecordsCount - _recordsChannel.Reader.Count;

                if (recordsToReadCount > 0)
                {
                    try
                    {
                        foreach (var outboxRecord in await outboxDataAccess.GetOutboxRecords(recordsToReadCount))
                            _recordsChannel.Writer.TryWrite(outboxRecord);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while getting outbox records");
                        await Task.Delay(1000, _cancellationToken);
                    }
                }

                await Task.Delay(_outboxOptions.DispatcherDbPollingDelayMs, _cancellationToken);
            }
        }
        finally
        {
            _recordsChannel.Writer.Complete();
        }
    }

    private async Task HandlerLoop()
    {
        await foreach (var outboxRecord in _recordsChannel.Reader.ReadAllAsync(_cancellationToken))
        {
            Interlocked.Decrement(ref _waitingHandlersCount);

            IOutboxActionHandler? handler = null;

            using var scope = _serviceProvider.CreateScope();
            var outboxDataAccess = scope.ServiceProvider.GetRequiredService<IOutboxDataAccess>();
            var outboxHandlerContext = scope.ServiceProvider.GetRequiredService<IOutboxHandlerContext>();

            try
            {
                outboxHandlerContext.ScopeId = outboxRecord.ScopeId;
                handler = _outboxActionHandlerFactory.TryGet(scope.ServiceProvider, outboxRecord.Action);

                if (handler == null)
                    throw new OutboxHandlerNotFoundException($"Handler for action {outboxRecord.Action} not found");

                await handler.Handle(outboxRecord);

                await outboxDataAccess.CompleteRecord(outboxRecord);
            }
            catch (Exception ex)
            {
                var retryStrategy = handler?.RetryStrategy ?? s_defaultRetryStrategy;
                var shouldRetry = retryStrategy.ShouldRetry(ex, outboxRecord);

                if (shouldRetry.HasValue)
                {
                    await outboxDataAccess.SendRecordToRetry(outboxRecord, shouldRetry.Value);
                }
                else
                {
                    await outboxDataAccess.FailRecord(outboxRecord);
                }
            }
            finally
            {
                Interlocked.Increment(ref _waitingHandlersCount);
            }
        }
    }
}