using System.Diagnostics.Metrics;
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
        var invoke = () => options.ConfigureRequestTimingOptions(httpContext, requestOptions);
        invoke.Should().NotThrow();
    }

    [Fact]
    public void WithRequestTimingOptionsThrowsWhenActionNull()
    {
        var options = new ServerTimingOptions();
        var invoke = () => options.WithRequestTimingOptions(null);
        invoke.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithRequestTimingOptionsCapturesAction()
    {
        var options = new ServerTimingOptions();
        var called = false;
        options.WithRequestTimingOptions((_, _) => called = true);
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
    public void DurationPrecisionInitializedToDefault()
    {
        var options = new ServerTimingOptions();
        options.DurationPrecision.Should().Be(ServerTimingOptions.DefaultDurationPrecision);
    }
    
    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(15, true)]
    [InlineData(16, false)]
    public void DurationPrecisionPropertyValidatesValue(int precision, bool valid)
    {
        var options = new ServerTimingOptions();
        Action validate = () => options.DurationPrecision = precision;
        if (valid)
            validate.Should().NotThrow();
        else
            validate.Should().Throw<ArgumentException>()
                .Which.ParamName.Should().Contain(nameof(ServerTimingOptions.DurationPrecision));
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

    [Fact]
    public void HistogramFilterReturnsFalseByDefault()
    {
        var options = new ServerTimingOptions();
        var meter = new Meter("Test");
        options.HistogramFilter(meter.CreateHistogram<double>("Test")).Should().BeFalse();
    }

    [Fact]
    public void HistogramFilterCanBeReplaced()
    {
        var options = new ServerTimingOptions();
        var meter = new Meter("Test");
        var histogram = meter.CreateHistogram<double>("Test");
        options.HistogramFilter = h => h == histogram;
        options.HistogramFilter(histogram).Should().BeTrue();
    }
}