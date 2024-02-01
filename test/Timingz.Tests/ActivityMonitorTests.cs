using System.Diagnostics;
using FakeItEasy;
using FluentAssertions;
using Xunit;

// ReSharper disable ExplicitCallerInfoArgument

namespace Timingz.Tests;

public class ActivityListenerServiceTests
{
    private static readonly ActivitySource Source = new("TestSource");

    [Fact]
    public void AddsActivityWithCustomPropertyToHttpContextItems()
    {
        var serverTiming = A.Fake<IServerTiming>();
        using var monitor = CreateListenerService(serverTiming);

        var activity1 = Source.StartActivity("Test1").AddServerTiming();
        activity1!.Stop();

        var activity2 = Source.StartActivity("Test2").AddServerTiming();
        activity2!.Stop();

        A.CallTo(() => serverTiming.Precalculated("Test1", A<double>._, null)).MustHaveHappenedOnceExactly();
        A.CallTo(() => serverTiming.Precalculated("Test2", A<double>._, null)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void IgnoresActivityWithoutCustomProperty()
    {
        var serverTiming = A.Fake<IServerTiming>();
        using var monitor = CreateListenerService(serverTiming);

        var activity = Source.StartActivity("Test");
        activity?.Stop();

        A.CallTo(() => serverTiming.Precalculated("Test", A<double>._, null)).MustNotHaveHappened();
    }

    [Fact]
    public void IgnoresActivityWhenSourceNotConfigured()
    {
        var serverTiming = A.Fake<IServerTiming>();
        using var monitor = CreateListenerService(serverTiming);

        var source = new ActivitySource("OtherSource");

        var activity = source.StartActivity("Test");
        activity.Should().BeNull();
    }

    [Fact]
    public void DoesNotAddActivityListenerWhenNoSourcesConfigured()
    {
        var serverTiming = A.Fake<IServerTiming>();
        using var monitor = CreateListenerService(serverTiming, false);

        var activity = Source.StartActivity("Test").AddServerTiming();

        activity.Should().BeNull();
    }

    private static ActivityMonitor CreateListenerService(IServerTiming serverTiming, bool enabled = true)
    {
        var accessor = A.Fake<IHttpContextAccessor>();
        var serviceProvider = A.Fake<IServiceProvider>();

        var context = new DefaultHttpContext { RequestServices = serviceProvider };
        A.CallTo(() => accessor.HttpContext).Returns(context);
        A.CallTo(() => serviceProvider.GetService(typeof(IServerTiming))).Returns(serverTiming);

        var options = new ServerTimingOptions();
        if (enabled)
            options.ActivitySources.Add(Source.Name);

        var monitor = new ActivityMonitor(accessor);
        monitor.Initialize(options);
        return monitor;
    }
}