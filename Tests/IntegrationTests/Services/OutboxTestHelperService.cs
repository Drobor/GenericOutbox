namespace IntegrationTests.Services;

public class OutboxTestHelperService : IOutboxTestHelperService
{
    public async Task Delay(int ms)
        => await Task.Delay(ms);

    public void ThrowException()
        => throw new Exception();

    public Task ThrowToSuccess()
        => throw new Exception();

    public Task ThrowToFail()
        => throw new Exception();

    public Task ThrowToRetry()
        => throw new Exception();

    public async Task StuckInProgressToComplete()
        => await Task.Delay(9999999);

    public async Task StuckInProgressToFail()
        => await Task.Delay(9999999);

    public async Task StuckInProgressToRetry()
        => await Task.Delay(9999999);
}