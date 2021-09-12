using FluentAssertions;
using Xunit;

namespace Timingz.Tests
{
    public class ActivityMonitoringOptionsTests
    {
        [Fact]
        public void EnabledDefaultsToFalse()
        {
            var options = new ActivityMonitoringOptions();
            options.Enabled.Should().BeFalse();
        }
        
        [Fact]
        public void SourceInitializedToEmptyHashSet()
        {
            var options = new ActivityMonitoringOptions();
            options.Sources.Should().BeEmpty();
        }
    }
}