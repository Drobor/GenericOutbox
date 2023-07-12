using Microsoft.EntityFrameworkCore;
using GenericOutbox.DataAccess.Configuration;
using GenericOutbox.DataAccess.Entities;

class TestDbContext : DbContext
{
    public DbSet<OutboxEntity> OutboxEntities { get; set; }

    public TestDbContext()
    {

    }

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxEntityConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}