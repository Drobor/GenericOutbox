using GenericOutbox;
using IntegrationTests.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IntegrationTests;

public static class TestConfiguration
{
    public static IServiceProvider TestServiceProvider { get; private set; }

    static TestConfiguration()
    {
        var services = new ServiceCollection();

        services.AddDbContext<IntegrationTestsDbContext>();

        services.AddOutbox<IntegrationTestsDbContext>(
            new OutboxOptions
            {
                Version = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            },
            x => x.Add<IOutboxedKeyStorageService>()
                .Add<IOutboxedOutboxTestHelperService>());

        services.AddScoped<IKeyStorageService, KeyStorageService>();
        services.AddScoped<IOutboxTestHelperService, OutboxTestHelperService>();
        services.AddLogging();

        TestServiceProvider = services.BuildServiceProvider();

        PrepareDatabase();

        var hostedServices = TestServiceProvider.GetRequiredService<IEnumerable<IHostedService>>();
        foreach (var hostedService in hostedServices)
            Task.Run(async () => await hostedService.StartAsync(CancellationToken.None));
    }

    private static void PrepareDatabase()
    {
        using var scope = TestServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationTestsDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.Migrate();
    }
}