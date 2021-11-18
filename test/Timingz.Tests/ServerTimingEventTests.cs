using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http.Extensions;
using Xunit;

namespace Timingz.Tests;

public class ServerTimingEventTests
{
    [Fact]
    public void ReadOnlyStateSetFromConstructorParameters()
    {
        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = "foo",
            Request =
            {
                Method = "GET",
                Scheme = "https",
                Host = new HostString("localhost", 8080),
                Path = "/foo/bar",
                QueryString = new QueryString("?foo=bar"),
                Protocol = "HTTP/1.1"
            },
            Connection =
            {
                RemoteIpAddress = IPAddress.Loopback,
                RemotePort = 80
            }
        };
        httpContext.Request.Headers.Add("foo", "bar");
        httpContext.Response.StatusCode = 200;
        httpContext.Response.Headers.Add("foo", "bar");

        var metric1 = new Metric("foo");
        var metric2 = new Metric("bar");
        var metrics = new[] { metric1, metric2 };

        var timingEvent = new ServerTimingEvent(httpContext, metrics);
        var context = timingEvent.Context;

        context.TraceIdentifier.Should().Be(httpContext.TraceIdentifier);
        context.DisplayUrl.Should().Be(httpContext.Request.GetDisplayUrl());
        context.Method.Should().Be(httpContext.Request.Method);
        context.Scheme.Should().Be(httpContext.Request.Scheme);
        context.Host.Should().Be(httpContext.Request.Host);
        context.Path.Should().Be(httpContext.Request.Path);
        context.QueryString.Should().Be(httpContext.Request.QueryString);
        context.Protocol.Should().Be(httpContext.Request.Protocol);
        context.RemoteIpAddress.Should().Be(httpContext.Connection.RemoteIpAddress);
        context.RemotePort.Should().Be(httpContext.Connection.RemotePort);
        context.RequestHeaders.Should().BeSameAs(httpContext.Request.Headers);
        context.StatusCode.Should().Be(httpContext.Response.StatusCode);
        context.ResponseHeaders.Should().BeSameAs(httpContext.Response.Headers);

        timingEvent.Metrics.Should().BeSameAs(metrics);
    }
}