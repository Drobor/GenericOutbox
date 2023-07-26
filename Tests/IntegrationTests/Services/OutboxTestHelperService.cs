namespace IntegrationTests.Services;

public class OutboxTestHelperService : IOutboxTestHelperService
{
    public async Task Delay(int ms)
        => await Task.Delay(ms);

    public void ThrowException()
        => throw new Exception();

}