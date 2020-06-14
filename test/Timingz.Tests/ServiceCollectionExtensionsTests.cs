using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Timingz.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddServerTimingAddsServiceToCollection()
        {
            var services = new ServiceCollection();

            services.AddServerTiming();

            var descriptor = services[0];
            descriptor.ServiceType.Should().Be(typeof(IServerTiming));
            descriptor.ImplementationType.Should().Be(typeof(ServerTiming));
            descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
        }

        [Fact]
        public void AddServerTimingThrowsWhenServicesNull()
        {
            Action use = () => ServiceCollectionExtensions.AddServerTiming(null);

            use.Should().Throw<ArgumentNullException>()
                .Which.ParamName.Should().Be("services");
        }
    }
}
