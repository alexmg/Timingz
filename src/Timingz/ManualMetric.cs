using System;
using Perfolizer.Horology;

namespace Timingz
{
    internal class ManualMetric : Metric, IManualMetric, IValidatableMetric
    {
        private StartedClock? _startedClock;

        internal ManualMetric(string name, string description = null) : base(name, description)
        {
        }

        public bool IsRunning => _startedClock.HasValue;

        public void Start()
        {
            if (_startedClock.HasValue)
                throw new InvalidOperationException("The manual timing has already been started.");

            _startedClock = Chronometer.Start();
        }

        public void Stop()
        {
            if (!_startedClock.HasValue)
                throw new InvalidOperationException("The manual timing has not been started.");

            var elapsed = _startedClock.Value.GetElapsed();
            _startedClock = null;

            Duration = (Duration ?? 0) + elapsed.GetTimeValue().ToMilliseconds();
        }

        public bool Validate(out string message)
        {
            message = default;

            if (_startedClock.HasValue)
            {
                message = $"The manual timing '{Name}' was started and not stopped.";
                return false;
            }

            if (Duration.HasValue) return true;

            message = $"The manual timing '{Name}' was not started and stopped.";
            return false;
        }
    }
}