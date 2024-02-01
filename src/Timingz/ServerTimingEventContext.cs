using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Timingz;

public class ServerTimingEventContext
{
    public ServerTimingEventContext(HttpContext httpContext)
    {
        TraceIdentifier = httpContext.TraceIdentifier;
        DisplayUrl = httpContext.Request.GetDisplayUrl();
        Method = httpContext.Request.Method;
        Scheme = httpContext.Request.Scheme;
        Host = httpContext.Request.Host;
        Path = httpContext.Request.Path;
        QueryString = httpContext.Request.QueryString;
        Protocol = httpContext.Request.Protocol;
        RemoteIpAddress = httpContext.Connection.RemoteIpAddress;
        RemotePort = httpContext.Connection.RemotePort;
        RequestHeaders = httpContext.Request.Headers;
        StatusCode = httpContext.Response.StatusCode;
        ResponseHeaders = httpContext.Response.Headers;
    }

    public string TraceIdentifier { get; }

    public string DisplayUrl { get; }

    public string Method { get; }

    public string Scheme { get; }

    public HostString Host { get; }

    public PathString Path { get; }

    public QueryString QueryString { get; }

    public string Protocol { get; }

    public IPAddress? RemoteIpAddress { get; }

    public int RemotePort { get; }

    public IHeaderDictionary RequestHeaders { get; }

    public int StatusCode { get; }

    public IHeaderDictionary ResponseHeaders { get; }
}