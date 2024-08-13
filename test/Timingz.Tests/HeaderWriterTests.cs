using System.Text.Json;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Timingz.Tests;

public class HeaderWriterTests
{
    [Theory]
    [MemberData(nameof(GetMetrics))]
    public void WriteHeaders(MetricTestCase testCase)
    {
        var metrics = testCase.Metrics;
        var includeDescription = testCase.IncludeDescription;
        var timingAllowOrigins = testCase.TimingAllowOrigins;
        var expectedServerTiming = testCase.ExpectedServerTiming;
        var expectedTimingAllowOrigin = testCase.ExpectedTimingAllowOrigin;

        var headerWriter = new HeaderWriter(timingAllowOrigins, ServerTimingOptions.DefaultDurationPrecision);
        var headers = new HeaderDictionary();
        headerWriter.WriteHeaders(headers, includeDescription, metrics);

        headers[HeaderWriter.ServerTimingHeaderName].ToString().Should().Be(expectedServerTiming);

        if (expectedTimingAllowOrigin == null)
            headers.ContainsKey(HeaderWriter.TimingAllowOriginHeaderName).Should().BeFalse();
        else
            headers[HeaderWriter.TimingAllowOriginHeaderName].ToString().Should().Be(expectedTimingAllowOrigin);
    }

    [Theory]
    [MemberData(nameof(GetCharacters))]
    public void EscapesMetricName(char character)
    {
        var headerWriter = new HeaderWriter([], ServerTimingOptions.DefaultDurationPrecision);
        var name = $"metric{character}name";
        var metric = new PrecalculatedMetric(name, 1.0d);
        var headers = new HeaderDictionary();

        headerWriter.WriteHeaders(headers, false, [metric]);

        var value = headers[HeaderWriter.ServerTimingHeaderName].ToString();
        var regex = HeaderWriter.InvalidTokenCharacters();
        var expectedName = regex.IsMatch([character]) ? "metric_name" : name;
        value.Should().Be($"{expectedName};dur=1");
    }

