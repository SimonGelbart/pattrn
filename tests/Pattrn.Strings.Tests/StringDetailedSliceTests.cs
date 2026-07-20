using TUnit.Assertions.Enums;

namespace Pattrn.Strings.Tests;

public sealed class StringDetailedSliceTests
{
    [Test]
    public async Task CallerBufferDetailedCapturesReferenceNormalizedSegments()
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
        var matches = new PatternMatchDetailedSlice<string>[index.GetMatchCountUpperBound(path)];
        var captures = new PatternCaptureSlice<string>[index.GetCaptureCountUpperBound(path)];

        var matchCount = index.MatchDetailed(path, matches, captures, out var captureCount);
        var segments = index.Options.Split(path);

        await Assert.That(matchCount).IsEqualTo(1);
        await Assert.That(captureCount).IsEqualTo(2);
        await Assert.That(captures[0]).IsEqualTo(new PatternCaptureSlice<string>("exchange", 1, 1));
        await Assert.That(captures[1]).IsEqualTo(new PatternCaptureSlice<string>("symbol", 2, 2));
        await Assert.That(segments.AsSpan(captures[1].StartSegmentIndex, captures[1].SegmentCount).ToArray())
            .IsEquivalentTo(["MSFT", "QUOTE"], CollectionOrdering.Matching);
    }
}
