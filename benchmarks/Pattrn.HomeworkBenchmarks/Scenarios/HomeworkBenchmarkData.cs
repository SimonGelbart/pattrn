using Homework.Routing;

namespace Homework.Benchmarks.Scenarios;

public sealed record HomeworkBenchmarkData(
    HomeworkLookupScenario Scenario,
    Subscription[] Registrations,
    MessageTypeId MessageTypeId,
    MessageRoutingContent RoutingContent,
    MatrixMessage Message);

public sealed record HomeworkMutationData(
    Subscription[] Registrations,
    Subscription[] Additions,
    Subscription[] Removals,
    ClientId HeavyConsumer);

public static class HomeworkBenchmarkDataFactory
{
    public static MessageRoutingContent TargetContent { get; } = new("orders", "eu", "123");

    public static HomeworkBenchmarkData CreateLookup(HomeworkLookupScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        if (scenario.MatchCount > scenario.RegistrationCount)
        {
            throw new ArgumentOutOfRangeException(nameof(scenario), "Match count cannot exceed registration count.");
        }

        var messageTypeId = MessageTypeId.Of<MatrixMessage>();
        var registrations = new Subscription[scenario.RegistrationCount];
        for (var index = 0; index < scenario.MatchCount; index++)
        {
            registrations[index] = new Subscription(
                new ClientId($"match-{index}"),
                messageTypeId,
                CreateMatchingPattern(scenario.Distribution, index));
        }

        for (var index = scenario.MatchCount; index < registrations.Length; index++)
        {
            registrations[index] = new Subscription(
                new ClientId($"miss-{index}"),
                messageTypeId,
                CreateNonMatchingPattern(scenario.Distribution, index));
        }

        var data = new HomeworkBenchmarkData(
            scenario,
            registrations,
            messageTypeId,
            TargetContent,
            new MatrixMessage());
        ValidateShape(data);
        return data;
    }

    public static Subscription[] CreateBuild(HomeworkBuildScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        var messageTypes = Enumerable.Range(0, scenario.MessageTypeCount)
            .Select(index => new MessageTypeId($"Homework.Benchmarks.BuildMessage{index}"))
            .ToArray();
        var registrations = new Subscription[scenario.RegistrationCount];
        for (var index = 0; index < registrations.Length; index++)
        {
            registrations[index] = new Subscription(
                new ClientId($"build-{index}"),
                messageTypes[index % messageTypes.Length],
                CreateBuildPattern(scenario.Distribution, index));
        }

        return registrations;
    }

