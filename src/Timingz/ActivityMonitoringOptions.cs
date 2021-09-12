using System.Collections.Generic;

namespace Timingz
{
    public class ActivityMonitoringOptions
    {
        public bool Enabled { get; set; }

        public HashSet<string> Sources { get; } = new();
    }
}