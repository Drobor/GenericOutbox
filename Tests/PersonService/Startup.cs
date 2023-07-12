using Microsoft.EntityFrameworkCore;
using GenericOutbox;
using PersonService.DataAccess;
using PersonService.Services;
using SchoolService.Client;
using TestServicesCommon;
using IStartup = TestServicesCommon.IStartup;

namespace PersonService;

public class Startup : IStartup
{
    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddDbContext<PersonServiceDbContext>(o => o.UseSqlite("Data Source=PersonService.db"));
        services.AddSingleton<TransientErrorsMiddleware>();

        services.AddHostedService<DbContextMigratorHostedService<PersonServiceDbContext>>();

        services.AddOutbox<PersonServiceDbContext>(
            new OutboxOptions
            {
                Version = Guid.NewGuid().ToString(),
                DirectPassthrough = false,
            },
            cfg => cfg
                .Add<IOutboxedSchoolClient>()
                .UseRetryStrategy<InstantRetryStrategy>());

        ConfigureApiClients(services);
    }

    protected virtual void ConfigureApiClients(IServiceCollection services)
    {
        services.AddHttpClient<ISchoolClient, SchoolClient>
            (x => x.BaseAddress = new Uri("http://localhost:5197"));
    }

    public void Configure(IApplicationBuilder app)
    {
        //app.UseMiddleware<TransientErrorsMiddleware>();
          
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseRouting();
        app.UseEndpoints(ep => ep.MapControllers());
    }
}