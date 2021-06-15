using System;
using System.Diagnostics;
using FakeItEasy;
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
            var serverTiming = A.Fake<IServerTiming>();
            var serviceProvider = A.Fake<IServiceProvider>();

            var context = new DefaultHttpContext {RequestServices = serviceProvider};
            A.CallTo(() => accessor.HttpContext).Returns(context);
            A.CallTo(() => serviceProvider.GetService(typeof(IServerTiming))).Returns(serverTiming);

            var processor = new ServerTimingProcessor(accessor);

            var activity1 = new Activity("Test1").AddServerTiming();
            processor.OnEnd(activity1);

            var activity2 = new Activity("Test2").AddServerTiming();
            processor.OnEnd(activity2);

            A.CallTo(() => serverTiming.Precalculated("Test1", A<double>._, null)).MustHaveHappenedOnceExactly();
            A.CallTo(() => serverTiming.Precalculated("Test2", A<double>._, null)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void IgnoresActivityWithoutCustomProperty()
        {
            var accessor = A.Fake<IHttpContextAccessor>();
            var serverTiming = A.Fake<IServerTiming>();
            var serviceProvider = A.Fake<IServiceProvider>();
            
            var context = new DefaultHttpContext {RequestServices = serviceProvider};
            A.CallTo(() => accessor.HttpContext).Returns(context);
            A.CallTo(() => serviceProvider.GetService(typeof(IServerTiming))).Returns(serverTiming);

            var processor = new ServerTimingProcessor(accessor);
            var activity = new Activity("Test");
            processor.OnEnd(activity);

            A.CallTo(() => serverTiming.Precalculated("Test", A<double>._, null)).MustNotHaveHappened();
        }
    }
}