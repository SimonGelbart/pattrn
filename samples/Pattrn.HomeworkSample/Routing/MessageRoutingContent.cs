

using System.Collections.Generic;

namespace Homework.Routing
{
    /// <summary>
    /// String-based representation of the routable parts of a message.
    /// </summary>
    public readonly struct MessageRoutingContent
    {
        public static readonly MessageRoutingContent Empty = new();

        public MessageRoutingContent(IReadOnlyList<string> parts) => Parts = parts;

        public MessageRoutingContent(params string[] parts) => Parts = parts;

        public IReadOnlyList<string>? Parts { get; }

        public static MessageRoutingContent FromMessage(IMessage message)
        {
            return message is IRoutableMessage routableMessage ? routableMessage.GetContent() : Empty;
        }
    }
}
