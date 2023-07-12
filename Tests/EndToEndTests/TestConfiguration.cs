using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PersonService;
using SchoolService;
using PersonService.Client;
using PersonService.DataAccess;
using SchoolService.Client;
using SchoolService.DataAccess;

namespace EndToEndTests;

public static class TestConfiguration
{
    public static IServiceProvider TestServiceProvider { get; private set; }

    public static WebApplicationFactory<PersonServiceTestStartup> PersonService { get; private set; }
    public static WebApplicationFactory<SchoolService.Startup> SchoolService { get; private set; }

    static TestConfiguration()
    {
        SchoolService = new CustomWebApplicationFactory<SchoolService.Startup>();
        PersonService = new CustomWebApplicationFactory<PersonServiceTestStartup>();

        var services = new ServiceCollection();
        services.AddScoped<IPersonClient>(x => new PersonClient(PersonService.CreateClient()));
        services.AddScoped<ISchoolClient>(x => new SchoolClient(SchoolService.CreateClient()));

        TestServiceProvider = services.BuildServiceProvider();
    }
}