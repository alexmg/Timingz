using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Timingz;

namespace WebApiSample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SampleController : ControllerBase
{
    private readonly IServerTiming _serverTiming;

    public SampleController(IServerTiming serverTiming) => _serverTiming = serverTiming;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        // This activity measures the duration of the action method and is included as a metric.
        using var actionActivity = Telemetry.Source.StartActivity("Action").AddServerTiming("GET action method");

        // Note that there was a cache miss. This metric does not have a duration and is simply an informational tag.
        _serverTiming.Marker("miss", "Cache miss");

        // Manual metrics allow for starting and stopping so that multiple calls can be combined into a single duration.
        var databaseMetric = _serverTiming.Manual("db", "Database queries");
        var cacheMetric = _serverTiming.Manual("cache", "Cache writes");

        using (Telemetry.Source.StartActivity("Database").AddServerTiming("Queries and caching"))
        {
            // Execute first database query.
            databaseMetric.Start();
            await Task.Delay(30);
            databaseMetric.Stop();

            // Write first result to cache.
            cacheMetric.Start();
            await Task.Delay(15);
            cacheMetric.Stop();

            // Execute second database query.
            databaseMetric.Start();
            await Task.Delay(30);
            databaseMetric.Stop();

            // Write second result to cache.
            cacheMetric.Start();
            await Task.Delay(15);
            cacheMetric.Stop();
        }

        // A disposable metric is started immediately and the duration is captured at the point of disposal.
        using (_serverTiming.Disposable("bus", "Send notification to bus"))
            await Task.Delay(10);

        // A precalculated metric allows timings to be added that were captured using a different metric package.
        var externalTiming = Stopwatch.StartNew();
        await Task.Delay(20);
        var duration = externalTiming.ElapsedMilliseconds;
        _serverTiming.Precalculated("external", duration, "External metric timing");

        // A list of metrics can be retrieved.
        // The duration for the request timing metric is not available yet because the request is still executing.
        return Ok(_serverTiming.GetMetrics());
    }
}