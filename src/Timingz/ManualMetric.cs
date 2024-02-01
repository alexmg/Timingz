using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Timingz;

internal class ManualMetric : Metric, IManualMetric, IValidatableMetric
{
    private long? _started;

    internal ManualMetric(string name, string? description = null) : base(name, description)
    {
    }

    public bool IsRunning => _started.HasValue;

    public void Start()
    {
        if (_started.HasValue)
            throw new InvalidOperationException("The manual timing has already been started.");

        _started = Stopwatch.GetTimestamp();
    }

    public void Stop()
    {
        if (!_started.HasValue)
            throw new InvalidOperationException("The manual timing has not been started.");

        var elapsed = GetElapsedMilliseconds(_started.Value);
        _started = null;

        Duration = (Duration ?? 0) + elapsed;
    }

    public bool Validate([NotNullWhen(false)] out string? message)
    {
        message = null;

        if (_started.HasValue)
        {
            message = $"The manual timing '{Name}' was started and not stopped.";
            return false;
        }

        if (Duration.HasValue) return true;

        message = $"The manual timing '{Name}' was not started and stopped.";
        return false;
    }
}