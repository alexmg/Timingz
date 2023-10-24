using Cysharp.Text;
using Microsoft.AspNetCore.Http;

namespace Timingz;

internal class HeaderWriter
{
    private readonly int _durationPrecision;
    private readonly string _timingAllowOriginValue;

    internal const string TimingAllowOriginHeaderName = "Timing-Allow-Origin";
    internal const string ServerTimingHeaderName = "Server-Timing";

    internal HeaderWriter(IEnumerable<string> timingAllowOrigins, int durationPrecision)
    {
        var origins = timingAllowOrigins?.ToArray();
        if (origins?.Length > 0)
            _timingAllowOriginValue = ZString.Join(",", origins);
        _durationPrecision = durationPrecision;
    }

    internal void WriteHeaders(
        IHeaderDictionary headers,
        bool includeDescription,
        IReadOnlyList<IMetric> metrics)
    {
        if (metrics.Count == 0) return;

        if (_timingAllowOriginValue != null)
            headers.Append(TimingAllowOriginHeaderName, _timingAllowOriginValue);

        var serverTimingValue = BuildServerTimingHeader(metrics, includeDescription);
        headers.Append(ServerTimingHeaderName, serverTimingValue);
    }

    private string BuildServerTimingHeader(IReadOnlyList<IMetric> metrics, bool includeDescription)
    {
        using var builder = ZString.CreateStringBuilder(true);

        for (var i = 0; i < metrics.Count; i++)
        {
            var metric = metrics[i];

            builder.Append(metric.Name);

            if (metric.Duration.HasValue)
                builder.AppendFormat(";dur={0}", Math.Round(metric.Duration.Value, _durationPrecision));

            if (includeDescription && !string.IsNullOrWhiteSpace(metric.Description))
                builder.AppendFormat(";desc=\"{0}\"", metric.Description);

            if (i != metrics.Count - 1)
                builder.Append(",");
        }

        return builder.ToString();
    }
}