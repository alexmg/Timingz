using System;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using Xunit;

namespace Timingz.Tests
{
    public class TracerProviderBuilderExtensionsTests
    {
        [Fact]
        public void ThrowsWhenBuilderNull()
        {
            Action invoke = () => TracerProviderBuilderExtensions.AddServerTimingProcessor(null);
            invoke.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");
        }

        [Fact]
        public void ThrowsWhenBuilderNotIDeferredTracerProviderBuilder()
        {
            var builder = A.Fake<TracerProviderBuilderBase>();
            Action ctor = () => builder.AddServerTimingProcessor();
            ctor.Should().Throw<NotSupportedException>();
        }

        [Fact]
        public void AddsServicesToDeferredBuilder()
        {
            var services = new ServiceCollection();
            services.AddOpenTelemetryTracing(builder => builder.AddServerTimingProcessor());
            
            var provider = services.BuildServiceProvider();
            
            provider.GetService<ServerTimingProcessor>().Should().NotBeNull();
            provider.GetService<IHttpContextAccessor>().Should().NotBeNull();
            provider.GetService<TracerProvider>().Should().NotBeNull();
        }
    }
}