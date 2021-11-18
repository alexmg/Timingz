using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Timingz;

internal class ActivityMonitor : IDisposable
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ActivityListener _listener;

    internal const string CustomPropertyKey = "__Timingz";

    public ActivityMonitor(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    internal void Initialize(ServerTimingOptions options)
    {
        if (options.ActivitySources.Count == 0) return;
            
        _listener = new ActivityListener();
        _listener.ShouldListenTo = source => options.ActivitySources.Contains(source.Name);
        _listener.Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.PropagationData;
        _listener.ActivityStopped = OnActivityStopped;
        ActivitySource.AddActivityListener(_listener);
    }

    public void Dispose() => _listener?.Dispose();

    private void OnActivityStopped(Activity activity)
    {
        if (activity.GetCustomProperty(CustomPropertyKey) == null
            || _httpContextAccessor.HttpContext.RequestServices == null) return;
                
        var serverTiming = _httpContextAccessor.HttpContext.RequestServices.GetService<IServerTiming>();

        var description = activity.OperationName != activity.DisplayName ? activity.DisplayName : null;
        serverTiming.Precalculated(activity.OperationName, activity.Duration.TotalMilliseconds, description);
    }
}