    public static HomeworkMutationData CreateMutation(HomeworkMutationScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        var messageTypeId = new MessageTypeId("Homework.Benchmarks.MutationMessage");
        var heavyConsumer = new ClientId("heavy-consumer");
        var heavyCount = Math.Min(2_000, Math.Max(1, scenario.RegistrationCount / 10));
        var registrations = new Subscription[scenario.RegistrationCount];

        for (var index = 0; index < registrations.Length; index++)
        {
            var consumer = index < heavyCount ? heavyConsumer : new ClientId($"base-{index}");
            var pattern = index < heavyCount
                ? new ContentPattern("heavy", index.ToString(System.Globalization.CultureInfo.InvariantCulture))
                : CreateBuildPattern(scenario.Distribution, index);
            registrations[index] = new Subscription(consumer, messageTypeId, pattern);
        }

        var additions = new Subscription[10_000];
        for (var index = 0; index < additions.Length; index++)
        {
            additions[index] = new Subscription(
                new ClientId($"addition-{index}"),
                messageTypeId,
                new ContentPattern("addition", index.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        }

        return new HomeworkMutationData(
            registrations,
            additions,
            registrations[..Math.Min(100, registrations.Length)],
            heavyConsumer);
    }

    public static ISubscriptionIndex CreateIndex(
        SubscriptionIndexImplementation implementation,
        IEnumerable<Subscription> registrations)
    {
        ISubscriptionIndex index = implementation switch
        {
            SubscriptionIndexImplementation.CustomTree => new SubscriptionIndex(),
            SubscriptionIndexImplementation.Pattrn => new PattrnSubscriptionIndex(),
            _ => throw new ArgumentOutOfRangeException(nameof(implementation)),
        };
        index.AddSubscriptions(registrations);
        return index;
    }

    public static void ValidateShape(HomeworkBenchmarkData data)
    {
        if (data.Registrations.Length != data.Scenario.RegistrationCount)
        {
            throw new InvalidOperationException(
                $"Generated {data.Registrations.Length} registrations; expected {data.Scenario.RegistrationCount}.");
        }

        var expectedMatches = data.Registrations.Count(
            subscription => Matches(subscription.ContentPattern.Parts, data.RoutingContent.Parts!));
        if (expectedMatches != data.Scenario.MatchCount)
        {
            throw new InvalidOperationException(
                $"Generated {expectedMatches} matches; expected {data.Scenario.MatchCount} for {data.Scenario}.");
        }
    }

    private static ContentPattern CreateMatchingPattern(PatternDistribution distribution, int index) =>
        distribution switch
        {
            PatternDistribution.MostlyLiterals => new ContentPattern("orders", "eu", "123"),
            PatternDistribution.MixedLiteralsAndWildcards => (index % 4) switch
            {
                0 => new ContentPattern("orders", "eu", "123"),
                1 => new ContentPattern("orders", "*", "123"),
                2 => new ContentPattern("*", "eu", "*"),
                _ => new ContentPattern("*", "*", "123"),
            },
            PatternDistribution.SharedPrefixes => new ContentPattern("orders", "eu", "123"),
            PatternDistribution.LowSelectivity => (index % 4) switch
            {
                0 => new ContentPattern("*", "*", "*"),
                1 => new ContentPattern("orders", "*", "*"),
                2 => new ContentPattern("*", "eu", "*"),
                _ => new ContentPattern("*", "*", "123"),
            },
            PatternDistribution.HighSelectivity => new ContentPattern("orders", "eu", "123"),
            _ => throw new ArgumentOutOfRangeException(nameof(distribution)),
        };

    private static ContentPattern CreateNonMatchingPattern(PatternDistribution distribution, int index)
    {
        var value = index.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return distribution switch
        {
            PatternDistribution.MostlyLiterals => new ContentPattern($"miss-{value}", "eu", value),
            PatternDistribution.MixedLiteralsAndWildcards => (index % 4) switch
            {
                0 => new ContentPattern($"miss-{value}", "eu", value),
                1 => new ContentPattern($"miss-{value}", "*", value),
                2 => new ContentPattern("*", $"miss-{value}", "*"),
                _ => new ContentPattern("*", "*", $"miss-{value}"),
            },
            PatternDistribution.SharedPrefixes => new ContentPattern("orders", "eu", $"miss-{value}"),
            PatternDistribution.LowSelectivity => (index % 3) switch
            {
                0 => new ContentPattern("*", "*", $"miss-{value}"),
                1 => new ContentPattern("*", $"miss-{value}", "*"),
                _ => new ContentPattern($"miss-{value}", "*", "*"),
            },
            PatternDistribution.HighSelectivity => new ContentPattern(
                $"tenant-{value}",
                $"region-{value}",
                $"item-{value}"),
            _ => throw new ArgumentOutOfRangeException(nameof(distribution)),
        };
    }

    private static ContentPattern CreateBuildPattern(PatternDistribution distribution, int index)
    {
        var value = index.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return distribution switch
        {
            PatternDistribution.MostlyLiterals => new ContentPattern("orders", $"region-{value}", $"item-{value}"),
            PatternDistribution.MixedLiteralsAndWildcards => (index % 4) switch
            {
                0 => new ContentPattern("orders", $"region-{value}", $"item-{value}"),
                1 => new ContentPattern("orders", "*", $"item-{value}"),
                2 => new ContentPattern("*", $"region-{value}", $"item-{value}"),
                _ => new ContentPattern($"orders-{value}", "*", "*"),
            },
            PatternDistribution.SharedPrefixes => new ContentPattern("orders", "eu", $"item-{value}"),
            PatternDistribution.LowSelectivity => (index % 3) switch
            {
                0 => new ContentPattern("*", "*", $"item-{value}"),
                1 => new ContentPattern("*", $"region-{value}", "*"),
                _ => new ContentPattern($"orders-{value}", "*", "*"),
            },
            PatternDistribution.HighSelectivity => new ContentPattern(
                $"tenant-{value}",
                $"region-{value}",
                $"item-{value}"),
            _ => throw new ArgumentOutOfRangeException(nameof(distribution)),
        };
    }

    private static bool Matches(IReadOnlyList<string> pattern, IReadOnlyList<string> path)
    {
        if (pattern.Count > path.Count)
        {
            return false;
        }

        for (var index = 0; index < pattern.Count; index++)
        {
            if (pattern[index] != "*" && !StringComparer.Ordinal.Equals(pattern[index], path[index]))
            {
                return false;
            }
        }

        return true;
    }
}
