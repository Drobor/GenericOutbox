// See https://aka.ms/new-console-template for more information

using System.Drawing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GenericOutbox;
using Test;

class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var services = new ServiceCollection();
        ConfigureServices(services);

        var host = CreateHostBuilder(args).Build();
        var runTask = Task.Run(async () => await host.RunAsync());

        var rootSp = host.Services;

        //rootSp.GetServices<>

        using (var scope = rootSp.CreateScope())
        {
            var sp = scope.ServiceProvider;

            var dbContext = sp.GetRequiredService<TestDbContext>();
            await dbContext.Database.MigrateAsync();

            var qwe = sp.GetRequiredService<IOutboxTestInterface>();
            await qwe.WithoutReturnType(new Point(1, 2), new[] { 1, 2, 34 });
        }

        await runTask;
    }

    static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ITestInterface, TestInterface>();
        services.AddDbContext<TestDbContext>(q => q.UseSqlite("Data Source=sqlite.db"));
        services.AddOutbox<TestDbContext>(
            new OutboxOptions
            {
                Version = "2023-04-15-000"
            },
            outboxServices => outboxServices.Add<IOutboxTestInterface>());
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
            .ConfigureServices(ConfigureServices);
}