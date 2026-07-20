using static Pattrn.Routing.Tests.TestAssertions;

namespace Pattrn.Routing.Tests;

public sealed class RouteDetailedSliceTests
{
    [Test]
    public void CallerBufferDetailedCapturesReferenceSplitRouteSegments()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .AddRoute("/files/{*path}", "handler")
            .Build();
        const string path = "/files/a/b.txt";
        var matches = new PatternMatchDetailedSlice<string>[index.GetRouteMatchCountUpperBound(path)];
        var captures = new PatternCaptureSlice<string>[index.GetRouteCaptureCountUpperBound(path)];

        var matchCount = index.MatchRouteDetailed(path, matches, captures, out var captureCount);
        var segments = RoutePattern.SplitPath(path);

        ShouldEqual(matchCount, 1);
        ShouldEqual(captureCount, 1);
        ShouldEqual(captures[0], new PatternCaptureSlice<string>("path", 1, 2));
        ShouldSequenceEqual(
            segments.AsSpan(captures[0].StartSegmentIndex, captures[0].SegmentCount).ToArray(),
            ["a", "b.txt"]);
    }
}
