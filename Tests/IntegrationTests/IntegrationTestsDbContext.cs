using System.Data.Common;
using GenericOutbox.DataAccess.Configuration;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests;

public class IntegrationTestsDbContext : DbContext
{
    public IntegrationTestsDbContext()
    {

    }

    public IntegrationTestsDbContext(DbContextOptions<IntegrationTestsDbContext> options) : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=IntegrationTestsSqlite.db");
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxDataEntityConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}