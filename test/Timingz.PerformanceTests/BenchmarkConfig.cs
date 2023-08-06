using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace Timingz.PerformanceTests;

internal sealed class BenchmarkConfig : ManualConfig
{
    internal BenchmarkConfig()
    {
        Add(DefaultConfig.Instance);

        var baseDirectory = AppContext.BaseDirectory;
        var rootFolder = baseDirectory[..baseDirectory.LastIndexOf("bin", StringComparison.OrdinalIgnoreCase)];
        var runFolder = DateTime.Now.ToString("u").Replace(' ', '_').Replace(':', '-');
        ArtifactsPath = $@"{rootFolder}\BenchmarkDotNet.Artifacts\{runFolder}";

        AddDiagnoser(MemoryDiagnoser.Default);
    }
}