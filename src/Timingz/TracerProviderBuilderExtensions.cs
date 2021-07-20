using System;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace Timingz
{
    public static class TracerProviderBuilderExtensions
    {
        public static TracerProviderBuilder AddServerTimingProcessor(this TracerProviderBuilder builder)
        {
            return builder switch
            {
                null => throw new ArgumentNullException(nameof(builder)),
                IDeferredTracerProviderBuilder deferredTracerProviderBuilder => deferredTracerProviderBuilder.Configure(
                    (sp, providerBuilder) =>
                    {
                        var processor = sp.GetRequiredService<ServerTimingProcessor>();
                        providerBuilder.AddProcessor(processor);
                    }),
                _ => throw new NotSupportedException(
                    "The builder must implement IDeferredTracerProviderBuilder for the ServerTiming processor to be added.")
            };
        }
    }
}