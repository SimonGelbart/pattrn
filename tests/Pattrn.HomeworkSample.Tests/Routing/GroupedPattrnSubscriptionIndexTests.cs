using System.Collections.Concurrent;

namespace Homework.Routing;

[InheritsTests]
public sealed class GroupedPattrnSubscriptionIndexContractTests : SubscriptionIndexContractTests
{
    public GroupedPattrnSubscriptionIndexContractTests()
        : base(() => new GroupedPattrnSubscriptionIndex())
    {
    }
}

[InheritsTests]
public sealed class LeanGroupedPattrnSubscriptionIndexContractTests : SubscriptionIndexContractTests
{
    public LeanGroupedPattrnSubscriptionIndexContractTests()
        : base(() => new GroupedPattrnSubscriptionIndex(
            GroupedPattrnSubscriptionIndex.GroupedMatcherMode.Lean))
    {
    }
}

public sealed class GroupedPattrnSubscriptionIndexTests
{
    [Test]
    public async Task ThirtyConsumersShareOneCompiledStructuralPattern()
    {
        var index = new GroupedPattrnSubscriptionIndex();
        var messageTypeId = MessageTypeId.Of<RoutableMessages.PriceUpdated>();
        var subscriptions = Enumerable.Range(0, 30)
            .Select(value => new Subscription(
                new ClientId($"consumer-{value}"),
                messageTypeId,
                new ContentPattern("NASDAQ")))
            .ToArray();

        index.AddSubscriptions(subscriptions);

        await Assert.That(index.StructuralPatternCount).IsEqualTo(1);
        await Assert.That(index.LogicalSubscriptionCount).IsEqualTo(30);
        await Assert.That(index.FindSubscriptions(
            messageTypeId,
            new MessageRoutingContent("NASDAQ", "MSFT")).ToArray()).IsEquivalentTo(subscriptions);
    }

    [Test]
    public async Task OneConsumerCanOwnMultiplePatternBuckets()
    {
        var index = new GroupedPattrnSubscriptionIndex();
        var consumer = new ClientId("same-consumer");
        var messageTypeId = MessageTypeId.Of<RoutableMessages.PriceUpdated>();
        var subscriptions = new[]
        {
            new Subscription(consumer, messageTypeId, ContentPattern.Any),
            new Subscription(consumer, messageTypeId, new ContentPattern("NASDAQ")),
            new Subscription(consumer, messageTypeId, new ContentPattern("*")),
        };

        index.AddSubscriptions(subscriptions);

        await Assert.That(index.FindSubscriptions(
            messageTypeId,
            new MessageRoutingContent("NASDAQ", "MSFT")).ToArray()).IsEquivalentTo(subscriptions);
        await Assert.That(index.StructuralPatternCount).IsEqualTo(3);
    }

    [Test]
    public async Task RemovingOneSubscriptionLeavesTheOtherValuesInItsBucket()
    {
        var index = new GroupedPattrnSubscriptionIndex();
        var messageTypeId = MessageTypeId.Of<RoutableMessages.PriceUpdated>();
        var first = new Subscription(new ClientId("first"), messageTypeId, new ContentPattern("NASDAQ"));
        var second = new Subscription(new ClientId("second"), messageTypeId, new ContentPattern("NASDAQ"));
        index.AddSubscriptions([first, second]);

        index.RemoveSubscriptions([first]);

        await Assert.That(index.StructuralPatternCount).IsEqualTo(1);
        await Assert.That(index.LogicalSubscriptionCount).IsEqualTo(1);
        await Assert.That(index.FindSubscriptions(
            messageTypeId,
            new MessageRoutingContent("NASDAQ", "MSFT"))).IsEquivalentTo([second]);
    }

    [Test]
    public async Task RemovingTheLastValueRemovesTheEmptyBucketAndType()
    {
        var index = new GroupedPattrnSubscriptionIndex();
        var subscription = Subscription.Of<RoutableMessages.PriceUpdated>(
            new ClientId("only"),
            new ContentPattern("NASDAQ"));
        index.AddSubscriptions([subscription]);

        index.RemoveSubscriptions([subscription]);

        await Assert.That(index.StructuralPatternCount).IsEqualTo(0);
        await Assert.That(index.LogicalSubscriptionCount).IsEqualTo(0);
        await Assert.That(index.CaptureSnapshot().Indexes.ContainsKey(subscription.MessageTypeId)).IsFalse();
    }

    [Test]
    public async Task DuplicateSubscriptionsDoNotDuplicateBucketValues()
    {
        var index = new GroupedPattrnSubscriptionIndex();
        var subscription = Subscription.Of<RoutableMessages.PriceUpdated>(
            new ClientId("duplicate"),
            new ContentPattern("NASDAQ"));

        index.AddSubscriptions([subscription, subscription]);

        await Assert.That(index.LogicalSubscriptionCount).IsEqualTo(1);
        await Assert.That(index.FindSubscriptions(
            subscription.MessageTypeId,
            new MessageRoutingContent("NASDAQ", "MSFT")).ToArray()).IsEquivalentTo([subscription]);
    }

