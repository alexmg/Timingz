using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;

namespace Timingz.PerformanceTests
{
    public class InvokeBenchmark
    {
        private ServerTimingMiddleware _middleware;
        private ServerTimingOptions _serverTimingOptions;
        private IServerTimingCallback[] _callbacks;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _serverTimingOptions = new ServerTimingOptions
            {
                InvokeCallbackServices = true
            };
            _serverTimingOptions.WithRequestTimingOptions((_, requestOptions) => requestOptions.WriteHeader = true);
            _middleware = new ServerTimingMiddleware(_ => Task.CompletedTask, _serverTimingOptions, null);
            _callbacks = Factory.CreateCallbacks(1);
        }

        [Benchmark]
        public Task Invoke() => _middleware.Invoke(new DefaultHttpContext(), new ServerTiming(), _callbacks);
    }
}
