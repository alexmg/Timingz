using System.Runtime.CompilerServices;

namespace Timingz;

public class ServerTiming : IServerTiming
{
    private readonly List<IMetric> _metrics = new();

    public IManualMetric Manual(string name, string description = null) =>
        AddMetric(new ManualMetric(name, description));

    public IDisposable Disposable(string name, string description = null) =>
        AddMetric(new DisposableMetric(name, description));

    public void Precalculated(string name, double duration, string description = null) =>
        AddMetric(new PrecalculatedMetric(name, duration, description));

    public void Marker(string name, string description = null) =>
        AddMetric(new Metric(name, description));

    public IReadOnlyList<IMetric> GetMetrics() => _metrics;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TMetric AddMetric<TMetric>(TMetric metric) where TMetric : IMetric
    {
        _metrics.Add(metric);
        return metric;
    }
}