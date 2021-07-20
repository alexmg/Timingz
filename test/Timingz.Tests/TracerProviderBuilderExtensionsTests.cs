using System;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        public void AddsProcessorDuringDeferredBuilderCallback()
        {
            var httpContextAccessor = A.Fake<IHttpContextAccessor>();
            var processor = new ServerTimingProcessor(httpContextAccessor);
            var instanceResolved = false;

            var builder = new HostBuilder()
                .ConfigureServices(services =>
                    services.AddSingleton(sp =>
                        {
                            instanceResolved = true;
                            return processor;
                        })
                        .AddOpenTelemetryTracing(tracerProviderBuilder =>
                            tracerProviderBuilder.AddServerTimingProcessor()));

            var host = builder.Build();
            instanceResolved.Should().BeFalse();

            host.Services.GetService<TracerProvider>().Should().NotBeNull();
            instanceResolved.Should().BeTrue();
        }
    }
}