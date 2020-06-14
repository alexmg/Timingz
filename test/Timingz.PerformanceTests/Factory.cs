using System.Linq;
using System.Threading.Tasks;

namespace Timingz.PerformanceTests
{
    internal static class Factory
    {
        internal static IServerTiming CreateServerTiming(int customTimingCount = 10)
        {
            var serverTiming = new ServerTiming();
            for (var i = 0; i < customTimingCount; i++)
                serverTiming.Precalculated($"metric{i:D2}", 123.123, $"This is a description {i:D2}");

            return serverTiming;
        }

        internal static IServerTimingCallback[] CreateCallbacks(int count = 5) =>
            Enumerable.Range(1, count)
                .Select(_ => new ServerTimingCallback())
                .OfType<IServerTimingCallback>()
                .ToArray();

        private class ServerTimingCallback : IServerTimingCallback
        {
            public async Task OnServerTiming(ServerTimingEvent serverTimingEvent) => await Task.Yield();
        }
    }
}
