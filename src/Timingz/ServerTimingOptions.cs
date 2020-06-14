using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Timingz
{
    public class ServerTimingOptions
    {
        internal const string DefaultTotalMetricName = "total";

        internal const string DefaultTotalDescription = "Total";

        public string TotalMetricName { get; set; } = DefaultTotalMetricName;

        public string TotalMetricDescription { get; set; } = DefaultTotalDescription;

        public Action<HttpContext, RequestTimingOptions> ConfigureRequestTimingOptions { get; set; } = (_, __) => { };

        public IEnumerable<string> TimingAllowOrigins { get; set; } = Enumerable.Empty<string>();

        public bool InvokeCallbackServices { get; set; }

        public bool ValidateMetrics { get; set; }

        internal void Validate()
        {
            static string Property(string property) => $"{nameof(ServerTimingOptions)}.{property}";

            if (string.IsNullOrWhiteSpace(TotalMetricName))
                throw new Exception(
                    $"A name must be provided for the {Property(nameof(TotalMetricName))} option.");

            if (ConfigureRequestTimingOptions == null)
                throw new Exception(
                    $"An action must be provided for the {Property(nameof(ConfigureRequestTimingOptions))} option.");
        }
    }
}
