using System.Linq;
using TUnit.Assertions.Enums;


// TODO: Make the existing tests pass
// TODO: Add missing matching tests
// TODO: Add subscription removal tests
namespace Homework.Routing
{
    public abstract class MessageRouterTests
    {
        private readonly ISubscriptionIndex _subscriptionIndex;
        private readonly MessageRouter _router;

        protected MessageRouterTests(ISubscriptionIndex subscriptionIndex)
        {
            _subscriptionIndex = subscriptionIndex;
            _router = new MessageRouter(_subscriptionIndex);
        }

        [Test]
        public async Task ShouldIncludeSingleMatchingSubscription()
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<SimpleMessages.ExchangeAdded>(clientId),
            });

            // Act
            var clientIds = _router.GetConsumers(new SimpleMessages.ExchangeAdded()).ToList();

            // Assert
            await Assert.That(clientIds).IsEquivalentTo(new[] { clientId }, CollectionOrdering.Matching);
        }

        [Test]
        public async Task ShouldIncludeMatchingClientForTwoMessages()
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<SimpleMessages.ExchangeAdded>(clientId),
                Subscription.Of<SimpleMessages.ExchangeTradingPhaseChanged>(clientId),
            });

            // Act
            var clientIdsForMessage1 = _router.GetConsumers(new SimpleMessages.ExchangeAdded()).ToList();
            var clientIdsForMessage2 = _router.GetConsumers(new SimpleMessages.ExchangeTradingPhaseChanged()).ToList();

            // Assert
            await Assert.That(clientIdsForMessage1).IsEquivalentTo(new[] { clientId }, CollectionOrdering.Matching);
            await Assert.That(clientIdsForMessage2).IsEquivalentTo(new[] { clientId }, CollectionOrdering.Matching);
        }

        [Test]
        public async Task ShouldExcludeSubscriptionWithOtherMessageType()
        {
            // Arrange
            var clientId1 = new ClientId("Client.1");
            var clientId2 = new ClientId("Client.2");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<SimpleMessages.ExchangeAdded>(clientId1),
                Subscription.Of<SimpleMessages.ExchangeTradingPhaseChanged>(clientId2),
            });

            // Act
            var clientIds = _router.GetConsumers(new SimpleMessages.ExchangeAdded()).ToList();

            // Assert
            await Assert.That(clientIds).IsEquivalentTo(new[] { clientId1 }, CollectionOrdering.Matching);
        }

        [Test]
        public async Task ShouldExcludeSingleSubscriptionWithOtherMessageType()
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<SimpleMessages.ExchangeAdded>(clientId),
            });

            // Act
            var clientIds = _router.GetConsumers(new SimpleMessages.ExchangeTradingPhaseChanged()).ToList();

            // Assert
            await Assert.That(clientIds).IsEmpty();
        }

        [Test]
        public async Task ShouldIncludeSingleMatchingRoutableSubscription()
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId, new ContentPattern("NASDAQ")),
            });

            var routableMessage = new RoutableMessages.PriceUpdated { ExchangeCode = "NASDAQ", Symbol = "MSFT" };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            await Assert.That(clientIds).IsEquivalentTo(new[] { clientId }, CollectionOrdering.Matching);
        }

        [Test]
        public async Task ShouldIncludeRoutableSubscriptionsForTwoClients()
        {
            // Arrange
            var clientId1 = new ClientId("Client.1");
            var clientId2 = new ClientId("Client.2");
            var clientId3 = new ClientId("Client.3");

            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId1, new ContentPattern("NASDAQ", "*")),
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId2, new ContentPattern("NYSE", "*")),
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId3, new ContentPattern("NASDAQ", "*")),
            });

            var routableMessage = new RoutableMessages.PriceUpdated { ExchangeCode = "NASDAQ", Symbol = "MSFT" };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            //The order of client for GetConsumers is not predictable because of the multithreading
            await Assert.That(clientIds.OrderBy(x => x.ToString())).IsEquivalentTo(new[] { clientId1, clientId3 }, CollectionOrdering.Matching);
        }

        [Test]
        public async Task ShouldIncludeMatchingRoutableSubscription()
        {
            // Arrange
            var clientId1 = new ClientId("Client.1");
            var clientId2 = new ClientId("Client.2");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId1, new ContentPattern("NASDAQ")),
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId2, new ContentPattern("NYSE")),
            });

            var routableMessage = new RoutableMessages.PriceUpdated { ExchangeCode = "NASDAQ", Symbol = "MSFT" };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            await Assert.That(clientIds).IsEquivalentTo(new[] { clientId1 }, CollectionOrdering.Matching);
        }


        [Test]
        public async Task ShouldExcludeRoutableSubscriptionWithRemovedClientSubscription()
        {
            // Arrange
            var clientId1 = new ClientId("Client.1");
            var clientId2 = new ClientId("Client.2");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId1, new ContentPattern("NASDAQ")),
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId2, new ContentPattern("NASDAQ")),
            });

            var routableMessage = new RoutableMessages.PriceUpdated { ExchangeCode = "NASDAQ", Symbol = "MSFT" };
            _subscriptionIndex.RemoveSubscriptionsForConsumer(clientId2);

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            await Assert.That(clientIds).IsEquivalentTo(new[] { clientId1 }, CollectionOrdering.Matching);
        }


        [Test]
        public async Task ShouldExcludeRoutableSubscriptionWithRemovedSubscription()
        {
            // Arrange
            var clientId = new ClientId("Client.1");

            var subscription = Subscription.Of<RoutableMessages.InstrumentAdded>(clientId, new ContentPattern("9"));
            _subscriptionIndex.AddSubscriptions(new[]
            {
                subscription
            });

            var routableMessage = new RoutableMessages.InstrumentAdded { ExchangeId = 9 };
            _subscriptionIndex.RemoveSubscriptions(new[]
            {
                subscription
            });
            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            await Assert.That(clientIds).IsEmpty();
        }


        [Test]
        public async Task ShouldExcludeRoutableSubscriptionWithOtherMessageType()
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.InstrumentAdded>(clientId, new ContentPattern("9")),
            });

            var routableMessage = new RoutableMessages.InstrumentDelisted { ExchangeId = 9 };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            await Assert.That(clientIds).IsEmpty();
        }



        [Test]
        public async Task ShouldExcludeRoutableSubscriptionWithOtherContent()
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId, new ContentPattern("NASDAQ")),
            });

            var routableMessage = new RoutableMessages.PriceUpdated { ExchangeCode = "NYSE" };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            await Assert.That(clientIds).IsEmpty();
        }

        [Test]
        [Arguments("NASDAQ")]
        [Arguments("NASDAQ.MSFT")]
        [Arguments("*")]
        [Arguments("*.MSFT")]
        [Arguments("*.*")]
        public async Task ShouldIncludeMatchingRoutableSubscriptionWithPattern(string contentPattern)
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId, ContentPattern.Split(contentPattern)),
            });

            var routableMessage = new RoutableMessages.PriceUpdated { ExchangeCode = "NASDAQ", Symbol = "MSFT" };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            await Assert.That(clientIds).IsEquivalentTo(new[] { clientId }, CollectionOrdering.Matching);
        }

        [Test]
        [Arguments("NASDAQ")]
        [Arguments("NASDAQ.42")]
        [Arguments("NASDAQ.*.*.*.MSFT")]
        [Arguments("NASDAQ.42.TECH.L.MSFT")]
        [Arguments("*")]
        [Arguments("*.*.*.*.MSFT")]
        [Arguments("*.*")]
        [Arguments("*.42.*.*.*")]
        [Arguments("*.*.*.*.*")]
        public async Task ShouldIncludeMatchingRoutableSubscriptionWithLongPattern(string contentPattern)
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            var subscription = Subscription.Of<RoutableMessages.InstrumentConnected>(clientId, ContentPattern.Split(contentPattern));

            var otherClientId = new ClientId("Client.2");
            var otherSubscription = Subscription.Of<RoutableMessages.InstrumentConnected>(otherClientId, ContentPattern.Split("NYSE.*.*.*.*"));

            _subscriptionIndex.AddSubscriptions(new[] { subscription, otherSubscription });

            var routableMessage = new RoutableMessages.InstrumentConnected { ExchangeCode = "NASDAQ", ProviderId = 42, Sector = "TECH", SymbolRangeStart = 'L', Symbol = "MSFT" };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            await Assert.That(clientIds).IsEquivalentTo(new[] { clientId }, CollectionOrdering.Matching);
        }


        [Test]
        [Arguments("*.*.*")]
        [Arguments("NASDAQ.*.*")]
        [Arguments("NASDAQ.MSFT.*")]
        [Arguments("*.MSFT.*")]
        public async Task ShouldNotFindMatchingRoutableSubscriptionWithPatternTooLong(string contentPattern)
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            var subscription = Subscription.Of<RoutableMessages.InstrumentConnected>(clientId, ContentPattern.Split(contentPattern));

            _subscriptionIndex.AddSubscriptions(new[] { subscription });

            var routableMessage = new RoutableMessages.PriceUpdated{ ExchangeCode = "NASDAQ", Symbol = "MSFT" };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            await Assert.That(clientIds).IsEmpty();
        }

        [Test]
        [Arguments("NYSE")]
        [Arguments("NASDAQ.AMZN")]
        [Arguments("NYSE.MSFT")]
        [Arguments("*.AMZN")]
        [Arguments("NYSE.*")]
        [Arguments("*.NASDAQ")]
        [Arguments("MSFT.NASDAQ")]
        public async Task ShouldExcludeSingleRoutableSubscriptionWithPattern(string contentPattern)
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId, ContentPattern.Split(contentPattern)),
            });

            var routableMessage = new RoutableMessages.PriceUpdated { ExchangeCode = "NASDAQ", Symbol = "MSFT" };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            await Assert.That(clientIds).IsEmpty();
        }

        [Test]
        public async Task ShouldSupportContentWithEmptyValue()
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.TradingHalted>(clientId, ContentPattern.Split("NASDAQ.*.*")),
            });

            var routableMessage = new RoutableMessages.TradingHalted { ExchangeCode = "NASDAQ", Symbol = "MSFT" };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            await Assert.That(clientIds).IsEquivalentTo(new[] { clientId }, CollectionOrdering.Matching);
        }
    }

    [InheritsTests]
    public sealed class CustomTreeMessageRouterTests : MessageRouterTests
    {
        public CustomTreeMessageRouterTests()
            : base(new SubscriptionIndex())
        {
        }
    }

    [InheritsTests]
    public sealed class PattrnMessageRouterTests : MessageRouterTests
    {
        public PattrnMessageRouterTests()
            : base(new PattrnSubscriptionIndex())
        {
        }
    }
}
