using Microsoft.EntityFrameworkCore;

public class DbContextMigratorHostedService<TDbContext> : IHostedService where TDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;

    public DbContextMigratorHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
        // do not use async overload ot ensure migration is completed before app is started
        dbContext.Database.Migrate();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }
}