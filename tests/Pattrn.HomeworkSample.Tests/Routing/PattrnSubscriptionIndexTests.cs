using System.Collections.Concurrent;

namespace Homework.Routing;

public sealed class PattrnSubscriptionIndexTests
{
    [Test]
    public async Task DuplicateAdditionAndMissingRemovalDoNotPublishANewSnapshot()
    {
        var index = new PattrnSubscriptionIndex();
        var existing = Subscription.Of<RoutableMessages.PriceUpdated>(
            new ClientId("existing"),
            new ContentPattern("NASDAQ"));
        var missing = Subscription.Of<RoutableMessages.PriceUpdated>(
            new ClientId("missing"),
            new ContentPattern("NYSE"));
        index.AddSubscriptions([existing]);
        var original = index.CaptureSnapshot();

        index.AddSubscriptions([existing]);
        var afterDuplicate = index.CaptureSnapshot();
        index.RemoveSubscriptions([missing]);
        var afterMissingRemoval = index.CaptureSnapshot();

        await Assert.That(ReferenceEquals(original, afterDuplicate)).IsTrue();
        await Assert.That(ReferenceEquals(original, afterMissingRemoval)).IsTrue();
    }

    [Test]
    public async Task OldSnapshotRemainsValidAfterNewSnapshotIsPublished()
    {
        var index = new PattrnSubscriptionIndex();
        var messageTypeId = MessageTypeId.Of<RoutableMessages.PriceUpdated>();
        var first = new Subscription(new ClientId("A"), messageTypeId, new ContentPattern("NASDAQ"));
        var second = new Subscription(new ClientId("B"), messageTypeId, new ContentPattern("*"));
        var content = new MessageRoutingContent("NASDAQ", "MSFT");
        index.AddSubscriptions([first]);
        var oldSnapshot = index.CaptureSnapshot();

        index.AddSubscriptions([second]);
        var newSnapshot = index.CaptureSnapshot();

        await Assert.That(oldSnapshot.FindSubscriptions(messageTypeId, content)).IsEquivalentTo([first]);
        await Assert.That(newSnapshot.FindSubscriptions(messageTypeId, content)).IsEquivalentTo([first, second]);
    }

    [Test]
    public async Task FailedCompilationLeavesCanonicalStateAndSnapshotUnchanged()
    {
        var index = new PattrnSubscriptionIndex();
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
    public async Task CallerBufferReturnsTheSameCompleteSubscriptionSet()
    {
        var index = new PattrnSubscriptionIndex();
        var messageTypeId = MessageTypeId.Of<RoutableMessages.PriceUpdated>();
        var root = new Subscription(new ClientId("root"), messageTypeId, ContentPattern.Any);
        var literal = new Subscription(new ClientId("literal"), messageTypeId, new ContentPattern("NASDAQ"));
        var wildcard = new Subscription(new ClientId("wildcard"), messageTypeId, new ContentPattern("*", "MSFT"));
        var content = new MessageRoutingContent("NASDAQ", "MSFT");
        index.AddSubscriptions([root, literal, wildcard]);
        var destination = new Subscription[index.GetMatchCountUpperBound(messageTypeId, content)];

        var success = index.TryFindSubscriptions(messageTypeId, content, destination, out var written);

        await Assert.That(success).IsTrue();
        await Assert.That(destination[..written]).IsEquivalentTo([root, literal, wildcard]);
    }

    [Test]
    public async Task ConcurrentReadersObserveOnlyCompleteOldOrNewSnapshots()
    {
        var index = new PattrnSubscriptionIndex();
        var messageTypeId = MessageTypeId.Of<RoutableMessages.PriceUpdated>();
        var unaffectedTypeId = MessageTypeId.Of<RoutableMessages.InstrumentAdded>();
        var first = new Subscription(new ClientId("A"), messageTypeId, new ContentPattern("NASDAQ"));
        var second = new Subscription(new ClientId("B"), messageTypeId, new ContentPattern("*"));
        var unaffected = new Subscription(new ClientId("sentinel"), unaffectedTypeId, new ContentPattern("9"));
        var content = new MessageRoutingContent("NASDAQ", "MSFT");
        index.AddSubscriptions([first, unaffected]);
        using var start = new ManualResetEventSlim();
        var failures = new ConcurrentQueue<Exception>();

        var writer = Task.Run(() =>
        {
            start.Wait();
            for (var iteration = 0; iteration < 500; iteration++)
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
                    for (var iteration = 0; iteration < 2_000; iteration++)
                    {
                        var consumers = index.FindSubscriptions(messageTypeId, content)
                            .Select(subscription => subscription.ConsumerId.ToString())
                            .OrderBy(value => value, StringComparer.Ordinal)
                            .ToArray();
                        if (consumers is not ["A"] and not ["A", "B"])
                        {
                            throw new InvalidOperationException(
                                $"Observed partial or corrupt state: [{string.Join(", ", consumers)}].");
                        }

                        var sentinel = index.FindSubscriptions(
                            unaffectedTypeId,
                            new MessageRoutingContent("9")).Single();
                        if (!sentinel.Equals(unaffected))
                        {
                            throw new InvalidOperationException("Unaffected message-type state changed.");
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
}
