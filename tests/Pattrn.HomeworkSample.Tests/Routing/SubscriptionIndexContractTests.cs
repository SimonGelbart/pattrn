namespace Homework.Routing;

public abstract class SubscriptionIndexContractTests
{
    private readonly Func<ISubscriptionIndex> _createIndex;

    protected SubscriptionIndexContractTests(Func<ISubscriptionIndex> createIndex)
    {
        _createIndex = createIndex;
    }

    [Test]
    public async Task ReturnsEveryApplicableRootLiteralAndWildcardSubscriptionExactlyOnce()
    {
        var index = _createIndex();
        var messageTypeId = MessageTypeId.Of<RoutableMessages.PriceUpdated>();
        var registrations = new[]
        {
            new Subscription(new ClientId("root"), messageTypeId, ContentPattern.Any),
            new Subscription(new ClientId("literal-prefix"), messageTypeId, new ContentPattern("NASDAQ")),
            new Subscription(new ClientId("wildcard-prefix"), messageTypeId, new ContentPattern("*")),
            new Subscription(new ClientId("literal-exact"), messageTypeId, new ContentPattern("NASDAQ", "MSFT")),
            new Subscription(new ClientId("literal-wildcard"), messageTypeId, new ContentPattern("NASDAQ", "*")),
            new Subscription(new ClientId("wildcard-literal"), messageTypeId, new ContentPattern("*", "MSFT")),
        };
        index.AddSubscriptions(registrations);

        var actual = index.FindSubscriptions(messageTypeId, new MessageRoutingContent("NASDAQ", "MSFT")).ToArray();

        await Assert.That(actual).IsEquivalentTo(registrations);
    }

    [Test]
    public async Task WildcardConsumesExactlyOneSegment()
    {
        var index = _createIndex();
        var messageTypeId = MessageTypeId.Of<RoutableMessages.PriceUpdated>();
        var subscription = new Subscription(
            new ClientId("wildcard"),
            messageTypeId,
            new ContentPattern("*", "MSFT"));
        index.AddSubscriptions([subscription]);

        var tooShort = index.FindSubscriptions(messageTypeId, new MessageRoutingContent("MSFT")).ToArray();
        var matching = index.FindSubscriptions(messageTypeId, new MessageRoutingContent("NASDAQ", "MSFT")).ToArray();

        await Assert.That(tooShort).IsEmpty();
        await Assert.That(matching).IsEquivalentTo([subscription]);
    }

    [Test]
    public async Task PreservesLiteralEmptySegmentsAndRejectsLongerPatterns()
    {
        var index = _createIndex();
        var messageTypeId = MessageTypeId.Of<RoutableMessages.TradingHalted>();
        var literalEmpty = new Subscription(
            new ClientId("literal-empty"),
            messageTypeId,
            new ContentPattern("NASDAQ", string.Empty, "AMZN"));
        var tooLong = new Subscription(
            new ClientId("too-long"),
            messageTypeId,
            new ContentPattern("NASDAQ", string.Empty, "AMZN", "tail"));
        index.AddSubscriptions([literalEmpty, tooLong]);

        var actual = index.FindSubscriptions(
            messageTypeId,
            new MessageRoutingContent("NASDAQ", string.Empty, "AMZN")).ToArray();

        await Assert.That(actual).IsEquivalentTo([literalEmpty]);
    }

    [Test]
    public async Task AddingTheSameSubscriptionTwiceReturnsOneLogicalResult()
    {
        var index = _createIndex();
        var subscription = Subscription.Of<RoutableMessages.PriceUpdated>(
            new ClientId("duplicate"),
            new ContentPattern("NASDAQ"));

        index.AddSubscriptions([subscription, subscription]);

        var actual = index.FindSubscriptions(
            subscription.MessageTypeId,
            new MessageRoutingContent("NASDAQ", "MSFT")).ToArray();
        await Assert.That(actual).IsEquivalentTo([subscription]);
    }

    [Test]
    public async Task DistinctMatchingSubscriptionsForOneConsumerRemainDistinct()
    {
        var index = _createIndex();
        var clientId = new ClientId("same-consumer");
        var literal = Subscription.Of<RoutableMessages.PriceUpdated>(clientId, new ContentPattern("NASDAQ"));
        var wildcard = Subscription.Of<RoutableMessages.PriceUpdated>(clientId, new ContentPattern("*"));
        index.AddSubscriptions([literal, wildcard]);

        var subscriptions = index.FindSubscriptions(
            literal.MessageTypeId,
            new MessageRoutingContent("NASDAQ", "MSFT")).ToArray();
        var consumers = new MessageRouter(index).GetConsumers(
            new RoutableMessages.PriceUpdated { ExchangeCode = "NASDAQ", Symbol = "MSFT" }).ToArray();

        await Assert.That(subscriptions).IsEquivalentTo([literal, wildcard]);
        await Assert.That(consumers.Length).IsEqualTo(2);
        await Assert.That(consumers.All(consumer => consumer.Equals(clientId))).IsTrue();
    }

