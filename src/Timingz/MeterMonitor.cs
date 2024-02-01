using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Timingz;

public sealed class MeterMonitor : IDisposable
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly MeterListener _listener = new();

    public MeterMonitor(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    internal void Initialize(ServerTimingOptions options)
    {
        _listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument is Histogram<double> histogram && options.HistogramFilter(histogram))
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };
        _listener.SetMeasurementEventCallback<double>(OnMeasurementRecorded);
        _listener.Start();
    }

    private void OnMeasurementRecorded(
        Instrument instrument,
        double measurement,
        ReadOnlySpan<KeyValuePair<string, object?>> tags,
        object? state)
    {
        var serverTiming = _httpContextAccessor.HttpContext?.RequestServices.GetService<IServerTiming>();
        serverTiming?.Precalculated(instrument.Name, measurement, instrument.Description);
    }

    public void Dispose() => _listener.Dispose();
}