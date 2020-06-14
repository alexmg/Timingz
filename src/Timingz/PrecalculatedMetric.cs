namespace Timingz
{
    internal class PrecalculatedMetric : Metric
    {
        internal PrecalculatedMetric(string name, double duration, string description = null)
            : base(name, description) => Duration = duration;
    }
}