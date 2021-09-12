using System.Diagnostics;
using FluentAssertions;
using Xunit;

namespace Timingz.Tests
{
    public class ActivityExtensionsTests
    {
        [Fact]
        public void AddServerTimingSetsCustomProperty()
        {
            var activity = new Activity("Test").AddServerTiming();
            activity.GetCustomProperty(ActivityMonitor.CustomPropertyKey).Should().NotBeNull();
        }

        [Fact]
        public void AddServerTimingDescriptionSetsDisplayName()
        {
            const string description = "Test description";
            var activity = new Activity("Test").AddServerTiming(description);
            activity.DisplayName.Should().Be(description);
        }
    }
}
