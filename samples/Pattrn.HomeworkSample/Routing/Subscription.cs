

using System;

namespace Homework.Routing
{
    /// <summary>
    /// Represents a message subscription from a consumer.
    /// </summary>
    public class Subscription : IEquatable<Subscription>
    {
        public Subscription(ClientId consumerId, MessageTypeId messageTypeId, ContentPattern contentPattern)
        {
            ConsumerId = consumerId;
            MessageTypeId = messageTypeId;
            ContentPattern = contentPattern;
        }

        public ClientId ConsumerId { get; }
        public MessageTypeId MessageTypeId { get; }
        public ContentPattern ContentPattern { get; }

        public bool Equals(Subscription? other) => other != null
                                                   && ConsumerId.Equals(other.ConsumerId)
                                                   && MessageTypeId.Equals(other.MessageTypeId)
                                                   && ContentPattern.Equals(other.ContentPattern);

        public override bool Equals(object? obj) => obj is Subscription subscription && Equals(subscription);

        public override int GetHashCode() => HashCode.Combine(ConsumerId, MessageTypeId, ContentPattern);

        public static Subscription Of<T>(ClientId clientId, ContentPattern contentPattern = default)
            where T : IMessage
            => new(clientId, MessageTypeId.Of<T>(), contentPattern);
    }
}
