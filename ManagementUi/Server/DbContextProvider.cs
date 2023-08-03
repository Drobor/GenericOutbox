using Microsoft.EntityFrameworkCore;

namespace GenericOutbox.ManagementUi;

public interface IOutboxDbContextProvider
{
    DbContext DbContext{ get; }
}

internal class OutboxDbContextProvider<TDbContext> : IOutboxDbContextProvider where TDbContext : DbContext
{
    public OutboxDbContextProvider(TDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public DbContext DbContext { get; init; }
}