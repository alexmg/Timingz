using System.Diagnostics;

namespace Timingz;

internal class DisposableMetric : Metric, IDisposable, IValidatableMetric
{
    private readonly long _started = Stopwatch.GetTimestamp();

    internal DisposableMetric(string name, string description = null) : base(name, description)
    {
    }

    public void Dispose() => Duration = GetElapsedMilliseconds(_started);

    public bool Validate(out string message)
    {
        message = default;

        if (Duration.HasValue) return true;

        message = $"The disposable timing '{Name}' has not been disposed.";
        return false;
    }
}