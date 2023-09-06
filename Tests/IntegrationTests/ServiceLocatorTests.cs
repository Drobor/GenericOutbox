using IntegrationTests.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests;

public class ServiceLocatorTests : DependencyInjectionTestBase
{
    private readonly IOutboxedKeyStorageServiceHelper _outboxedKeyStorageServiceHelper;
    private readonly IKeyStorageService _keyStorageService;

    public ServiceLocatorTests()
    {
        _outboxedKeyStorageServiceHelper = ServiceProvider.GetRequiredService<IOutboxedKeyStorageServiceHelper>();
        _keyStorageService = ServiceProvider.GetRequiredService<IKeyStorageService>();
    }

    [Fact]
    public async Task BasicFlowTest()
    {
        var key = Guid.NewGuid().ToString();

        _outboxedKeyStorageServiceHelper.AddUsingScope(key);
        await Task.Delay(700);

        Assert.True(_keyStorageService.Contains(key));
    }
}
