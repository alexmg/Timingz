using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Timingz
{
    public class ServerTimingEvent
    {
        internal ServerTimingEvent(HttpContext httpContext, IReadOnlyList<IMetric> metrics)
        {
            HttpContext = httpContext;
            Metrics = metrics;
        }

        public HttpContext HttpContext { get; }

        public IReadOnlyList<IMetric> Metrics { get; }
    }
}