using TUnit.Assertions.Enums;

namespace Pattrn.Routing.Tests;

public sealed class RouteDetailedSliceTests
{
    [Test]
    public async Task CallerBufferDetailedCapturesReferenceSplitRouteSegments()
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

        await Assert.That(matchCount).IsEqualTo(1);
        await Assert.That(captureCount).IsEqualTo(1);
        await Assert.That(captures[0]).IsEqualTo(new PatternCaptureSlice<string>("path", 1, 2));
        await Assert.That(segments.AsSpan(captures[0].StartSegmentIndex, captures[0].SegmentCount).ToArray())
            .IsEquivalentTo(["a", "b.txt"], CollectionOrdering.Matching);
    }
}
