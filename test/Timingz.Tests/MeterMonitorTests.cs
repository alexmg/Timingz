using System.Diagnostics.Metrics;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Timingz.Tests;

public class MeterMonitorTests
{
    [Fact]
    public void AddsHistogramValuesToPrecalculatedMetric()
    {
        var httpContextAccessor = A.Fake<IHttpContextAccessor>();
        var serviceProvider = A.Fake<IServiceProvider>();
        var serverTiming = new ServerTiming();

        A.CallTo(() => httpContextAccessor.HttpContext.RequestServices)
            .Returns(serviceProvider);
        A.CallTo(() => serviceProvider.GetService(typeof(IServerTiming)))
            .Returns(serverTiming);

        var monitor = new MeterMonitor(httpContextAccessor);
        var options = new ServerTimingOptions { HistogramFilter = _ => true };
        monitor.Initialize(options);

        var meter = new Meter("meter");
        var histogram = meter.CreateHistogram<double>("histogram");
        histogram.Record(123.456);

        serverTiming.GetMetrics().Should()
            .HaveCount(1)
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            .And.ContainSingle(x => x.Name == histogram.Name && x.Duration == 123.456);
    }
}