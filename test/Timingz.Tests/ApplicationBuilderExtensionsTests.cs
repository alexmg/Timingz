using FluentAssertions;
using Xunit;

namespace Timingz.Tests;

public class ApplicationBuilderExtensionsTests
{
    [Fact]
    public void UseServerTimingInvokesConfigurationAction()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        ServerTimingOptions capturedOptions = null;
        app.UseServerTiming(options => capturedOptions = options);

        capturedOptions.Should().NotBeNull();
    }

    [Fact]
    public void UseServerTimingWithOptionsThrowsWhenAppNull()
    {
        var options = new ServerTimingOptions();
        Action use = () => ApplicationBuilderExtensions.UseServerTiming(null, options);

        use.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("app");
    }

    [Fact]
    public void UseServerTimingWithOptionsThrowsWhenOptionsNull()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);
        Action use = () => app.UseServerTiming(default(ServerTimingOptions));

        use.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("options");
    }

    [Fact]
    public void UseServerTimingHandlesNullConfigurationAction()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        Action use = () => app.UseServerTiming();
        use.Should().NotThrow();
    }

    [Fact]
    public void UseServerTimingThrowsWhenAppNull()
    {
        Action use = () => ApplicationBuilderExtensions.UseServerTiming(null);

        use.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("app");
    }
}