    [Test]
    public async Task OverlappingRootLiteralAndWildcardBucketsFlattenTogether()
    {
        var index = new GroupedPattrnSubscriptionIndex(
            GroupedPattrnSubscriptionIndex.GroupedMatcherMode.Lean);
        var messageTypeId = MessageTypeId.Of<RoutableMessages.PriceUpdated>();
        var subscriptions = new[]
        {
            new Subscription(new ClientId("root"), messageTypeId, ContentPattern.Any),
            new Subscription(new ClientId("literal"), messageTypeId, new ContentPattern("NASDAQ")),
            new Subscription(new ClientId("wildcard"), messageTypeId, new ContentPattern("*")),
            new Subscription(new ClientId("literal-exact"), messageTypeId, new ContentPattern("NASDAQ", "MSFT")),
            new Subscription(new ClientId("literal-wildcard"), messageTypeId, new ContentPattern("NASDAQ", "*")),
            new Subscription(new ClientId("wildcard-literal"), messageTypeId, new ContentPattern("*", "MSFT")),
        };
        index.AddSubscriptions(subscriptions);
        var destination = new Subscription[index.GetMatchCountUpperBound(
            messageTypeId,
            new MessageRoutingContent("NASDAQ", "MSFT"))];

        var success = index.TryFindSubscriptions(
            messageTypeId,
            new MessageRoutingContent("NASDAQ", "MSFT"),
            destination,
            out var written);

        await Assert.That(success).IsTrue();
        await Assert.That(destination[..written]).IsEquivalentTo(subscriptions);
    }

    [Test]
    public async Task FailedGroupedCompilationLeavesStateAndSnapshotUnchanged()
    {
        var index = new GroupedPattrnSubscriptionIndex();
        var existing = Subscription.Of<RoutableMessages.PriceUpdated>(
            new ClientId("existing"),
            new ContentPattern("NASDAQ"));
        var invalid = Subscription.Of<RoutableMessages.PriceUpdated>(
            new ClientId("invalid"),
            new ContentPattern([null!]));
        index.AddSubscriptions([existing]);
        var snapshot = index.CaptureSnapshot();

        var threw = false;
        try
        {
            index.AddSubscriptions([invalid]);
        }
        catch (ArgumentNullException)
        {
            threw = true;
        }

        await Assert.That(threw).IsTrue();
        await Assert.That(ReferenceEquals(snapshot, index.CaptureSnapshot())).IsTrue();
        await Assert.That(index.FindSubscriptions(
            existing.MessageTypeId,
            new MessageRoutingContent("NASDAQ", "MSFT"))).IsEquivalentTo([existing]);
    }

    [Test]
    public async Task ConcurrentReadersObserveCompleteOldOrNewGroupedSnapshots()
    {
        var index = new GroupedPattrnSubscriptionIndex(
            GroupedPattrnSubscriptionIndex.GroupedMatcherMode.Lean);
        var messageTypeId = MessageTypeId.Of<RoutableMessages.PriceUpdated>();
        var first = Subscription.Of<RoutableMessages.PriceUpdated>(
            new ClientId("A"),
            new ContentPattern("NASDAQ"));
        var second = Subscription.Of<RoutableMessages.PriceUpdated>(
            new ClientId("B"),
            new ContentPattern("*"));
        var content = new MessageRoutingContent("NASDAQ", "MSFT");
        index.AddSubscriptions([first]);
        using var start = new ManualResetEventSlim();
        var failures = new ConcurrentQueue<Exception>();

        var writer = Task.Run(() =>
        {
            start.Wait();
            for (var iteration = 0; iteration < 200; iteration++)
            {
                index.AddSubscriptions([second]);
                index.RemoveSubscriptions([second]);
            }
        });
        var readers = Enumerable.Range(0, Math.Max(2, Environment.ProcessorCount / 2))
            .Select(_ => Task.Run(() =>
            {
                start.Wait();
                try
                {
                    for (var iteration = 0; iteration < 500; iteration++)
                    {
                        var values = index.FindSubscriptions(messageTypeId, content).ToArray();
                        if (values is not [var only] || !only.Equals(first))
                        {
                            if (values.Length != 2 || !values.Contains(first) || !values.Contains(second))
                            {
                                throw new InvalidOperationException("Observed a partial grouped snapshot.");
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    failures.Enqueue(exception);
                }
            }))
            .ToArray();

        start.Set();
        await Task.WhenAll(readers.Append(writer));

        await Assert.That(failures).IsEmpty();
    }

    [Test]
    public async Task MixedLiteralWildcardPatternMatchesItsThreeSegmentContent()
    {
        var index = new GroupedPattrnSubscriptionIndex();
        var messageTypeId = MessageTypeId.Of<RoutableMessages.PriceUpdated>();
        var matching = new Subscription(
            new ClientId("matching"),
            messageTypeId,
            new ContentPattern("orders", "eu", "123"));
        var nonMatching = new Subscription(
            new ClientId("non-matching"),
            messageTypeId,
            new ContentPattern("miss", "*", "1"));

        index.AddSubscriptions([matching, nonMatching]);

        await Assert.That(index.FindSubscriptions(
            messageTypeId,
            new MessageRoutingContent("orders", "eu", "123"))).IsEquivalentTo([matching]);
    }
}
