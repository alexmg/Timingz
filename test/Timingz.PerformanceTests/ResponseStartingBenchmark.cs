using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;

namespace Timingz.PerformanceTests;

public class ResponseStartingBenchmark
{
    private HttpContext _httpContext;
    private IServerTiming _serverTiming;
    private ServerTimingMiddleware _middleware;
    private ServerTimingOptions _serverTimingOptions;
    private RequestTimingOptions _requestTimingOptions;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _httpContext = new DefaultHttpContext();
        _serverTiming = Factory.CreateServerTiming();

        _serverTimingOptions = new ServerTimingOptions();
        _requestTimingOptions = new RequestTimingOptions { WriteHeader = true };
        _middleware = new ServerTimingMiddleware(_ => Task.CompletedTask, _serverTimingOptions,
            new ActivityMonitor(null), null);
    }

    [Params(true, false)]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public bool IncludeCustomMetrics { get; set; }

    [Params(true, false)]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public bool IncludeDescriptions { get; set; }

    [Params(true, false)]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public bool ValidateMetrics { get; set; }

    [Benchmark]
    public Task OnResponseStarting()
    {
        _requestTimingOptions.IncludeCustomMetrics = IncludeCustomMetrics;
        _serverTimingOptions.ValidateMetrics = ValidateMetrics;
        _requestTimingOptions.IncludeDescriptions = IncludeDescriptions;

        _httpContext.Response.Headers.Clear();
        var totalMetric = new ManualMetric(_serverTimingOptions.TotalMetricName);
        totalMetric.Start();

        return _middleware.OnResponseStarting(_httpContext, totalMetric, _serverTiming, _requestTimingOptions);
    }
}