    public static TheoryData<MetricTestCase> GetMetrics()
    {
        const string origin1 = "https://origin1.com/";
        const string origin2 = "https://origin2.com/";
        var timingAllowOrigins = new[] { origin1, origin2 };
        var emptyTimingAllowOrigins = Array.Empty<string>();

        var metric1Dur = Math.Round(1.2345d, ServerTimingOptions.DefaultDurationPrecision);
        const string metric1 = "foo";
        const string metric1Desc = $"best {metric1} ever";

        var metric2Dur = Math.Round(4.5678d, ServerTimingOptions.DefaultDurationPrecision);
        const string metric2 = "bar";
        const string metric2Desc = $"best {metric2} ever";

        var theoryData = new TheoryData<MetricTestCase>();

        theoryData.Add(new MetricTestCase(
            [],
            true,
            emptyTimingAllowOrigins,
            string.Empty));

        theoryData.Add(new MetricTestCase(
            [new PrecalculatedMetric(metric1, metric1Dur, metric1Desc)],
            true,
            emptyTimingAllowOrigins,
            $"{metric1};dur={metric1Dur};desc=\"{metric1Desc}\""));

        theoryData.Add(new MetricTestCase(
            [new PrecalculatedMetric(metric1, metric1Dur, metric1Desc)],
            false,
            emptyTimingAllowOrigins,
            $"{metric1};dur={metric1Dur}"));

        theoryData.Add(new MetricTestCase(
            [new PrecalculatedMetric("name with space", metric1Dur)],
            false,
            emptyTimingAllowOrigins,
            $"name_with_space;dur={metric1Dur}"));

        theoryData.Add(new MetricTestCase(
            [new PrecalculatedMetric("name/with\\slash", metric1Dur)],
            false,
            emptyTimingAllowOrigins,
            $"name_with_slash;dur={metric1Dur}"));

        theoryData.Add(new MetricTestCase(
            [
                new PrecalculatedMetric(metric1, metric1Dur, metric1Desc),
                new PrecalculatedMetric(metric2, metric2Dur, metric2Desc)
            ],
            true,
            emptyTimingAllowOrigins,
            $"{metric1};dur={metric1Dur};desc=\"{metric1Desc}\",{metric2};dur={metric2Dur};desc=\"{metric2Desc}\""));

        theoryData.Add(new MetricTestCase(
            [
                new PrecalculatedMetric(metric1, metric1Dur, metric1Desc),
                new PrecalculatedMetric(metric2, metric2Dur, metric2Desc)
            ],
            false,
            emptyTimingAllowOrigins,
            $"{metric1};dur={metric1Dur},{metric2};dur={metric2Dur}"));

        theoryData.Add(new MetricTestCase(
            [new Metric(metric1, metric1Desc)],
            true,
            emptyTimingAllowOrigins,
            $"{metric1};desc=\"{metric1Desc}\""));

        theoryData.Add(new MetricTestCase(
            [new Metric(metric1, metric1Desc)],
            false,
            emptyTimingAllowOrigins,
            metric1));

        theoryData.Add(new MetricTestCase(
            [
                new Metric(metric1, metric1Desc),
                new Metric(metric2, metric2Desc)
            ],
            true,
            emptyTimingAllowOrigins,
            $"{metric1};desc=\"{metric1Desc}\",{metric2};desc=\"{metric2Desc}\""));

        theoryData.Add(new MetricTestCase(
            [
                new Metric(metric1, metric1Desc),
                new Metric(metric2, metric2Desc)
            ],
            false,
            emptyTimingAllowOrigins,
            $"{metric1},{metric2}"));

        theoryData.Add(new MetricTestCase(
            [
                new PrecalculatedMetric(metric1, metric1Dur, metric1Desc),
                new Metric(metric2, metric2Desc)
            ],
            true,
            emptyTimingAllowOrigins,
            $"{metric1};dur={metric1Dur};desc=\"{metric1Desc}\",{metric2};desc=\"{metric2Desc}\""));

        theoryData.Add(new MetricTestCase(
            [
                new PrecalculatedMetric(metric1, metric1Dur, metric1Desc),
                new Metric(metric2, metric2Desc)
            ],
            false,
            emptyTimingAllowOrigins,
            $"{metric1};dur={metric1Dur},{metric2}"));

        theoryData.Add(new MetricTestCase(
            [new PrecalculatedMetric(metric1, metric1Dur, metric1Desc)],
            false,
            timingAllowOrigins,
            $"{metric1};dur={metric1Dur}",
            $"{origin1},{origin2}"));

        return theoryData;
    }

    public static TheoryData<char> GetCharacters()
    {
        var data = new TheoryData<char>();
        for (var index = 0; index <= 127; index++)
        {
            var character = (char)index;
            if (char.IsControl(character)) continue;
            data.Add(character);
        }
        return data;
    }

    public class MetricTestCase : IXunitSerializable
    {
        // ReSharper disable once UnusedMember.Global
        public MetricTestCase()
        {
        }

        public MetricTestCase(
            IMetric[] metrics,
            bool includeDescription,
            string[] timingAllowOrigins,
            string expectedServerTiming,
            string expectedTimingAllowOrigin = null)
        {
            Metrics = metrics;
            IncludeDescription = includeDescription;
            TimingAllowOrigins = timingAllowOrigins;
            ExpectedServerTiming = expectedServerTiming;
            ExpectedTimingAllowOrigin = expectedTimingAllowOrigin;
        }

        public IMetric[] Metrics { get; private set; }
        public bool IncludeDescription { get; private set; }
        public string[] TimingAllowOrigins { get; private set; }
        public string ExpectedServerTiming { get; private set; }
        public string ExpectedTimingAllowOrigin { get; private set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            var data = info.GetValue<string>("data");
            var testCase = JsonSerializer.Deserialize<MetricTestCase>(data);
            Metrics = testCase.Metrics;
            IncludeDescription = testCase.IncludeDescription;
            TimingAllowOrigins = testCase.TimingAllowOrigins;
            ExpectedServerTiming = testCase.ExpectedServerTiming;
            ExpectedTimingAllowOrigin = testCase.ExpectedTimingAllowOrigin;
        }

        public void Serialize(IXunitSerializationInfo info) =>
            info.AddValue("data", JsonSerializer.Serialize(this));
    }
}