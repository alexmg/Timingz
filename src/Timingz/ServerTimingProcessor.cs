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
                List<Activity> activities;

                var items = _httpContextAccessor.HttpContext.Items;

                if (items.TryGetValue(ServerTimingMiddleware.ActivitiesItemKey, out var timings))
                {
                    activities = (List<Activity>)timings;
                }
                else
                {
                    activities = new List<Activity>();
                    items[ServerTimingMiddleware.ActivitiesItemKey] = activities;
                }

                activities.Add(data);
            }

            base.OnEnd(data);
        }
    }
}