using Microsoft.AspNetCore.Builder;

namespace Timingz;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseServerTiming(
        this IApplicationBuilder app,
        Action<ServerTimingOptions> configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = new ServerTimingOptions();
        configureOptions?.Invoke(options);

        return app.UseServerTiming(options);
    }

    public static IApplicationBuilder UseServerTiming(
        this IApplicationBuilder app,
        ServerTimingOptions options)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(options);

        return app.UseMiddleware<ServerTimingMiddleware>(options);
    }
}