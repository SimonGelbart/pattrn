using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class DetailedMatchDefaultValueTests
{
    [Test]
    public void DefaultPatternCaptureUsesStableValueSemantics()
    {
        var first = default(PatternCapture<string>);
        var second = default(PatternCapture<string>);

        ShouldEqual(first.SegmentCount, 0);
        ShouldBeTrue(first.Equals(second), "Expected default captures to compare equal.");
        ShouldBeTrue(first == second, "Expected the capture equality operator to handle default values.");
        ShouldEqual(first.GetHashCode(), second.GetHashCode());
        ShouldThrow<InvalidOperationException>(() => _ = first.Value);
    }

    [Test]
    public void DefaultPatternMatchDetailedUsesStableValueSemantics()
    {
        var first = default(PatternMatchDetailed<string, string>);
        var second = default(PatternMatchDetailed<string, string>);

        ShouldBeTrue(first.Equals(second), "Expected default detailed matches to compare equal.");
        ShouldBeTrue(first == second, "Expected the detailed-match equality operator to handle default values.");
        ShouldEqual(first.GetHashCode(), second.GetHashCode());
    }

    [Test]
    public void CallerBufferCatchAllUsesOneCaptureSlice()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .AddPattern([PatternSegment<string>.Literal("files"), PatternSegment<string>.CatchAll("path")], "handler")
            .Build();
        var path = new[] { "files", "a", "b.txt" };
        var matches = new PatternMatch<string>[index.GetMatchCountUpperBound(path)];
        var captures = new PatternCaptureSlice<string>[index.GetCaptureCountUpperBound(path)];

        var matchCount = index.MatchDetailed(path, matches, captures, out var captureCount);

        ShouldEqual(matchCount, 1);
        ShouldEqual(captureCount, 1);
        ShouldEqual(matches[0].CaptureCount, 1);
        ShouldEqual(captures[0], new PatternCaptureSlice<string>("path", 1, 2));
    }

    [Test]
    public void CallerBufferEmptyCatchAllUsesOneEmptyCaptureSlice()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .AddPattern([PatternSegment<string>.Literal("files"), PatternSegment<string>.CatchAll("path")], "handler")
            .Build();
        var path = new[] { "files" };
        var matches = new PatternMatch<string>[index.GetMatchCountUpperBound(path)];
        var captures = new PatternCaptureSlice<string>[index.GetCaptureCountUpperBound(path)];

        var matchCount = index.MatchDetailed(path, matches, captures, out var captureCount);

        ShouldEqual(matchCount, 1);
        ShouldEqual(captureCount, 1);
        ShouldEqual(matches[0].CaptureCount, 1);
        ShouldEqual(captures[0], new PatternCaptureSlice<string>("path", 1, 0));
    }
}
