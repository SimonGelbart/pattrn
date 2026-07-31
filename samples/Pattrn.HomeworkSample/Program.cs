using Homework.Routing;

var client = new ClientId("sample-client");
var registrations = new[]
{
    Subscription.Of<PriceUpdated>(client, ContentPattern.Any),
    Subscription.Of<PriceUpdated>(client, new ContentPattern("NASDAQ")),
    Subscription.Of<PriceUpdated>(client, new ContentPattern("*", "MSFT")),
};
var message = new PriceUpdated("NASDAQ", "MSFT");

foreach (var candidate in new (string Name, ISubscriptionIndex Index)[]
         {
             ("Homework tree", new SubscriptionIndex()),
             ("Pattrn snapshot", new PattrnSubscriptionIndex()),
         })
{
    candidate.Index.AddSubscriptions(registrations);
    var consumers = new MessageRouter(candidate.Index).GetConsumers(message).ToArray();
    Console.WriteLine($"{candidate.Name}: {string.Join(", ", consumers.Select(value => value.ToString()))}");
}

internal sealed record PriceUpdated(string Exchange, string Symbol) : IRoutableMessage
{
    public MessageRoutingContent GetContent() => new(Exchange, Symbol);
}
