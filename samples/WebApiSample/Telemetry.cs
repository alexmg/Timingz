using System.Diagnostics;

namespace WebApiSample
{
    internal static class Telemetry
    {
        internal static readonly ActivitySource Source = new("WebApiSample");
    }
}