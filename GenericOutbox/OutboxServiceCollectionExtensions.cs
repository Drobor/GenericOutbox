using System.Reflection;
using GenericOutbox.DataAccess.Entities;
using GenericOutbox.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenericOutbox;

public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddOutbox<TDbContext>(this IServiceCollection services, OutboxOptions options, Action<OutboxSettingsBuilder> configurator) where TDbContext : DbContext
    {
        services.AddScoped<IOutboxDataAccess, OutboxDataAccess<TDbContext>>();
        services.AddScoped<IOutboxCreatorContext, OutboxCreatorContext<TDbContext>>();
        services.AddScoped<IOutboxDataStorageService, OutboxDataStorageService<TDbContext>>();
        services.AddScoped<IOutboxHandlerContext, OutboxHandlerContext>();


        services.AddHostedService<OutboxDispatcherHostedService>();

        var outboxSettings = new OutboxSettingsBuilder();
        configurator(outboxSettings);

        var actionNameMap = new Dictionary<string, Type>();

        foreach (var type in outboxSettings.OutboxTypes)
        {
            var implType = type.Assembly.ExportedTypes.Single( //todo: Optimize this to be O(N) instead of O(N*N)?
                x => type.IsAssignableFrom(x)
                     && x.GetFields(BindingFlags.Public | BindingFlags.Static).Length == 1);

            var handlers = (Dictionary<string, Type>)implType.GetFields(BindingFlags.Public | BindingFlags.Static)[0].GetValue(null)!;

            foreach (var handler in handlers)
                actionNameMap[handler.Key] = handler.Value;

            services.AddScoped(type, implType);
        }

        services.AddSingleton<IOutboxActionHandlerFactory>(new OutboxActionOutboxActionHandlerFactory(actionNameMap));
        services.AddSingleton(options);
        services.AddSingleton(typeof(ISerializer), outboxSettings.SerializerType);
        services.AddScoped(typeof(IRetryStrategy), outboxSettings.RetryStrategyType);
        services.AddSingleton(new HooksProvider(outboxSettings.Hooks));

        return services;
    }

    public class OutboxSettingsBuilder
    {
        internal List<Type> OutboxTypes = new List<Type>();
        internal Type RetryStrategyType = typeof(RetryStrategy);
        internal Type SerializerType = typeof(JsonOutboxSerializer);
        internal Func<OutboxEntity, Task>[] Hooks = Array.Empty<Func<OutboxEntity, Task>>();

        public OutboxSettingsBuilder Add<T>()
        {
            OutboxTypes.Add(typeof(T));
            return this;
        }

        public OutboxSettingsBuilder UseRetryStrategy<T>() where T : IRetryStrategy
        {
            RetryStrategyType = typeof(T);
            return this;
        }

        public OutboxSettingsBuilder UseSerializer<T>() where T : ISerializer
        {
            SerializerType = typeof(T);
            return this;
        }
        
        public OutboxSettingsBuilder UseHooks(params Func<OutboxEntity, Task>[] hooks)
        {
            this.Hooks = hooks;
            return this;
        }
    }
}