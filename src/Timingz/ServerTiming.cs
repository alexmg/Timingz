using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Timingz
{
    public class ServerTiming : IServerTiming
    {
        private readonly ConcurrentDictionary<string, IMetric> _metrics =
            new ConcurrentDictionary<string, IMetric>(StringComparer.OrdinalIgnoreCase);

        public IManualMetric Manual(string name, string description = null) =>
            AddMetric(name, new ManualMetric(name, description));

        public IDisposable Disposable(string name, string description = null) =>
            AddMetric(name, new DisposableMetric(name, description));

        public void Precalculated(string name, double duration, string description = null) =>
            AddMetric(name, new PrecalculatedMetric(name, duration, description));

        public void Marker(string name, string description = null) =>
            AddMetric(name, new Metric(name, description));

        public IReadOnlyList<IMetric> GetMetrics() => _metrics.Values.ToArray();

        private TMetric AddMetric<TMetric>(string name, TMetric metric) where TMetric : IMetric
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The metric name cannot be null or empty.", nameof(name));

            if (_metrics.TryAdd(name, metric))
                return metric;

            throw new ArgumentException($"A metric with the name '{name}' already exists.", nameof(name));
        }
    }
}
