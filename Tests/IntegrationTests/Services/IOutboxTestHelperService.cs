
using GenericOutbox;

namespace IntegrationTests.Services;

[OutboxInterface]
public interface IOutboxedOutboxTestHelperService : IOutboxTestHelperService
{
}

public interface IOutboxTestHelperService
{
    Task Delay(int ms);
    void ThrowException();
    Task ThrowToSuccess();
    Task ThrowToFail();
    Task ThrowToRetry();
    Task StuckInProgressToComplete();
    Task StuckInProgressToFail();
    Task StuckInProgressToRetry();
}