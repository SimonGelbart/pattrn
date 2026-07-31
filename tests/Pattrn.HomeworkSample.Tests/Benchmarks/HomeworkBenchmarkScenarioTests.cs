using Homework.Benchmarks.Scenarios;
using Homework.Routing;

namespace Homework.Benchmarks;

public sealed class HomeworkBenchmarkScenarioTests
{
    [Test]
    public async Task CatalogContainsTheRequestedCompatibleCartesianMatrix()
    {
        var lookup = HomeworkBenchmarkScenarioCatalog.LookupScenarios().ToArray();
        var build = HomeworkBenchmarkScenarioCatalog.BuildScenarios().ToArray();
        var mutation = HomeworkBenchmarkScenarioCatalog.MutationScenarios().ToArray();

        await Assert.That(lookup.Length).IsEqualTo(76);
        await Assert.That(build.Length).IsEqualTo(40);
        await Assert.That(mutation.Length).IsEqualTo(20);
        await Assert.That(lookup.All(scenario =>
            HomeworkBenchmarkScenarioCatalog.RegistrationCounts.Contains(scenario.RegistrationCount)
            && HomeworkBenchmarkScenarioCatalog.CompatibleMatchCounts(scenario.Distribution)
                .Contains(scenario.MatchCount))).IsTrue();
    }

    [Test]
    public async Task EveryLookupShapeProducesTheRequestedResultForBothIndexes()
    {
        var scenarios = HomeworkBenchmarkScenarioCatalog.LookupScenarios()
            .Where(scenario => scenario.RegistrationCount == 1_000)
            .ToArray();

        foreach (var scenario in scenarios)
        {
            var data = HomeworkBenchmarkDataFactory.CreateLookup(scenario);
            foreach (var implementation in Enum.GetValues<SubscriptionIndexImplementation>())
            {
                var index = HomeworkBenchmarkDataFactory.CreateIndex(implementation, data.Registrations);
                var matches = index.FindSubscriptions(data.MessageTypeId, data.RoutingContent).ToArray();

                await Assert.That(matches.Length)
                    .IsEqualTo(scenario.MatchCount)
                    .Because($"{implementation} must preserve the generated shape {scenario}");
                await Assert.That(matches.Distinct().Count()).IsEqualTo(matches.Length);
            }
        }
    }
}
