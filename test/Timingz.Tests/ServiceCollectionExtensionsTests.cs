using FluentAssertions;
using Xunit;

namespace Timingz.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddServerTimingAddsServicesToCollection()
    {
        var services = new ServiceCollection();

        services.AddServerTiming();

        services.Should()
            .Contain(descriptor =>
                descriptor.ServiceType == typeof(IServerTiming)
                && descriptor.ImplementationType == typeof(ServerTiming)
                && descriptor.Lifetime == ServiceLifetime.Scoped)
            .And.Contain(descriptor =>
                descriptor.ServiceType == typeof(IHttpContextAccessor)
                && descriptor.ImplementationType == typeof(HttpContextAccessor)
                && descriptor.Lifetime == ServiceLifetime.Singleton)
            .And.Contain(descriptor =>
                descriptor.ServiceType == typeof(ActivityMonitor)
                && descriptor.ImplementationType == typeof(ActivityMonitor)
                && descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddServerTimingThrowsWhenServicesNull()
    {
        Action use = () => ServiceCollectionExtensions.AddServerTiming(null);

        use.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("services");
    }
}