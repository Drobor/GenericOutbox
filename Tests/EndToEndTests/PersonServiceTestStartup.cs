using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SchoolService.Client;

namespace EndToEndTests;

public class PersonServiceTestStartup : PersonService.Startup
{

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
    }

    protected override void ConfigureApiClients(IServiceCollection services)
    {
        services.AddScoped<ISchoolClient>(sp => new SchoolClient(TestConfiguration.SchoolService.CreateClient()));
    }
}