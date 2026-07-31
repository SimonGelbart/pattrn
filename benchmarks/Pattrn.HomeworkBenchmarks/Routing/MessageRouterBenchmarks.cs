using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Homework.Routing
{
    [MemoryDiagnoser]
    public class MessageRouterBenchmarks
    {
        private MessageRouter _customRouter = null!;
        private MessageRouter _pattrnRouter = null!;
        private RoutableMessage0 _message = null!;

        [GlobalSetup]
        public void Setup()
        {
            var subscriptions = BuildSubscriptions();
            var customIndex = new SubscriptionIndex();
            customIndex.AddSubscriptions(subscriptions);
            var pattrnIndex = new PattrnSubscriptionIndex();
            pattrnIndex.AddSubscriptions(subscriptions);
            _customRouter = new MessageRouter(customIndex);
            _pattrnRouter = new MessageRouter(pattrnIndex);
            _message = new RoutableMessage0 { Id = 999, Value = 1234m };
        }

        private static Subscription[] BuildSubscriptions()
        {
            var baseTypeName = typeof(RoutableMessage0).FullName!.TrimEnd('0');

            return (from clientIndex in Enumerable.Range(0, 30)
                    let clientId = new ClientId($"Client.{clientIndex}")
                    from typeIndex in Enumerable.Range(0, 10)
                    let messageTypeId = new MessageTypeId($"{baseTypeName}{typeIndex}")
                    from contentIndex in Enumerable.Range(0, 4_000)
                    select new Subscription(
                        clientId,
                        messageTypeId,
                        new ContentPattern(contentIndex.ToString())))
                .ToArray();
        }

        [Benchmark(Baseline = true)]
        public List<ClientId> CustomTree_GetConsumers() => _customRouter.GetConsumers(_message).ToList();

        [Benchmark]
        public List<ClientId> Pattrn_GetConsumers() => _pattrnRouter.GetConsumers(_message).ToList();

        public class RoutableMessage0 : IRoutableMessage
        {
            public int Id { get; set; }
            public decimal Value { get; set; }

            public MessageRoutingContent GetContent() => new(Id.ToString(), Value.ToString());
        }
    }
}
