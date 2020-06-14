using System;
using Microsoft.Extensions.DependencyInjection;

namespace Timingz
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServerTiming(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services.AddScoped<IServerTiming, ServerTiming>();
        }
    }
}