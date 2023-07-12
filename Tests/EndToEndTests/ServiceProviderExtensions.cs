using Microsoft.Extensions.DependencyInjection;

namespace EndToEndTests;

public static class ServiceProviderExtensions
{
    public static void Resolve<T1>(this IServiceProvider serviceProvider, out T1 v1)
    {
        v1 = serviceProvider.GetRequiredService<T1>();
    }

    public static void Resolve<T1, T2>(this IServiceProvider serviceProvider, out T1 v1, out T2 v2)
    {
        v1 = serviceProvider.GetRequiredService<T1>();
        v2 = serviceProvider.GetRequiredService<T2>();
    }

    public static void Resolve<T1, T2, T3>(this IServiceProvider serviceProvider, out T1 v1, out T2 v2, out T3 v3)
    {
        v1 = serviceProvider.GetRequiredService<T1>();
        v2 = serviceProvider.GetRequiredService<T2>();
        v3 = serviceProvider.GetRequiredService<T3>();
    }

    public static void Resolve<T1, T2, T3, T4>(this IServiceProvider serviceProvider, out T1 v1, out T2 v2, out T3 v3, out T4 v4)
    {
        v1 = serviceProvider.GetRequiredService<T1>();
        v2 = serviceProvider.GetRequiredService<T2>();
        v3 = serviceProvider.GetRequiredService<T3>();
        v4 = serviceProvider.GetRequiredService<T4>();
    }
}