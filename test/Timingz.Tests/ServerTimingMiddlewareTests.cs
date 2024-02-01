using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Timingz.Tests;

public sealed class ServerTimingMiddlewareTests : IDisposable
{
    private const string DurationParameterName = "dur";
    private const string DescriptionParameterName = "desc";
    private const string CustomMetricName = "my-name";
    private const string CustomMetricDescription = "my-description";

    private IEnumerable<IServerTimingCallback> _callbackServices;
    private readonly TestLogger<ServerTimingMiddleware> _logger = new();
    private readonly ActivityMonitor _activityMonitor = new(A.Fake<IHttpContextAccessor>());
    private readonly MeterMonitor _meterMonitor = new(A.Fake<IHttpContextAccessor>());

    [Fact]
    public void ConstructorThrowsWhenRequestDelegateNull()
    {
        var ctr = () => new ServerTimingMiddleware(
            null!,
            new ServerTimingOptions(),
            _activityMonitor,
            _meterMonitor,
            _logger);
        ctr.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("next");
    }

    [Fact]
    public void ConstructorThrowsWhenOptionsNull()
    {
        var ctr = () => new ServerTimingMiddleware(
            _ => Task.CompletedTask,
            null!,
            _activityMonitor,
            _meterMonitor,
            _logger);
        ctr.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("options");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RequestTotalMetricAddedWhenEnabled(bool writeHeader)
    {
        using var host = await BuildHost(writeHeader);
        var client = host.GetTestClient();

        var response = await client.GetAsync("/");

        var values = ParseTimingHeader(response);
        values.ContainsKey(ServerTimingOptions.DefaultTotalMetricName).Should().Be(writeHeader);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CustomMetricAddedWhenHeaderAndCustomMetricsEnabled(bool writeHeader)
    {
        using var host = await BuildHost(
            writeHeader,
            includeCustomMetrics: true,
            addTimings: serverTiming => serverTiming.Precalculated(CustomMetricName, 123.456, CustomMetricDescription));
        var client = host.GetTestClient();

        var response = await client.GetAsync("/");

        var values = ParseTimingHeader(response);
        values.ContainsKey(CustomMetricName).Should().Be(writeHeader);
        values.ContainsKey(ServerTimingOptions.DefaultTotalMetricName).Should().Be(writeHeader);
    }

    [Fact]
    public async Task CustomMetricExcludedWhenCustomMetricsDisabled()
    {
        using var host = await BuildHost(
            includeCustomMetrics: false,
            addTimings: serverTiming => serverTiming.Precalculated(CustomMetricName, 123.456, CustomMetricDescription));
        var client = host.GetTestClient();

        var response = await client.GetAsync("/");

        var values = ParseTimingHeader(response);
        values.ContainsKey(CustomMetricName).Should().BeFalse();
        values.ContainsKey(ServerTimingOptions.DefaultTotalMetricName).Should().BeTrue();
    }

    [Fact]
    public async Task HeaderNotWrittenWhenTimingDisabled()
    {
        using var host = await BuildHost(false);
        var client = host.GetTestClient();

        var response = await client.GetAsync("/");

        var values = ParseTimingHeader(response);
        values.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("https://origin.com")]
    public async Task ValidAllowTimingOriginsApplied(string origin)
    {
        var includeOrigins = !string.IsNullOrEmpty(origin);
        var timingAllowOrigins = includeOrigins ? new List<string> { origin } : null;
        using var host = await BuildHost(timingAllowOrigins: timingAllowOrigins);
        var client = host.GetTestClient();

        var response = await client.GetAsync("/");

        var headers = response.Headers;
        headers.Contains(HeaderWriter.TimingAllowOriginHeaderName).Should().Be(includeOrigins);
    }

    [Fact]
    public async Task TotalMetricMetricConfigurationApplied()
    {
        using var host = await BuildHost(
            includeDescriptions: true,
            totalMetricName: CustomMetricName,
            totalMetricDescription: CustomMetricDescription);
        var client = host.GetTestClient();

        var response = await client.GetAsync("/");

        var values = ParseTimingHeader(response);
        values.Should().ContainKey(CustomMetricName)
            .WhoseValue.Should().ContainKey(DescriptionParameterName)
            .WhoseValue.Should().Be($"\"{CustomMetricDescription}\"");
    }

    [Theory]
    [MemberData(nameof(GetCustomMetrics))]
    public async Task WritesCustomMetricsToHeader(Action<IServerTiming> addTimings)
    {
        using var host = await BuildHost(
            includeCustomMetrics: true,
            includeDescriptions: true,
            addTimings: addTimings);
        var client = host.GetTestClient();

        var response = await client.GetAsync("/");

        var values = ParseTimingHeader(response);

        values.Should().ContainKey(CustomMetricName)
            .WhoseValue.Should().ContainKey(DurationParameterName);

        values.Should().ContainKey(CustomMetricName)
            .WhoseValue.Should().ContainKey(DescriptionParameterName)
            .WhoseValue.Should().Be($"\"{CustomMetricDescription}\"");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ValidatesManualMetricsBeforeWriting(bool validateMetrics)
    {
        const string metricName = "ForgotToStop";

        using var host = await BuildHost(
            true,
            validateMetrics,
            addTimings: serverTiming => serverTiming.Manual(metricName));
        var client = host.GetTestClient();

        await client.GetAsync("/");

        _logger.GetLogMessages()
            .Any(m => m.LogLevel == LogLevel.Warning && m.Message.Contains(metricName))
            .Should().Be(validateMetrics);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task InvokesCallbackWhenResponseCompleted(bool writeHeader)
    {
        using var host = await BuildHost(writeHeader, invokeCallbackServices: true);
        var client = host.GetTestClient();

        await client.GetAsync("/");

        var callback = _callbackServices.OfType<CallbackThatRecordsEvent>().Single();
        callback.Event.Should().NotBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DoesNotInvokeCallbackWhenDisabled(bool writeHeader)
    {
        using var host = await BuildHost(writeHeader);
        var client = host.GetTestClient();

        await client.GetAsync("/");

        var callback = _callbackServices.OfType<CallbackThatRecordsEvent>().Single();
        callback.Event.Should().BeNull();
    }

    [Fact]
    public async Task DoesNotThrowWhenCallbacksEnabledButNoServicesRegistered()
    {
        using var host = await BuildHost(registerCallbackServices: false, invokeCallbackServices: true);
        var client = host.GetTestClient();

        Func<Task> invoke = async () => await client.GetAsync("/");

        await invoke.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CallbackExceptionsIndividuallyLogged()
    {
        using var host = await BuildHost(invokeCallbackServices: true, throwCallbackException: true);
        var client = host.GetTestClient();

        await client.GetAsync("/");

        var logMessages = _logger.GetLogMessages();
        while (logMessages.Count < 2)
        {
            await Task.Delay(10); // Give the server time to run the OnCompleted handler for the response.
            logMessages = _logger.GetLogMessages();
        }

        logMessages.Should().Contain(m => m.LogLevel == LogLevel.Error && m.Exception.Message == "A");
        logMessages.Should().Contain(m => m.LogLevel == LogLevel.Error && m.Exception.Message == "B");
    }

    private Task<IHost> BuildHost(
        bool writerHeader = true,
        bool validateMetrics = false,
        bool invokeCallbackServices = false,
        bool registerCallbackServices = true,
        bool throwCallbackException = false,
        bool includeDescriptions = false,
        bool includeCustomMetrics = false,
        IEnumerable<string> timingAllowOrigins = null,
        string totalMetricName = ServerTimingOptions.DefaultTotalMetricName,
        string totalMetricDescription = ServerTimingOptions.DefaultTotalDescription,
        Action<IServerTiming> addTimings = null) =>
        new HostBuilder()
            .ConfigureWebHost(webBuilder => webBuilder
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddServerTiming()
                        .AddSingleton<ILogger<ServerTimingMiddleware>>(_logger);

                    if (registerCallbackServices)
                    {
                        services.AddScoped<IServerTimingCallback, CallbackThatRecordsEvent>()
                            .AddScoped<IServerTimingCallback>(_ =>
                                new CallbackThatThrowsException("A", throwCallbackException))
                            .AddScoped<IServerTimingCallback>(_ =>
                                new CallbackThatThrowsException("B", throwCallbackException));
                    }
                })
                .Configure(app => app
                    .UseServerTiming(options =>
                    {
                        options.InvokeCallbackServices = invokeCallbackServices;
                        options.ValidateMetrics = validateMetrics;
                        options.TimingAllowOrigins = timingAllowOrigins ?? Array.Empty<string>();
                        options.TotalMetricName = totalMetricName;
                        options.TotalMetricDescription = totalMetricDescription;
                        options.WithRequestTimingOptions((_, timingOptions) =>
                        {
                            timingOptions.WriteHeader = writerHeader;
                            timingOptions.IncludeDescriptions = includeDescriptions;
                            timingOptions.IncludeCustomMetrics = includeCustomMetrics;
                        });
                    })
                    .Use(async (context, next) =>
                    {
                        _callbackServices = context.RequestServices.GetServices<IServerTimingCallback>();
                        var serverTiming = context.RequestServices.GetService<IServerTiming>();
                        addTimings?.Invoke(serverTiming);
                        await next();
                    })))
            .StartAsync();

    private static Dictionary<string, Dictionary<string, string>> ParseTimingHeader(HttpResponseMessage response)
    {
        var headers = response.Headers;
        if (!headers.TryGetValues(HeaderWriter.ServerTimingHeaderName, out var values))
            return new Dictionary<string, Dictionary<string, string>>();

        var header = values.FirstOrDefault();

        return string.IsNullOrEmpty(header)
            ? new Dictionary<string, Dictionary<string, string>>()
            : header.Split(",")
                .Select(m => m.Split(";"))
                .ToDictionary(
                    v => v[0],
                    v => v.Skip(1)
                        .Select(p => p.Split("="))
                        .ToDictionary(p2 => p2[0], p2 => p2[1]));
    }

    public static IEnumerable<object[]> GetCustomMetrics()
    {
        yield return new object[]
        {
            new Action<IServerTiming>(timing =>
            {
                var manual = timing.Manual(CustomMetricName, CustomMetricDescription);
                manual.Start();
                manual.Stop();
            })
        };

        yield return new object[]
        {
            new Action<IServerTiming>(timing =>
            {
                var disposable = timing.Disposable(CustomMetricName, CustomMetricDescription);
                disposable.Dispose();
            })
        };

        yield return new object[]
        {
            new Action<IServerTiming>(timing =>
            {
                timing.Precalculated(CustomMetricName, 123.456, CustomMetricDescription);
            })
        };
    }

    private sealed class LogMessage
    {
        public LogMessage(LogLevel logLevel, string message, Exception exception)
        {
            LogLevel = logLevel;
            Message = message;
            Exception = exception;
        }

        public LogLevel LogLevel { get; }

        public string Message { get; }

        public Exception Exception { get; }
    }

    private sealed class TestLogger<TName> : ILogger<TName>
    {
        private readonly List<LogMessage> _logMessages = [];

#pragma warning disable CA1859
        public IReadOnlyList<LogMessage> GetLogMessages()
        {
            lock (_logMessages) return _logMessages.AsReadOnly();
        }
#pragma warning restore CA1859

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            var logMessage = new LogMessage(logLevel, message, exception);
            lock (_logMessages) _logMessages.Add(logMessage);
        }
    }

    private sealed class CallbackThatThrowsException : IServerTimingCallback
    {
        private readonly string _message;
        private readonly bool _throwException;

        public CallbackThatThrowsException(string message, bool throwException)
        {
            _message = message;
            _throwException = throwException;
        }

        public async Task OnServerTiming(ServerTimingEvent _)
        {
            await Task.Yield();
            if (_throwException) throw new InvalidOperationException(_message);
        }
    }

    private sealed class CallbackThatRecordsEvent : IServerTimingCallback
    {
        internal ServerTimingEvent Event { get; private set; }

        public Task OnServerTiming(ServerTimingEvent serverTimingEvent)
        {
            Event = serverTimingEvent;
            return Task.CompletedTask;
        }
    }

    public void Dispose()
    {
        _activityMonitor?.Dispose();
        _meterMonitor?.Dispose();
    }
}