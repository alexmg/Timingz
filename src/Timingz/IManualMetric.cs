namespace Timingz;

public interface IManualMetric
{
    void Start();

    void Stop();

    bool IsRunning { get; }
}