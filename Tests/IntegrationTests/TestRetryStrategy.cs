using GenericOutbox;
using GenericOutbox.DataAccess.Entities;
using GenericOutbox.Exceptions;
using IntegrationTests.Services;

namespace IntegrationTests;

public class TestRetryStrategy : IRetryStrategy
{
    private readonly RetryStrategy _retryStrategy = new RetryStrategy();

    public ExecutionResult HandleError(Exception ex, OutboxEntity entity)
    {
        if (entity.Action.Contains(nameof(IOutboxTestHelperService.ThrowToFail)))
            return ExecutionResult.Fail;

        if (entity.Action.Contains(nameof(IOutboxTestHelperService.ThrowToSuccess)))
            return ExecutionResult.Success;

        if (entity.Action.Contains(nameof(IOutboxTestHelperService.ThrowToRetry)))
            return ExecutionResult.RetryAfter(TimeSpan.FromDays(36500));

        if (ex.GetType() == typeof(InProgressTimeoutException))
        {
            if (entity.Action.Contains(nameof(IOutboxTestHelperService.StuckInProgressToFail)))
                return ExecutionResult.Fail;

            if (entity.Action.Contains(nameof(IOutboxTestHelperService.StuckInProgressToComplete)))
                return ExecutionResult.Success;

            if (entity.Action.Contains(nameof(IOutboxTestHelperService.StuckInProgressToRetry)))
                return ExecutionResult.RetryAfter(TimeSpan.FromDays(36500));
        }

        return _retryStrategy.HandleError(ex, entity);
    }
}