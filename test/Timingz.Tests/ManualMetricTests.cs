using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Timingz.Tests
{
    public class ManualMetricTests
    {
        private const string Name = "foo";
        private const string Description = "foo description";

        [Theory]
        [InlineData(Name, null)]
        [InlineData(Name, Description)]
        public void InitialStateSetFromConstructor(string name, string description)
        {
            var metric = new ManualMetric(name, description);
            metric.Name.Should().Be(name);
            metric.Description.Should().Be(description);
            metric.Duration.HasValue.Should().BeFalse();
        }

        [Fact]
        public void ValidateReturnsFalseWhenNotStarted()
        {
            var metric = new ManualMetric(Name, Description);
            metric.Validate(out var message).Should().BeFalse();
            message.Should().Contain("was not started and stopped");
        }

        [Fact]
        public void ValidateReturnsFalseWhenNotStopped()
        {
            var metric = new ManualMetric(Name, Description);
            metric.Start();
            metric.Validate(out var message).Should().BeFalse();
            message.Should().Contain("was started and not stopped");
        }

        [Fact]
        public void ValidateReturnsTrueWhenStartedAndStopped()
        {
            var metric = new ManualMetric(Name, Description);
            metric.Start();
            metric.Stop();
            metric.Validate(out _).Should().BeTrue();
        }

        [Fact]
        public void StartThrowsWhenAlreadyStarted()
        {
            var metric = new ManualMetric(Name, Description);
            metric.Start();
            Action validate = () => metric.Start();
            validate.Should().Throw<InvalidOperationException>()
                .Which.Message.Should().Be("The manual timing has already been started.");
        }

        [Fact]
        public void StopThrowsWhenAlreadyStopped()
        {
            var metric = new ManualMetric(Name, Description);
            metric.Start();
            metric.Stop();
            Action validate = () => metric.Stop();
            validate.Should().Throw<InvalidOperationException>()
                .Which.Message.Should().Be("The manual timing has not been started.");
        }

        [Fact]
        public void StopThrowsWhenNotStarted()
        {
            var metric = new ManualMetric(Name, Description);
            Action validate = () => metric.Stop();
            validate.Should().Throw<InvalidOperationException>()
                .Which.Message.Should().Be("The manual timing has not been started.");
        }

        [Fact]
        public void DurationDoesNotHaveInitialValue()
        {
            var metric = new ManualMetric(Name, Description);
            metric.Duration.HasValue.Should().BeFalse();
        }

        [Fact]
        public void DurationDoesNotHaveValueAtFirstStart()
        {
            var metric = new ManualMetric(Name, Description);
            metric.Start();
            metric.Duration.HasValue.Should().BeFalse();
        }

        [Fact]
        public void DurationHasValueOnceStopped()
        {
            var metric = new ManualMetric(Name, Description);
            metric.Start();
            metric.Stop();
            metric.Duration.HasValue.Should().BeTrue();
        }

        [Fact]
        public void IsRunningUpdatedWhenStartingAndStopping()
        {
            var metric = new ManualMetric(Name, Description);
            metric.IsRunning.Should().BeFalse();

            metric.Start();
            metric.IsRunning.Should().BeTrue();

            metric.Stop();
            metric.IsRunning.Should().BeFalse();
        }

        [Fact]
        public async Task CanStartAndStoppedMultipleTimes()
        {
            var metric = new ManualMetric(Name, Description);

            const int iterations = 10;
            var durations = new List<double?>(iterations);

            for (var i = 0; i < iterations; i++)
            {
                metric.Start();
                await Task.Delay(1);
                metric.Stop();
                durations.Add(metric.Duration);
            }

            durations.Should().NotContainNulls().And.BeInAscendingOrder();
        }
    }
}
