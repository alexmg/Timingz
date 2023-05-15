using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;

namespace Timingz.PerformanceTests;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class MetricLifecycleBenchmark
{
    private const string MetricName = "Benchmark";

    [Benchmark]
    public void DisposableMetric()
    {
        var metric = new DisposableMetric(MetricName);
        metric.Dispose();
        metric.Validate(out _);
    }

    [Benchmark]
    public void ManualMetric()
    {
        var metric = new ManualMetric(MetricName);
        metric.Start();
        metric.Stop();
        metric.Validate(out _);
    }
}