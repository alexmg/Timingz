using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;

namespace Timingz.PerformanceTests;

public class ResponseCompletedBenchmark
{
    private HttpContext _httpContext;
    private IServerTiming _serverTiming;
    private IServerTimingCallback[] _callbacks;
    private ServerTimingMiddleware _middleware;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _httpContext = new DefaultHttpContext();
        _serverTiming = Factory.CreateServerTiming();
        _callbacks = Factory.CreateCallbacks();
        _middleware = new ServerTimingMiddleware(_ => Task.CompletedTask, new ServerTimingOptions(), new ActivityMonitor(null), null);
    }

    [Benchmark]
    public Task OnResponseCompleted() => _middleware.OnResponseCompleted(_httpContext, _serverTiming, _callbacks);
}