using System;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace Timingz
{
    public static class TracerProviderBuilderExtensions
    {
        public static TracerProviderBuilder AddServerTimingProcessor(this TracerProviderBuilder builder)
        {
            switch (builder)
            {
                case null:
                    throw new ArgumentNullException(nameof(builder));
                case IDeferredTracerProviderBuilder deferredTracerProviderBuilder:
                    deferredTracerProviderBuilder.Services.AddHttpContextAccessor();
                    deferredTracerProviderBuilder.Services.AddSingleton<ServerTimingProcessor>();
                    return deferredTracerProviderBuilder.Configure((sp, providerBuilder) =>
                    {
                        var processor = sp.GetRequiredService<ServerTimingProcessor>();
                        providerBuilder.AddProcessor(processor);
                    });
                default:
                    throw new NotSupportedException(
                        "The builder must implement IDeferredTracerProviderBuilder for the ServerTiming processor to be added.");
            }
        }
    }
}