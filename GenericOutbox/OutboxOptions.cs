namespace GenericOutbox;

public class OutboxOptions
{
    public int DispatcherDbPollingDelayMs { get; set; } = 100;
    public int HandlerThreadsCount { get; set; } = Environment.ProcessorCount;
    public int BufferizedOutboxRecordsCount { get; set; }
    public string Version { get; set; } = "DefaultVersion";
    public bool DirectPassthrough { get; set; }

    /// <summary>
    /// Timeout after which InProgress outbox records are considered stuck and will be evaluated for retry with InProgressTimeoutException.
    /// </summary>
    public TimeSpan? InProgressRecordTimeout { get; set; } = null;
}