using FluentAssertions;
using Xunit;

namespace Timingz.Tests;

public class ServerTimingOptionsTests
{
    [Fact]
    public void TimingAllowOriginsInitializedToEmpty()
    {
        var options = new ServerTimingOptions();
        options.TimingAllowOrigins.Should().BeEmpty();
    }

    [Fact]
    public void ConfigureRequestTimingOptionsInitializedToEmptyAction()
    {
        var options = new ServerTimingOptions();
        var httpContext = new DefaultHttpContext();
        var requestOptions = new RequestTimingOptions();
        Action invoke = () => options.ConfigureRequestTimingOptions(httpContext, requestOptions);
        invoke.Should().NotThrow();
    }

    [Fact]
    public void WithRequestTimingOptionsThrowsWhenActionNull()
    {
        var options = new ServerTimingOptions();
        Action invoke = () => options.WithRequestTimingOptions(null);
        invoke.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithRequestTimingOptionsCapturesAction()
    {
        var options = new ServerTimingOptions();
        var called = false;
        options.WithRequestTimingOptions((_, __) => called = true);
        options.ConfigureRequestTimingOptions(null, null);
        called.Should().BeTrue();
    }

    [Fact]
    public void TotalMetricNameInitializedToDefault()
    {
        var options = new ServerTimingOptions();
        options.TotalMetricName.Should().Be(ServerTimingOptions.DefaultTotalMetricName);
    }

    [Fact]
    public void TotalMetricDescriptionInitializedToDefault()
    {
        var options = new ServerTimingOptions();
        options.TotalMetricDescription.Should().Be(ServerTimingOptions.DefaultTotalDescription);
    }

    [Fact]
    public void ValidateMetricsInitializedToFalse()
    {
        var options = new ServerTimingOptions();
        options.ValidateMetrics.Should().BeFalse();
    }

    [Fact]
    public void InvokeCallbackServicesInitializedToFalse()
    {
        var options = new ServerTimingOptions();
        options.InvokeCallbackServices.Should().BeFalse();
    }

    [Fact]
    public void ActivitySourcesInitializedToEmpty()
    {
        var options = new ServerTimingOptions();
        options.ActivitySources.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void TotalMetricNamePropertyValidatesValue(string totalMetricName)
    {
        var options = new ServerTimingOptions();
        Action validate = () => options.TotalMetricName = totalMetricName;
        validate.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Contain(nameof(ServerTimingOptions.TotalMetricName));
    }
}