using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;

namespace Timingz.PerformanceTests;

public class HeaderWriterBenchmark
{
    private IReadOnlyList<IMetric> _metrics;
    private HeaderWriter _headerWriter;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _metrics = Factory.CreateServerTiming(20).GetMetrics();

        var timingAllowOrigins = new List<string>
        {
            "https://origin1.com/",
            "https://origin2.com/"
        };
        _headerWriter = new HeaderWriter(timingAllowOrigins);
    }

    [Params(true, false)]
    public bool IncludeDescriptions { get; set; }

    [Benchmark]
    public void WriteHeaders() => _headerWriter.WriteHeaders(new HeaderDictionary(), IncludeDescriptions, _metrics);
}