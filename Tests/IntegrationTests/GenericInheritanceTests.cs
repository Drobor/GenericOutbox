using IntegrationTests.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests;

public class GenericInheritanceTests : DependencyInjectionTestBase
{
    private readonly IGuidKeyStorageService _guidKeyStorageService;
    private readonly IOutboxedGuidKeyStorageService _outboxedGuidKeyStorageService;

    public GenericInheritanceTests()
    {
        _guidKeyStorageService = ServiceProvider.GetRequiredService<IGuidKeyStorageService>();
        _outboxedGuidKeyStorageService = ServiceProvider.GetRequiredService<IOutboxedGuidKeyStorageService>();
    }

    [Fact]
    public async Task BasicTest()
    {
        var myGuid = Guid.NewGuid();
        _outboxedGuidKeyStorageService.Add(myGuid);
        await Task.Delay(700);
        Assert.True(_guidKeyStorageService.Contains(myGuid));
    }
}