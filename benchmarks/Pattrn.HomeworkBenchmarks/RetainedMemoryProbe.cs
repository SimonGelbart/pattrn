using Homework.Benchmarks.Scenarios;
using Homework.Routing;

namespace Homework.Benchmarks;

internal static class RetainedMemoryProbe
{
    internal static int Run(string[] args)
    {
        if (args.Length != 4
            || !Enum.TryParse<SubscriptionIndexImplementation>(args[1], ignoreCase: true, out var implementation)
            || !Enum.TryParse<PatternDistribution>(args[2], ignoreCase: true, out var distribution)
            || !int.TryParse(args[3], out var registrationCount))
        {
            Console.Error.WriteLine(
                "Usage: --retained-memory <CustomTree|Pattrn> <distribution> <registration-count>");
            return 2;
        }

        var compatibleMatches = HomeworkBenchmarkScenarioCatalog.CompatibleMatchCounts(distribution);
        var matchCount = compatibleMatches.Contains(30) ? 30 : compatibleMatches[^1];
        var data = HomeworkBenchmarkDataFactory.CreateLookup(
            new HomeworkLookupScenario(registrationCount, distribution, matchCount));
        ForceCollection();
        var before = GC.GetTotalMemory(forceFullCollection: true);
        var index = HomeworkBenchmarkDataFactory.CreateIndex(implementation, data.Registrations);
        ForceCollection();
        var after = GC.GetTotalMemory(forceFullCollection: true);
        var retained = after - before;
        var matches = index.FindSubscriptions(data.MessageTypeId, data.RoutingContent).Count();

        Console.WriteLine(
            $"implementation={implementation} distribution={distribution} registrations={registrationCount} "
            + $"matches={matches} retainedBytes={retained}");
        GC.KeepAlive(index);
        GC.KeepAlive(data);
        return 0;
    }

    private static void ForceCollection()
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
    }
}
