using System.Diagnostics;
using Homework.Routing;
using Pattrn.Matching;

namespace Pattrn.LeanCoreBenchmarks;

internal static class LeanCoreProbe
{
    internal static Task<int> RunAsync(string[] args)
    {
        if (args.Length == 3
            && string.Equals(args[0], "lookup", StringComparison.OrdinalIgnoreCase)
            && Enum.TryParse<LeanDistribution>(args[1], ignoreCase: true, out var distribution)
            && int.TryParse(args[2], out var expected))
        {
            var lookupScenario = new LeanLookupCase(40_000, distribution, expected);
            var lookupData = LeanBenchmarkData.CreateLookup(lookupScenario);
            foreach (var lookupImplementation in Enum.GetValues<LeanImplementation>())
            {
                var lookupIndex = LeanBenchmarkData.CreateIndex(lookupImplementation, lookupData.Registrations);
                Console.WriteLine($"implementation={lookupImplementation} expected={expected} actual={lookupIndex.FindSubscriptions(lookupData.MessageTypeId, lookupData.Content).Count()}");
            }

            return Task.FromResult(0);
        }

        if (args.Length != 2
            || !string.Equals(args[0], "original", StringComparison.OrdinalIgnoreCase)
            || !Enum.TryParse<LeanImplementation>(args[1], ignoreCase: true, out var implementation))
        {
            Console.Error.WriteLine("Usage: --probe original <CustomTree|Pattrn|GroupedGeneral|GroupedLean> | lookup <MostlyLiteral|MixedLiteralWildcard> <0|1|30|256>");
            return Task.FromResult(2);
        }

        var data = LeanBenchmarkData.CreateOriginalQuery();
        ForceCollection();
        var before = GC.GetTotalMemory(forceFullCollection: true);
        var stopwatch = Stopwatch.StartNew();
        var index = LeanBenchmarkData.CreateIndex(implementation, data.Registrations);
        stopwatch.Stop();
        ForceCollection();
        var after = GC.GetTotalMemory(forceFullCollection: true);
        var matches = index.FindSubscriptions(data.MessageTypeId, data.Content).Count();
        var structuralPatterns = index switch
        {
            PattrnSubscriptionIndex pattrn => pattrn.CaptureSnapshot().Indexes.Values.Sum(value => value.PatternCount),
            GroupedPattrnSubscriptionIndex grouped => grouped.StructuralPatternCount,
            _ => 40_000,
        };

        Console.WriteLine(
            $"implementation={implementation} logicalSubscriptions={data.Registrations.Length} "
            + $"structuralPatterns={structuralPatterns} matches={matches} "
            + $"buildMilliseconds={stopwatch.Elapsed.TotalMilliseconds:F3} "
            + $"retainedBytes={after - before}");
        GC.KeepAlive(index);
        GC.KeepAlive(data.Registrations);
        return Task.FromResult(0);
    }

    private static void ForceCollection()
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
    }
}