    [Test]
    public async Task RemovingOneSubscriptionPreservesTheOtherForTheSameConsumer()
    {
        var index = _createIndex();
        var clientId = new ClientId("consumer");
        var nasdaq = Subscription.Of<RoutableMessages.PriceUpdated>(clientId, new ContentPattern("NASDAQ"));
        var nyse = Subscription.Of<RoutableMessages.PriceUpdated>(clientId, new ContentPattern("NYSE"));
        index.AddSubscriptions([nasdaq, nyse]);

        index.RemoveSubscriptions([nasdaq]);

        await Assert.That(index.FindSubscriptions(
            nasdaq.MessageTypeId,
            new MessageRoutingContent("NASDAQ", "MSFT"))).IsEmpty();
        await Assert.That(index.FindSubscriptions(
            nyse.MessageTypeId,
            new MessageRoutingContent("NYSE", "IBM"))).IsEquivalentTo([nyse]);
    }

    [Test]
    public async Task NonexistentRemovalDoesNotChangeExistingResults()
    {
        var index = _createIndex();
        var existing = Subscription.Of<RoutableMessages.PriceUpdated>(
            new ClientId("existing"),
            new ContentPattern("NASDAQ"));
        var missing = Subscription.Of<RoutableMessages.PriceUpdated>(
            new ClientId("missing"),
            new ContentPattern("NYSE"));
        index.AddSubscriptions([existing]);

        index.RemoveSubscriptions([missing]);

        await Assert.That(index.FindSubscriptions(
            existing.MessageTypeId,
            new MessageRoutingContent("NASDAQ", "MSFT"))).IsEquivalentTo([existing]);
    }

    [Test]
    public async Task RemovedSubscriptionCanBeAddedAgain()
    {
        var index = _createIndex();
        var subscription = Subscription.Of<RoutableMessages.PriceUpdated>(
            new ClientId("consumer"),
            new ContentPattern("NASDAQ"));
        var content = new MessageRoutingContent("NASDAQ", "MSFT");

        index.AddSubscriptions([subscription]);
        var added = index.FindSubscriptions(subscription.MessageTypeId, content).ToArray();
        index.RemoveSubscriptions([subscription]);
        var removed = index.FindSubscriptions(subscription.MessageTypeId, content).ToArray();
        index.AddSubscriptions([subscription]);
        var readded = index.FindSubscriptions(subscription.MessageTypeId, content).ToArray();

        await Assert.That(added).IsEquivalentTo([subscription]);
        await Assert.That(removed).IsEmpty();
        await Assert.That(readded).IsEquivalentTo([subscription]);
    }

    [Test]
    public async Task RemovingConsumerCoversEveryAffectedTypeAndPreservesOtherConsumers()
    {
        var index = _createIndex();
        var removedClient = new ClientId("removed");
        var retainedClient = new ClientId("retained");
        var removedPrice = Subscription.Of<RoutableMessages.PriceUpdated>(
            removedClient,
            new ContentPattern("NASDAQ"));
        var removedInstrument = Subscription.Of<RoutableMessages.InstrumentAdded>(
            removedClient,
            new ContentPattern("9"));
        var retainedPrice = Subscription.Of<RoutableMessages.PriceUpdated>(
            retainedClient,
            new ContentPattern("*"));
        index.AddSubscriptions([removedPrice, removedInstrument, retainedPrice]);

        index.RemoveSubscriptionsForConsumer(removedClient);

        await Assert.That(index.FindSubscriptions(
            removedPrice.MessageTypeId,
            new MessageRoutingContent("NASDAQ", "MSFT"))).IsEquivalentTo([retainedPrice]);
        await Assert.That(index.FindSubscriptions(
            removedInstrument.MessageTypeId,
            new MessageRoutingContent("9"))).IsEmpty();
    }
}

[InheritsTests]
public sealed class CustomTreeSubscriptionIndexContractTests : SubscriptionIndexContractTests
{
    public CustomTreeSubscriptionIndexContractTests()
        : base(static () => new SubscriptionIndex())
    {
    }
}

[InheritsTests]
public sealed class PattrnSubscriptionIndexContractTests : SubscriptionIndexContractTests
{
    public PattrnSubscriptionIndexContractTests()
        : base(static () => new PattrnSubscriptionIndex())
    {
    }
}
