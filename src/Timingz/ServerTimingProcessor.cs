using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using OpenTelemetry;

namespace Timingz
{
    internal class ServerTimingProcessor : BaseProcessor<Activity>
    {
        internal const string CustomPropertyKey = "__Timingz";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public ServerTimingProcessor(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

        public override void OnEnd(Activity data)
        {
            if (data.GetCustomProperty(CustomPropertyKey) != null)
            {
                var items = _httpContextAccessor.HttpContext.Items;

                lock (items)
                {
                    if (items[ServerTimingMiddleware.ActivitiesItemKey] is not List<Activity> activities)
                    {
                        activities = new List<Activity>();
                        items[ServerTimingMiddleware.ActivitiesItemKey] = activities;
                    }
                    activities.Add(data);
                }
            }

            base.OnEnd(data);
        }
    }
}