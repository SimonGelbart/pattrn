

using System.Collections.Generic;

namespace Homework.Routing
{
    public class MessageRouter
    {
        private readonly ISubscriptionIndex _subscriptionIndex;

        public MessageRouter(ISubscriptionIndex subscriptionIndex)
        {
            _subscriptionIndex = subscriptionIndex;
        }

        public IEnumerable<ClientId> GetConsumers(IMessage message)
        {
            var messageTypeId = MessageTypeId.FromMessage(message);
            var messageContent = MessageRoutingContent.FromMessage(message);

            foreach (var subscription in _subscriptionIndex.FindSubscriptions(messageTypeId, messageContent))
            {
                yield return subscription.ConsumerId;
            }
        }
    }
}
