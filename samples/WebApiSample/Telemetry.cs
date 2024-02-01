using System.Diagnostics;

namespace WebApiSample;

internal static class Telemetry
{
    internal static readonly ActivitySource Source = new("WebApiSample");

    internal static readonly string MeterName = Source.Name;
}