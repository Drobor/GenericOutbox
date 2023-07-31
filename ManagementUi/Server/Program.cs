using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;

namespace ManagementUi;

public static class Extensions
{
    public static void AddOutboxManagementUi(this IServiceCollection services)
    {
        services.AddControllers();
    }

    public static void AddManagementUi(WebApplication app)
    {
        app.UseBlazorFrameworkFiles("/Outbox");
        app.UseStaticFiles();

        app.MapFallbackToFile("/Outbox", "Outbox/index.html");
    }
}