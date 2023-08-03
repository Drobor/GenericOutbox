using GenericOutbox.DataAccess.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GenericOutbox.ManagementUi.App;

public class ManagementUiDbContext : DbContext
{
    public ManagementUiDbContext(DbContextOptions<ManagementUiDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxDataEntityConfiguration());
    }
}