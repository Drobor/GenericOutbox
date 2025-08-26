using GenericOutbox.Enums;

namespace GenericOutbox;

public abstract record ExecutionResult
{
    public static ExecutionResult Success { get; } = new SuccessResult();
    public static ExecutionResult RetryAfter(TimeSpan retryInterval) => new RetryResult(DateTime.UtcNow + retryInterval);
    public static ExecutionResult Fail { get; } = new FailedResult();

    public abstract OutboxRecordStatus Status { get; internal set; }
    public abstract DateTime? RetryTimeoutUtc { get; internal set; }

    private sealed record SuccessResult : ExecutionResult
    {
        public override OutboxRecordStatus Status { get; internal set; } = OutboxRecordStatus.Completed;
        public override DateTime? RetryTimeoutUtc { get; internal set; } = null;
    }

    private sealed record FailedResult : ExecutionResult
    {
        public override OutboxRecordStatus Status { get; internal set; } = OutboxRecordStatus.Failed;
        public override DateTime? RetryTimeoutUtc { get; internal set; } = null;
    }

    private sealed record RetryResult : ExecutionResult
    {
        public override OutboxRecordStatus Status { get; internal set; } = OutboxRecordStatus.WaitingForRetry;
        public override DateTime? RetryTimeoutUtc { get; internal set; }

        internal RetryResult(DateTime? retryTimeoutUtc)
        {
            RetryTimeoutUtc = retryTimeoutUtc;
        }
    }
}