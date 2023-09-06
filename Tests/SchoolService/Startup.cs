using Microsoft.EntityFrameworkCore;
using SchoolService.DataAccess;
using IStartup = TestServicesCommon.IStartup;

namespace SchoolService;

public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddDbContext<SchoolServiceDbContext>(o => o.UseSqlite("Data Source=SchoolService.db"));
        services.AddSingleton<TransientErrorsMiddleware>();

        services.AddHostedService<DbContextMigratorHostedService<SchoolServiceDbContext>>();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseMiddleware<TransientErrorsMiddleware>();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseRouting();
        app.UseEndpoints(ep => ep.MapControllers());
    }
}