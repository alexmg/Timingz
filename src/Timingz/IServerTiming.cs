namespace Timingz;

public interface IServerTiming
{
    IManualMetric Manual(string name, string? description = null);

    IDisposable Disposable(string name, string? description = null);

    void Precalculated(string name, double duration, string? description = null);

    void Marker(string name, string? description = null);

    IReadOnlyList<IMetric> GetMetrics();
}