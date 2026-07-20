namespace Pattrn.Routing.Tests;

public sealed class RoutingPerformanceSmokeTests
{
    [Test]
    public async Task PreSplitRouteMatchingToCallerProvidedBuffer_DoesNotAllocate()
    {
        var state = CreateValueMatchState();

        var succeeded = state.Index.TryMatchValues(state.PathSegments, state.Values, out var written);
        await Assert.That(succeeded).IsTrue().Because("Expected TryMatch to succeed when the destination is large enough.");
        await Assert.That(written).IsEqualTo(1);
        await Assert.That(state.Values[0]).IsEqualTo(1);

        var measured = GetAllocatedBytes(static state => state.Index.TryMatchValues(state.PathSegments, state.Values, out var written) ? written : -1, state);

        await Assert.That(measured.Result).IsEqualTo(1);
        await Assert.That(state.Values[0]).IsEqualTo(1);
        await Assert.That(measured.Allocated).IsEqualTo(0L);
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
