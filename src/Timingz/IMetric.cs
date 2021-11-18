namespace Timingz;

public interface IMetric
{
    string Name { get; }

    string Description { get; }

    double? Duration { get; }
}