#pragma warning disable CA1852

using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Timingz;
using WebApiSample;
using WebApiSample.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add the required services to the dependency injection container.
// The IServerTiming service is scoped the HTTP request and is available for dependency injection.
builder.Services.AddServerTiming();

// Callback services can be registered that will be invoked after the request is completed.
// These can have a lifetime that is scoped to the request or a singleton.
// The event received will contain the HttpContext and a list of metrics that were recorded.
// You can use this to send the collected metrics to another service or metrics package.
builder.Services.AddScoped<IServerTimingCallback, SampleServerTimingCallback>();

// Configure OpenTelemetry tracing to see our custom Activity
// being exported and included in the Server-Timing header.
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder
            .CreateDefault()
            .AddService(builder.Environment.ApplicationName))
        .AddSource(Telemetry.Source.Name)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());

builder.Services.Configure<AspNetCoreInstrumentationOptions>(options =>
    options.RecordException = true);

builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeScopes = true;
    options.ParseStateValues = true;
    options.IncludeFormattedMessage = true;
    options.AddConsoleExporter();
});

var app = builder.Build();

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Add the middleware before UseEndpoints.
app.UseServerTiming(options =>
{
    // Use the callback below to configure the per-request options. You can use the provided HttpContext to tailor
    // the options for individual requests or statically define the options and have them apply to all requests.
    options.WithRequestTimingOptions((httpContext, requestOptions) =>
    {
        // The sample uses query string parameters to provide an easy way of toggling the settings
        // and seeing the resulting change in behaviour. Replace these with something more appropriate
        // for a production quality application.
        var query = httpContext.Request.Query;

        // Server-Timing headers could expose sensitive information and must be explicitly enabled.
        // The HttpContext is available in this callback so you can configure this per request if required.
        requestOptions.WriteHeader = query.ContainsKey("timing");

        // The metric descriptions increase the Server-Timing header size and must be explicitly enabled.
        // The HttpContext is available in this callback so you can configure this per request if required.
        requestOptions.IncludeDescriptions = query.ContainsKey("desc");

        // Custom metrics could expose sensitive information in the Server-Timing header and must be explicitly enabled.
        // You can choose to include additional metrics (other than the request total duration) conditionally.
        requestOptions.IncludeCustomMetrics = query.ContainsKey("custom");
    });

    // Add any required origins for the Timing-Allow-Origin header.
    options.TimingAllowOrigins = new[] { "https://example.com" };

    // You may want to invoke the callbacks with the timings to record them elsewhere,
    // even if you do not want to send the Server-Timing header in the response.
    // This option also allows you to register callback services unconditionally and only
    // have them invoked when required.
    options.InvokeCallbackServices = true;

    // When this setting is enabled the middleware will throw an exception if a metric has not been used correctly.
    // For example, a manual metric was started but not stopped, or a disposable metric was not disposed.
    // This is intended to help you find incorrect logic in your code and prevent invalid metrics from being sent.
    // You may choose to enable this feature only in a local or development environment.
    // If the request has a status code of 500 the metrics will not be validated to avoid further exceptions.
    options.ValidateMetrics = app.Environment.IsDevelopment();

    // Add the Activity Source that should be monitored for AddServerTiming calls.
    options.ActivitySources.Add(Telemetry.Source.Name);
});

app.Run();