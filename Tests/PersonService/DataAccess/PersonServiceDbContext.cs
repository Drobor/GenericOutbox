using Microsoft.EntityFrameworkCore;
using GenericOutbox.DataAccess.Configuration;
using PersonService.DataAccess.Entities;

namespace PersonService.DataAccess;

public class PersonServiceDbContext : DbContext
{
    public DbSet<Person> Persons { get; set; }

    public PersonServiceDbContext(DbContextOptions<PersonServiceDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxDataEntityConfiguration());
    }
}