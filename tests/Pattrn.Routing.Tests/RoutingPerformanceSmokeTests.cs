using static Pattrn.Routing.Tests.TestAssertions;

namespace Pattrn.Routing.Tests;

public sealed class RoutingPerformanceSmokeTests
{
    [Test]
    public void PreSplitRouteMatchingToCallerProvidedBuffer_DoesNotAllocate()
    {
        var state = CreateValueMatchState();

        var written = state.Index.Match(state.PathSegments, state.Values);
        ShouldEqual(written, 1);
        ShouldEqual(state.Values[0], 1);

        var measured = GetAllocatedBytes(static state => state.Index.Match(state.PathSegments, state.Values), state);

        ShouldEqual(measured.Result, 1);
        ShouldEqual(state.Values[0], 1);
        ShouldEqual(measured.Allocated, 0L);
    }

    private static ValueMatchState CreateValueMatchState()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .AddRoute("/market/{exchange}/{symbol}", 1)
            .AddRoute("/market/{exchange}/orders/{id}", 2)
            .AddRoute("/files/{*path}", 3)
            .Build();

        var pathSegments = new[] { "market", "NASDAQ", "MSFT" };
        var values = new int[index.GetMatchCountUpperBound(pathSegments)];

        return new ValueMatchState(index, pathSegments, values);
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

    private sealed record ValueMatchState(PattrnIndex<string, int> Index, string[] PathSegments, int[] Values);

    private readonly record struct AllocationMeasurement<TResult>(TResult Result, long Allocated);
}
