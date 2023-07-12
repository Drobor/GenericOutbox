namespace GenericOutbox.Enums;

public enum OutboxRecordStatus
{
    ReadyToExecute = 0,
    InProgress = 1,
    WaitingForRetry = 2,
    Failed = 3,
    Completed = 4,
}