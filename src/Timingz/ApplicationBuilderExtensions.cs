using System;
using Microsoft.AspNetCore.Builder;

namespace Timingz
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseServerTiming(
            this IApplicationBuilder app,
            Action<ServerTimingOptions> configureOptions = null)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            var options = new ServerTimingOptions();
            configureOptions?.Invoke(options);

            return app.UseServerTiming(options);
        }

        public static IApplicationBuilder UseServerTiming(
            this IApplicationBuilder app,
            ServerTimingOptions options)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (options == null) throw new ArgumentNullException(nameof(options));

            return app.UseMiddleware<ServerTimingMiddleware>(options);
        }
    }
}
