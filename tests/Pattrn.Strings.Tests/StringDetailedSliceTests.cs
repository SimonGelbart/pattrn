using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Strings.Tests;

public sealed class StringDetailedSliceTests
{
    [Test]
    public void CallerBufferDetailedCapturesReferenceNormalizedSegments()
    {
        var index = StringPattrnIndexBuilder
            .CreateDotted<string>()
            .AddPattern(
                [
                    PatternSegment<string>.Literal("market"),
                    PatternSegment<string>.Parameter("exchange"),
                    PatternSegment<string>.CatchAll("symbol")
                ],
                "handler")
            .Build();
        const string path = "market.NASDAQ.MSFT.QUOTE";
        var matches = new PatternMatch<string>[index.GetMatchCountUpperBound(path)];
        var captures = new PatternCaptureSlice<string>[index.GetCaptureCountUpperBound(path)];

        var matchCount = index.MatchDetailed(path, matches, captures, out var captureCount);
        var segments = index.Options.Split(path);

        ShouldEqual(matchCount, 1);
        ShouldEqual(captureCount, 2);
        ShouldEqual(captures[0], new PatternCaptureSlice<string>("exchange", 1, 1));
        ShouldEqual(captures[1], new PatternCaptureSlice<string>("symbol", 2, 2));
        ShouldSequenceEqual(
            segments.AsSpan(captures[1].StartSegmentIndex, captures[1].SegmentCount).ToArray(),
            ["MSFT", "QUOTE"]);
    }
}
