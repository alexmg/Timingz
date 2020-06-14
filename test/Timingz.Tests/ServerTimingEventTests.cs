using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Timingz.Tests
{
    public class ServerTimingEventTests
    {
        [Fact]
        public void ReadOnlyStateSetFromConstructor()
        {
            var httpContext = new DefaultHttpContext();
            var metric1 = new Metric("foo");
            var metric2 = new Metric("bar");
            var metrics = new[] {metric1, metric2};

            var @event = new ServerTimingEvent(httpContext, metrics);
            @event.HttpContext.Should().BeSameAs(httpContext);
            @event.Metrics.Should().BeSameAs(metrics);
        }
    }
}
