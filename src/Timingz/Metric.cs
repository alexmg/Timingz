namespace Timingz
{
    internal class Metric : IMetric
    {
        public string Name { get; }

        public string Description { get; }

        public double? Duration { get; protected set; }

        internal Metric(string name, string description = null)
        {
            Name = name;
            Description = description;
        }
    }
}