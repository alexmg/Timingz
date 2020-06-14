using System;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Timingz.Tests
{
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
        public void InitialOptionsStateIsValid()
        {
            Action validate = () => new ServerTimingOptions().Validate();
            validate.Should().NotThrow();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void ValidateThrowsExceptionWhenTotalMetricNameInvalid(string totalMetricName)
        {
            var options = new ServerTimingOptions {TotalMetricName = totalMetricName};
            Action validate = () => options.Validate();
            validate.Should().Throw<Exception>()
                .Which.Message.Should().Contain(nameof(ServerTimingOptions.TotalMetricName));
        }

        [Fact]
        public void ValidateThrowsExceptionWhenConfigureRequestTimingOptionsIsNull()
        {
            var options = new ServerTimingOptions {ConfigureRequestTimingOptions = null};
            Action validate = () => options.Validate();
            validate.Should().Throw<Exception>()
                .Which.Message.Should().Contain(nameof(ServerTimingOptions.ConfigureRequestTimingOptions));
        }
    }
}
