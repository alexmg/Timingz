using FluentAssertions;
using Xunit;

namespace Timingz.Tests;

public class PrecalculatedMetricTests
{
    [Theory]
    [InlineData("foo", 1.23d, null)]
    [InlineData("foo", 1.23d, "foo description")]
    public void InitialStateSetFromConstructor(string name, double duration, string description)
    {
        var metric = new PrecalculatedMetric(name, duration, description);
        metric.Name.Should().Be(name);
        metric.Duration.Should().Be(duration);
        metric.Description.Should().Be(description);
    }
}