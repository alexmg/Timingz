using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Timingz;

// ReSharper disable once ClassNeverInstantiated.Global
internal partial class ServerTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ServerTimingOptions _options;
    private readonly ILogger<ServerTimingMiddleware> _logger;
    private readonly HeaderWriter _headerWriter;

    [LoggerMessage(0, LogLevel.Warning, "Metric validation failed with error: {Error}")]
    partial void LogValidationError(string error);

    [LoggerMessage(1, LogLevel.Error, "Exception was thrown invoking Server Timing callback")]
    partial void LogCallbackError(Exception exception);

    public ServerTimingMiddleware(RequestDelegate next,
        ServerTimingOptions options,
        ActivityMonitor activityMonitor,
        ILogger<ServerTimingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        _headerWriter = new HeaderWriter(options.TimingAllowOrigins, options.DurationPrecision);
        activityMonitor.Initialize(options);
    }

    // ReSharper disable once UnusedMember.Global
    public Task Invoke(
        HttpContext httpContext,
        IServerTiming serverTiming,
        IEnumerable<IServerTimingCallback> callbacks)
    {
        var callbackServices = callbacks as IServerTimingCallback[] ?? callbacks.ToArray();
        var invokeCallbacks = _options.InvokeCallbackServices && callbackServices.Length > 0;

        var requestOptions = new RequestTimingOptions();
        _options.ConfigureRequestTimingOptions(httpContext, requestOptions);

        if (!requestOptions.WriteHeader && !invokeCallbacks)
            return _next(httpContext);

        var totalMetric = serverTiming.Manual(
            _options.TotalMetricName,
            _options.TotalMetricDescription);
        totalMetric.Start();

        httpContext.Response.OnStarting(
            () => OnResponseStarting(httpContext, totalMetric, serverTiming, requestOptions));

        if (invokeCallbacks)
            httpContext.Response.OnCompleted(() => OnResponseCompleted(httpContext, serverTiming, callbackServices));

        return _next(httpContext);
    }

    internal Task OnResponseStarting(
        HttpContext httpContext,
        IManualMetric totalMetric,
        IServerTiming serverTiming,
        RequestTimingOptions requestOptions)
    {
        totalMetric.Stop();

        var metrics = serverTiming.GetMetrics();

        if (_options.ValidateMetrics && httpContext.Response.StatusCode != 500)
            ValidateMetrics(metrics);

        if (!requestOptions.WriteHeader)
            return Task.CompletedTask;

        if (!requestOptions.IncludeCustomMetrics)
            metrics = new[] { (IMetric)totalMetric };

        _headerWriter.WriteHeaders(httpContext.Response.Headers, requestOptions.IncludeDescriptions, metrics);

        return Task.CompletedTask;
    }

    private void ValidateMetrics(IReadOnlyList<IMetric> metrics)
    {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < metrics.Count; i++)
            if (metrics[i] is IValidatableMetric validatable && !validatable.Validate(out var message))
                LogValidationError(message);
    }

    internal async Task OnResponseCompleted(
        HttpContext httpContext,
        IServerTiming serverTiming,
        IReadOnlyList<IServerTimingCallback> callbacks)
    {
        var metrics = serverTiming.GetMetrics();
        var timingEvent = new ServerTimingEvent(httpContext, metrics);

        var tasks = new Task[callbacks.Count];
        for (var i = 0; i < callbacks.Count; i++)
            tasks[i] = callbacks[i].OnServerTiming(timingEvent);

        var aggregationTask = Task.WhenAll(tasks);
        try
        {
            await aggregationTask;
        }
        // ReSharper disable once EmptyGeneralCatchClause
        catch (Exception)
        {
            // AggregateException will be available below with exceptions from all tasks.
        }

        if (aggregationTask.Exception == null) return;

        var innerExceptions = aggregationTask.Exception.InnerExceptions;
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < innerExceptions.Count; i++)
            LogCallbackError(innerExceptions[i]);
    }
}