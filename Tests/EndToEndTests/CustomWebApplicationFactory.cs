using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using IStartup = TestServicesCommon.IStartup;

namespace EndToEndTests;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class, IStartup, new()
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        //builder.UseStartup<MockStartup>();
        builder = builder.UseContentRoot("");
        base.ConfigureWebHost(builder);
    }

    protected override IWebHostBuilder CreateWebHostBuilder()
    {
        var startup = new TStartup();

        return new WebHostBuilder()
            .ConfigureServices(startup.ConfigureServices)
            .Configure(startup.Configure);
    }
}