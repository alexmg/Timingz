using FluentAssertions;
using Xunit;

namespace Timingz.Tests;

public class HeaderWriterTests
{
    [Theory]
    [MemberData(nameof(GetMetrics))]
    public void WriteHeaders(
        IReadOnlyList<IMetric> metrics,
        bool includeDescription,
        string[] timingAllowOrigins,
        string expectedServerTiming,
        string expectedTimingAllowOrigin = null)
    {
        var headerWriter = new HeaderWriter(timingAllowOrigins);
        var headers = new HeaderDictionary();
        headerWriter.WriteHeaders(headers, includeDescription, metrics);

        headers[HeaderWriter.ServerTimingHeaderName].ToString().Should().Be(expectedServerTiming);

        if (expectedTimingAllowOrigin == null)
            headers.ContainsKey(HeaderWriter.TimingAllowOriginHeaderName).Should().BeFalse();
        else
            headers[HeaderWriter.TimingAllowOriginHeaderName].ToString().Should().Be(expectedTimingAllowOrigin);
    }

    public static IEnumerable<object[]> GetMetrics()
    {
        const string origin1 = "https://origin1.com/";
        const string origin2 = "https://origin2.com/";
        var timingAllowOrigins = new[] { origin1, origin2 };
        var emptyTimingAllowOrigins = Array.Empty<string>();

        const string metric1 = "foo";
        const double metric1Dur = 1.23d;
        const string metric1Desc = $"best {metric1} ever";

        const string metric2 = "bar";
        const double metric2Dur = 4.56d;
        const string metric2Desc = $"best {metric2} ever";

        yield return new object[]
        {
            Array.Empty<IMetric>(),
            true,
            emptyTimingAllowOrigins,
            string.Empty
        };

        yield return new object[]
        {
            new[] { new PrecalculatedMetric(metric1, metric1Dur, metric1Desc) },
            true,
            emptyTimingAllowOrigins,
            $"{metric1};dur={metric1Dur};desc=\"{metric1Desc}\""
        };

        yield return new object[]
        {
            new[] { new PrecalculatedMetric(metric1, metric1Dur, metric1Desc) },
            false,
            emptyTimingAllowOrigins,
            $"{metric1};dur={metric1Dur}"
        };

        yield return new object[]
        {
            new[]
            {
                new PrecalculatedMetric(metric1, metric1Dur, metric1Desc),
                new PrecalculatedMetric(metric2, metric2Dur, metric2Desc)
            },
            true,
            emptyTimingAllowOrigins,
            $"{metric1};dur={metric1Dur};desc=\"{metric1Desc}\",{metric2};dur={metric2Dur};desc=\"{metric2Desc}\""
        };

        yield return new object[]
        {
            new[]
            {
                new PrecalculatedMetric(metric1, metric1Dur, metric1Desc),
                new PrecalculatedMetric(metric2, metric2Dur, metric2Desc)
            },
            false,
            emptyTimingAllowOrigins,
            $"{metric1};dur={metric1Dur},{metric2};dur={metric2Dur}"
        };

        yield return new object[]
        {
            new[] { new Metric(metric1, metric1Desc) },
            true,
            emptyTimingAllowOrigins,
            $"{metric1};desc=\"{metric1Desc}\""
        };

        yield return new object[]
        {
            new[] { new Metric(metric1, metric1Desc) },
            false,
            emptyTimingAllowOrigins,
            metric1
        };

        yield return new object[]
        {
            new[]
            {
                new Metric(metric1, metric1Desc),
                new Metric(metric2, metric2Desc)
            },
            true,
            emptyTimingAllowOrigins,
            $"{metric1};desc=\"{metric1Desc}\",{metric2};desc=\"{metric2Desc}\""
        };

        yield return new object[]
        {
            new[]
            {
                new Metric(metric1, metric1Desc),
                new Metric(metric2, metric2Desc)
            },
            false,
            emptyTimingAllowOrigins,
            $"{metric1},{metric2}"
        };

        yield return new object[]
        {
            new[]
            {
                new PrecalculatedMetric(metric1, metric1Dur, metric1Desc),
                new Metric(metric2, metric2Desc)
            },
            true,
            emptyTimingAllowOrigins,
            $"{metric1};dur={metric1Dur};desc=\"{metric1Desc}\",{metric2};desc=\"{metric2Desc}\""
        };

        yield return new object[]
        {
            new[]
            {
                new PrecalculatedMetric(metric1, metric1Dur, metric1Desc),
                new Metric(metric2, metric2Desc)
            },
            false,
            emptyTimingAllowOrigins,
            $"{metric1};dur={metric1Dur},{metric2}"
        };

        yield return new object[]
        {
            new[] { new PrecalculatedMetric(metric1, metric1Dur, metric1Desc) },
            false,
            timingAllowOrigins,
            $"{metric1};dur={metric1Dur}",
            $"{origin1},{origin2}"
        };
    }
}