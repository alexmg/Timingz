using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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
                var serverTiming = _httpContextAccessor.HttpContext.RequestServices.GetService<IServerTiming>();

                var description = data.OperationName != data.DisplayName ? data.DisplayName : null;
                serverTiming.Precalculated(data.OperationName, data.Duration.TotalMilliseconds, description);
            }

            base.OnEnd(data);
        }
    }
}