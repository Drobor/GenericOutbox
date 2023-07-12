using Microsoft.EntityFrameworkCore;
using SchoolService.DataAccess.Entities;

namespace SchoolService.DataAccess;

public class SchoolServiceDbContext : DbContext
{
    public DbSet<Student> Students { get; set; }

    public SchoolServiceDbContext(DbContextOptions<SchoolServiceDbContext> options) : base(options)
    {
    }
}