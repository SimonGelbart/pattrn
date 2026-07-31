
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Homework.Routing
{
    public class SubscriptionIndex : ISubscriptionIndex
    {
        private readonly ConcurrentDictionary<MessageTypeId, SubscriptionTree> _subscriptions = new();

        public void AddSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            foreach (var subscription in subscriptions)
            {
                var subscriptionTree = _subscriptions.GetOrAdd(subscription.MessageTypeId, new SubscriptionTree());
                subscriptionTree.Add(subscription);
            }
        }

        public void RemoveSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            foreach (var subscription in subscriptions)
            {
                if (_subscriptions.TryGetValue(subscription.MessageTypeId, out var subscriptionTree))
                {
                    subscriptionTree.Remove(subscription);
                }
            }
        }

        public void RemoveSubscriptionsForConsumer(ClientId consumer)
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Value.Remove(consumer);
            }
        }

        public IEnumerable<Subscription> FindSubscriptions(MessageTypeId messageTypeId,
            MessageRoutingContent routingContent)
        {
            if (!_subscriptions.TryGetValue(messageTypeId, out var subscriptionTree))
                return Array.Empty<Subscription>();

            IList<string> routingParts =(routingContent.Parts ?? Array.Empty<string>()).ToList();
            return subscriptionTree.GetSubscriptions(routingParts.ToList());
        }
    }
}
