using FluentAssertions;
using Xunit;

namespace Timingz.Tests;

public class DisposableMetricTests
{
    [Fact]
    public void InitialStateSetFromConstructor()
    {
        const string name = "foo";
        const string description = "description";

        var metric = new DisposableMetric(name, description);
        metric.Name.Should().Be(name);
        metric.Description.Should().Be(description);
        metric.Duration.Should().BeNull();
    }

    [Fact]
    public void DisposeCalculatesDuration()
    {
        var metric = new DisposableMetric("foo");
            
        metric.Dispose();

        metric.Duration.HasValue.Should().BeTrue();
    }

    [Fact]
    public void ValidateReturnsFalseWhenMetricNotDisposed()
    {
        var metric = new DisposableMetric("foo");
            
        metric.Validate(out var message).Should().BeFalse();

        message.Should().Contain("has not been disposed");
    }

    [Fact]
    public void ValidateReturnsTrueWhenMetricDisposed()
    {
        var metric = new DisposableMetric("foo");
        metric.Dispose();

        metric.Validate(out _).Should().BeTrue();
    }
}