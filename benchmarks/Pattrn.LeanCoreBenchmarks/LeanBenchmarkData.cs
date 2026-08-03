using Homework.Routing;

namespace Pattrn.LeanCoreBenchmarks;

public enum LeanImplementation
{
    CustomTree,
    Pattrn,
    GroupedGeneral,
    GroupedLean,
}

public enum LeanDistribution
{
    MostlyLiteral,
    MixedLiteralWildcard,
}

public sealed record LeanLookupCase(
    int RegistrationCount,
    LeanDistribution Distribution,
    int MatchCount)
{
    public override string ToString() => $"{RegistrationCount}-{Distribution}-{MatchCount}";
}

internal static class LeanBenchmarkData
{
    internal static IReadOnlyList<LeanLookupCase> LookupCases { get; } =
        [
            new(40_000, LeanDistribution.MostlyLiteral, 0),
            new(40_000, LeanDistribution.MostlyLiteral, 1),
            new(40_000, LeanDistribution.MostlyLiteral, 30),
            new(40_000, LeanDistribution.MostlyLiteral, 256),
            new(40_000, LeanDistribution.MixedLiteralWildcard, 0),
            new(40_000, LeanDistribution.MixedLiteralWildcard, 1),
            new(40_000, LeanDistribution.MixedLiteralWildcard, 30),
            new(40_000, LeanDistribution.MixedLiteralWildcard, 256),
        ];

    internal static Subscription[] CreateOriginalSubscriptions()
    {
        var subscriptions = new Subscription[1_200_000];
        var offset = 0;
        for (var consumerIndex = 0; consumerIndex < 30; consumerIndex++)
        {
            var consumer = new ClientId($"Client.{consumerIndex}");
            for (var typeIndex = 0; typeIndex < 10; typeIndex++)
            {
                var messageTypeId = OriginalMessageTypeId(typeIndex);
                for (var contentIndex = 0; contentIndex < 4_000; contentIndex++)
                {
                    subscriptions[offset++] = new Subscription(
                        consumer,
                        messageTypeId,
                        new ContentPattern(contentIndex.ToString()));
                }
            }
        }

        return subscriptions;
    }

    internal static (Subscription[] Registrations, MessageTypeId MessageTypeId, MessageRoutingContent Content) CreateOriginalQuery()
        => (CreateOriginalSubscriptions(), OriginalMessageTypeId(0), new MessageRoutingContent("999", "1234"));

    internal static MessageTypeId OriginalMessageTypeId(int typeIndex) => typeIndex == 0
        ? new MessageTypeId(typeof(OriginalMessage0))
        : new MessageTypeId($"Original.Message{typeIndex}");

    internal static (Subscription[] Registrations, MessageTypeId MessageTypeId, MessageRoutingContent Content) CreateLookup(
        LeanLookupCase scenario)
    {
        var messageTypeId = new MessageTypeId(typeof(FocusedBenchmarkMessage));
        var content = new MessageRoutingContent("orders", "eu", "123");
        var registrations = new Subscription[scenario.RegistrationCount];
        for (var index = 0; index < scenario.MatchCount; index++)
        {
            registrations[index] = new Subscription(
                new ClientId($"match-{index}"),
                messageTypeId,
                MatchingPattern(scenario.Distribution, index));
        }

        for (var index = scenario.MatchCount; index < registrations.Length; index++)
        {
            registrations[index] = new Subscription(
                new ClientId($"miss-{index}"),
                messageTypeId,
                NonMatchingPattern(scenario.Distribution, index));
        }

        return (registrations, messageTypeId, content);
    }

    internal static ISubscriptionIndex CreateIndex(
        LeanImplementation implementation,
        IEnumerable<Subscription> registrations)
    {
        ISubscriptionIndex index = implementation switch
        {
            LeanImplementation.CustomTree => new SubscriptionIndex(),
            LeanImplementation.Pattrn => new PattrnSubscriptionIndex(),
            LeanImplementation.GroupedGeneral => new GroupedPattrnSubscriptionIndex(),
            LeanImplementation.GroupedLean => new GroupedPattrnSubscriptionIndex(
                GroupedPattrnSubscriptionIndex.GroupedMatcherMode.Lean),
            _ => throw new ArgumentOutOfRangeException(nameof(implementation)),
        };
        index.AddSubscriptions(registrations);
        return index;
    }

    private static ContentPattern MatchingPattern(LeanDistribution distribution, int index) =>
        distribution switch
        {
            LeanDistribution.MostlyLiteral => new ContentPattern("orders", "eu", "123"),
            LeanDistribution.MixedLiteralWildcard => (index % 4) switch
            {
                0 => new ContentPattern("orders", "eu", "123"),
                1 => new ContentPattern("orders", "*", "123"),
                2 => new ContentPattern("*", "eu", "*"),
                _ => new ContentPattern("*", "*", "123"),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(distribution)),
        };

    private static ContentPattern NonMatchingPattern(LeanDistribution distribution, int index)
    {
        var value = index.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return distribution switch
        {
            LeanDistribution.MostlyLiteral => new ContentPattern($"miss-{value}", "eu", value),
            LeanDistribution.MixedLiteralWildcard => (index % 3) switch
            {
                0 => new ContentPattern($"miss-{value}", "eu", value),
                1 => new ContentPattern($"miss-{value}", "*", value),
                _ => new ContentPattern("*", $"miss-{value}", "*"),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(distribution)),
        };
    }
}

internal sealed class FocusedBenchmarkMessage(MessageRoutingContent content) : IRoutableMessage
{
    public MessageRoutingContent GetContent() => content;
}

internal sealed class OriginalMessage0(MessageRoutingContent content) : IRoutableMessage
{
    public MessageRoutingContent GetContent() => content;
}
