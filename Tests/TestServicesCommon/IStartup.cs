namespace TestServicesCommon;

public interface IStartup
{
    void ConfigureServices(IServiceCollection services);
    void Configure(IApplicationBuilder app);
}