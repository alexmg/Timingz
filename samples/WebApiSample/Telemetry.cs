using System.Diagnostics;

namespace WebApiSample
{
    public static class Telemetry
    {
        internal static readonly ActivitySource Source = new("WebApiSample");
    }
}