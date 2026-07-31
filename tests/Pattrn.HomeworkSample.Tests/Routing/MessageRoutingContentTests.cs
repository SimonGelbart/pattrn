

using System;
using TUnit.Assertions.Enums;


namespace Homework.Routing
{
    public class MessageRoutingContentTests
    {
        [Test]
        public async Task ShouldGetContentWithEmptyValue()
        {
            // Arrange
            var message = new RoutableMessages.TradingHalted
            {
                ExchangeCode = "NASDAQ",
                Symbol = "AMZN",
                TimestampUtc = DateTime.UtcNow,
            };

            // Act
            var routingContent = MessageRoutingContent.FromMessage(message);

            // Assert
            await Assert.That(routingContent.Parts).IsEquivalentTo(new[] { "NASDAQ", string.Empty, "AMZN" }, CollectionOrdering.Matching);
        }
    }
}
