using System.Linq;
using BenchmarkDotNet.Running;
using Xunit;

namespace Timingz.PerformanceTests
{
    public class Harness
    {
        [Fact]
        public void HeaderWriterBenchmark() => RunBenchmark<HeaderWriterBenchmark>();

        [Fact]
        public void MetricLifecycleBenchmark() => RunBenchmark<MetricLifecycleBenchmark>();

        [Fact]
        public void InvokeBenchmark() => RunBenchmark<InvokeBenchmark>();

        [Fact]
        public void RequestCompletedBenchmark() => RunBenchmark<ResponseStartingBenchmark>();

        [Fact]
        public void RequestStartingBenchmark() => RunBenchmark<ResponseCompletedBenchmark>();

        [Fact]
        public void ServerTimingBenchmark() => RunBenchmark<ServerTimingBenchmark>();

        /// <remarks>
        /// This method is used to enforce that benchmark types are added to <see cref="Benchmarks.All"/>
        /// so that they can be used directly from the command line in <see cref="Program.Main"/> as well.
        /// </remarks>
        private static void RunBenchmark<TBenchmark>()
        {
            var targetType = typeof(TBenchmark);
            var benchmarkType = Benchmarks.All.Single(type => type == targetType);
            BenchmarkRunner.Run(benchmarkType, new BenchmarkConfig());
        }
    }
}