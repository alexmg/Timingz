using FluentAssertions;
using Xunit;

namespace Timingz.Tests
{
    public class RequestTimingOptionsTests
    {
        [Fact]
        public void WriteHeaderInitializedToFalse()
        {
            var options = new RequestTimingOptions();
            options.WriteHeader.Should().BeFalse();
        }

        [Fact]
        public void IncludeDescriptionsInitializedToFalse()
        {
            var options = new RequestTimingOptions();
            options.IncludeDescriptions.Should().BeFalse();
        }

        [Fact]
        public void IncludeCustomMetricsInitializedToFalse()
        {
            var options = new RequestTimingOptions();
            options.IncludeCustomMetrics.Should().BeFalse();
        }
    }
}
