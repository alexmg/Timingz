using System;

namespace Timingz.PerformanceTests
{
    internal static class Benchmarks
    {
        internal static readonly Type[] All =
        {
            typeof(HeaderWriterBenchmark),
            typeof(MetricLifecycleBenchmark),
            typeof(InvokeBenchmark),
            typeof(ResponseStartingBenchmark),
            typeof(ResponseCompletedBenchmark),
            typeof(ServerTimingBenchmark)
        };
    }
}