using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Timingz;

internal class Metric : IMetric
{
    public string Name { get; }

    public string? Description { get; }

    public double? Duration { get; protected set; }

    internal Metric(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("The metric name cannot be null or empty.", nameof(name));

        Name = name;
        Description = description;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static double GetElapsedMilliseconds(long start) =>
        (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency;
}