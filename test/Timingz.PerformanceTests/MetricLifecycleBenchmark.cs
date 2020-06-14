using BenchmarkDotNet.Attributes;

namespace Timingz.PerformanceTests
{
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
}