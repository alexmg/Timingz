using System.Diagnostics.CodeAnalysis;

namespace Timingz;

public interface IManualMetric
{
    void Start();

    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
    void Stop();

    bool IsRunning { get; }
}