using System.Collections.Generic;
using System.Diagnostics;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Timingz.Tests
{
    public class ServerTimingProcessorTests
    {
        [Fact]
        public void AddsActivityWithCustomPropertyToHttpContextItems()
        {
            var accessor = A.Fake<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            A.CallTo(() => accessor.HttpContext).Returns(context);

            var processor = new ServerTimingProcessor(accessor);

            var activity1 = new Activity("Test1").AddServerTiming();
            processor.OnEnd(activity1);

            var activity2 = new Activity("Test2").AddServerTiming();
            processor.OnEnd(activity2);

            var activities = context.Items[ServerTimingMiddleware.ActivitiesItemKey].As<List<Activity>>();
            activities.Should().Contain(new[] {activity1, activity2});
        }

        [Fact]
        public void IgnoresActivityWithoutCustomProperty()
        {
            var accessor = A.Fake<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            A.CallTo(() => accessor.HttpContext).Returns(context);

            var processor = new ServerTimingProcessor(accessor);
            var activity = new Activity("Test");
            processor.OnEnd(activity);

            var activities = context.Items[ServerTimingMiddleware.ActivitiesItemKey].As<List<Activity>>();
            activities.Should().BeNull();
        }
    }
}