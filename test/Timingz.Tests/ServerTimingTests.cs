using FluentAssertions;
using Xunit;

namespace Timingz.Tests;

public class ServerTimingTests
{
    private const string MetricName = "foo";

    [Theory]
    [MemberData(nameof(AddMetricActions))]
    public void MetricsAreAddedToCollection(AddMetricTestCase testCase)
    {
        var serverTiming = new ServerTiming();
        testCase.AddMetric(serverTiming, MetricName);
        serverTiming.GetMetrics().Should().ContainSingle(m => m.Name == MetricName);
    }

    [Theory]
    [MemberData(nameof(AddMetricActions))]
    public void CanCreateMetricWithDuplicateName(AddMetricTestCase testCase)
    {
        var serverTiming = new ServerTiming();
        serverTiming.Marker(MetricName);
        var add = () => testCase.AddMetric(serverTiming, MetricName);
        add.Should().NotThrow();
    }

    [Theory]
    [MemberData(nameof(AddMetricActions))]
    public void CannotCreateMetricWithEmptyName(AddMetricTestCase testCase)
    {
        var serverTiming = new ServerTiming();
        serverTiming.Marker(MetricName);
        var add = () => testCase.AddMetric(serverTiming, string.Empty);
        add.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("name");
    }

    [Theory]
    [MemberData(nameof(AddMetricActions))]
    public void CannotCreateMetricWithNullName(AddMetricTestCase testCase)
    {
        var serverTiming = new ServerTiming();
        serverTiming.Marker(MetricName);
        var add = () => testCase.AddMetric(serverTiming, null);
        add.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("name");
    }

    [Theory]
    [MemberData(nameof(AddMetricActions))]
    public void CannotCreateMetricWithWhitespaceName(AddMetricTestCase testCase)
    {
        var serverTiming = new ServerTiming();
        serverTiming.Marker(MetricName);
        var add = () => testCase.AddMetric(serverTiming, " ");
        add.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("name");
    }

    [Fact]
    public void GetMetricsReturnsAllMetrics()
    {
        var serverTiming = new ServerTiming();
        for (var i = 0; i < 5; i++)
            serverTiming.Marker($"{MetricName}-{i}");

        serverTiming.GetMetrics().Should().HaveCount(5)
            .And.OnlyContain(m => m.Name.StartsWith(MetricName));
    }

    public static IEnumerable<object[]> AddMetricActions()
    {
        yield return new object[]
        {
            new AddMetricTestCase(nameof(IServerTiming.Marker), (st, n) => st.Marker(n))
        };
        yield return new object[]
        {
            new AddMetricTestCase(nameof(IServerTiming.Disposable), (st, n) => st.Disposable(n))
        };
        yield return new object[]
        {
            new AddMetricTestCase(nameof(IServerTiming.Manual), (st, n) => st.Manual(n))
        };
        yield return new object[]
        {
            new AddMetricTestCase(nameof(IServerTiming.Precalculated), (st, n) => st.Precalculated(n, 1.23))
        };
    }

    public class AddMetricTestCase
    {
        private readonly string _displayName;

        public AddMetricTestCase(string displayName, Action<IServerTiming, string> addMetric)
        {
            _displayName = displayName;
            AddMetric = addMetric;
        }

        public Action<IServerTiming, string> AddMetric { get; }

        public override string ToString() => _displayName;
    }
}