using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Timingz
{
    public class ServerTimingEvent
    {
        internal ServerTimingEvent(HttpContext httpContext, IReadOnlyList<IMetric> metrics)
        {
            Context = new ServerTimingEventContext(httpContext);
            Metrics = metrics;
        }

        public ServerTimingEventContext Context { get; }

        public IReadOnlyList<IMetric> Metrics { get; }
    }
}