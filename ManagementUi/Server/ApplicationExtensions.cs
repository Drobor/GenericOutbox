using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenericOutbox.ManagementUi;

public static class ApplicationExtensions
{
    public static void AddOutboxManagementUi<TDbContext>(this IServiceCollection services) where TDbContext: DbContext
    {
        services.AddControllers();
        services.AddScoped<IOutboxDbContextProvider, OutboxDbContextProvider<TDbContext>>();
    }

    public static void AddOutboxManagementUi(this WebApplication app)
    {
        app.UseBlazorFrameworkFiles("/Outbox");
        app.UseStaticFiles();
        app.UseRouting();
        app.MapControllers();
        app.MapFallbackToFile("/Outbox/{**slug}", "Outbox/index.html");
    }
}