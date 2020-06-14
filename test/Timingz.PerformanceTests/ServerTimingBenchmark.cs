using System;
using BenchmarkDotNet.Attributes;

namespace Timingz.PerformanceTests
{
    public class ServerTimingBenchmark
    {
        private const string MetricName = "Benchmark";

        [Benchmark]
        public IManualMetric Manual()
        {
            var serverTiming = new ServerTiming();
            return serverTiming.Manual(MetricName);
        }

        [Benchmark]
        public IDisposable Disposable()
        {
            var serverTiming = new ServerTiming();
            return serverTiming.Disposable(MetricName);
        }

        [Benchmark]
        public void Marker()
        {
            var serverTiming = new ServerTiming();
            serverTiming.Marker(MetricName);
        }

        [Benchmark]
        public void Precalculated()
        {
            var serverTiming = new ServerTiming();
            serverTiming.Precalculated(MetricName, 1.2);
        }
    }
}
