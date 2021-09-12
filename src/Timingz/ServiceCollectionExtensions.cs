using System;
using Microsoft.Extensions.DependencyInjection;

namespace Timingz
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServerTiming(this IServiceCollection services) =>
            services == null
                ? throw new ArgumentNullException(nameof(services))
                : services.AddScoped<IServerTiming, ServerTiming>()
                    .AddSingleton<ActivityMonitor>()
                    .AddHttpContextAccessor();
    }
}