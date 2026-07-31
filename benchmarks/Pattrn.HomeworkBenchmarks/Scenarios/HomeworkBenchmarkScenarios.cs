using Homework.Routing;

namespace Homework.Benchmarks.Scenarios;

public enum SubscriptionIndexImplementation
{
    CustomTree,
    Pattrn,
}

public enum PatternDistribution
{
    MostlyLiterals,
    MixedLiteralsAndWildcards,
    SharedPrefixes,
    LowSelectivity,
    HighSelectivity,
}

public sealed record HomeworkLookupScenario(
    int RegistrationCount,
    PatternDistribution Distribution,
    int MatchCount)
{
    public override string ToString() => $"{RegistrationCount}-{Distribution}-{MatchCount}";
}

public sealed record HomeworkBuildScenario(
    int RegistrationCount,
    PatternDistribution Distribution,
    int MessageTypeCount)
{
    public override string ToString() => $"{RegistrationCount}-{Distribution}-{MessageTypeCount}Types";
}

public sealed record HomeworkMutationScenario(
    int RegistrationCount,
    PatternDistribution Distribution)
{
    public override string ToString() => $"{RegistrationCount}-{Distribution}";
}

public static class HomeworkBenchmarkScenarioCatalog
{
    public static IReadOnlyList<int> RegistrationCounts { get; } = [1_000, 10_000, 100_000, 1_200_000];

    public static IReadOnlyList<int> MatchCounts { get; } = [0, 1, 5, 30, 256];

    public static IReadOnlyList<PatternDistribution> Distributions { get; } =
        Enum.GetValues<PatternDistribution>();

    public static IEnumerable<HomeworkLookupScenario> LookupScenarios()
    {
        foreach (var registrationCount in RegistrationCounts)
        {
            foreach (var distribution in Distributions)
            {
                foreach (var matchCount in CompatibleMatchCounts(distribution))
                {
                    yield return new HomeworkLookupScenario(registrationCount, distribution, matchCount);
                }
            }
        }
    }

    public static IEnumerable<HomeworkBuildScenario> BuildScenarios()
    {
        foreach (var registrationCount in RegistrationCounts)
        {
            foreach (var distribution in Distributions)
            {
                yield return new HomeworkBuildScenario(registrationCount, distribution, MessageTypeCount: 1);
                yield return new HomeworkBuildScenario(registrationCount, distribution, MessageTypeCount: 10);
            }
        }
    }

    public static IEnumerable<HomeworkMutationScenario> MutationScenarios()
    {
        foreach (var registrationCount in RegistrationCounts)
        {
            foreach (var distribution in Distributions)
            {
                yield return new HomeworkMutationScenario(registrationCount, distribution);
            }
        }
    }

    public static IReadOnlyList<int> CompatibleMatchCounts(PatternDistribution distribution) =>
        distribution switch
        {
            PatternDistribution.LowSelectivity => [30, 256],
            PatternDistribution.HighSelectivity => [0, 1],
            _ => MatchCounts,
        };
}

public sealed class MatrixMessage : IRoutableMessage
{
    public MessageRoutingContent GetContent() => HomeworkBenchmarkDataFactory.TargetContent;
}
