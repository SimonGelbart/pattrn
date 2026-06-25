using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class PerformanceSmokeTests
{
    [Test]
    public void MatchToCallerProvidedSpanAllocatesZeroBytes()
    {
        var state = CreateValueMatchState();

        var written = state.Index.Match(state.Path, state.Values);
        ShouldEqual(written, 4);

        var measured = GetAllocatedBytes(static state => state.Index.Match(state.Path, state.Values), state);

        ShouldEqual(measured.Result, 4);
        ShouldEqual(measured.Allocated, 0L);
    }

    [Test]
    public void TryMatchWithSufficientDestinationAllocatesZeroBytes()
    {
        var state = CreateValueMatchState();

        var succeeded = state.Index.TryMatch(state.Path, state.Values, out var written);
        ShouldBeTrue(succeeded, "Expected TryMatch to succeed when the destination is large enough.");
        ShouldEqual(written, 4);

        var measured = GetAllocatedBytes(
            static state => state.Index.TryMatch(state.Path, state.Values, out var written) ? written : -1,
            state);

        ShouldEqual(measured.Result, 4);
        ShouldEqual(measured.Allocated, 0L);
    }

    [Test]
    public void GetMatchCountUpperBoundAllocatesZeroBytes()
    {
        var state = CreateValueMatchState();

        var upperBound = state.Index.GetMatchCountUpperBound(state.Path);
        ShouldEqual(upperBound, 4);

        var measured = GetAllocatedBytes(static state => state.Index.GetMatchCountUpperBound(state.Path), state);

        ShouldEqual(measured.Result, 4);
        ShouldEqual(measured.Allocated, 0L);
    }

    [Test]
    public void MatchDetailedToCallerProvidedBuffersAllocatesZeroBytes()
    {
        var state = CreateDetailedMatchState();

        var matchCount = state.Index.MatchDetailed(state.Path, state.Matches, state.Captures, out var captureCount);
        ShouldEqual(matchCount, 3);
        ShouldEqual(captureCount, 2);

        var measured = GetAllocatedBytes(
            static state =>
            {
                var matchCount = state.Index.MatchDetailed(state.Path, state.Matches, state.Captures, out var captureCount);
                return new DetailedResult(matchCount, captureCount);
            },
            state);

        ShouldEqual(measured.Result, new DetailedResult(3, 2));
        ShouldEqual(measured.Allocated, 0L);
    }

    private static ValueMatchState CreateValueMatchState()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .Add(["market", "NASDAQ", "MSFT"], 1)
            .Add(["market", "NASDAQ", "*"], 2)
            .Add(["market", "*", "MSFT"], 3)
            .Add(["market", "*", "*"], 4)
            .Build(MatchOptions.PreserveDuplicates);

        var path = new[] { "market", "NASDAQ", "MSFT" };
        var values = new int[index.GetMatchCountUpperBound(path)];

        return new ValueMatchState(index, path, values);
    }

    private static DetailedMatchState CreateDetailedMatchState()
    {
        var builder = PattrnIndex<string, int>.Builder("*");
        builder.Add(["orders", "new"], 1);
        builder.AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], 2);
        builder.AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.CatchAll("remainder")], 3);

        var index = builder.Build(MatchOptions.PreserveDuplicates);
        var path = new[] { "orders", "new" };
        var matches = new PatternMatch<int>[index.GetMatchCountUpperBound(path)];
        var captures = new PatternCapture<string>[index.GetCaptureCountUpperBound(path)];

        return new DetailedMatchState(index, path, matches, captures);
    }

    private static AllocationMeasurement<TResult> GetAllocatedBytes<TState, TResult>(Func<TState, TResult> action, TState state)
    {
        action(state);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var before = GC.GetAllocatedBytesForCurrentThread();
        var result = action(state);
        var after = GC.GetAllocatedBytesForCurrentThread();

        return new AllocationMeasurement<TResult>(result, after - before);
    }

    private sealed record ValueMatchState(PattrnIndex<string, int> Index, string[] Path, int[] Values);

    private sealed record DetailedMatchState(
        PattrnIndex<string, int> Index,
        string[] Path,
        PatternMatch<int>[] Matches,
        PatternCapture<string>[] Captures);

    private readonly record struct DetailedResult(int MatchCount, int CaptureCount);

    private readonly record struct AllocationMeasurement<TResult>(TResult Result, long Allocated);
}
