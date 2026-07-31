

using System;

namespace Homework.Routing
{
    public readonly struct MessageTypeId : IEquatable<MessageTypeId>
    {
        private readonly string _value;

        public MessageTypeId(string value) => _value = value;

        public MessageTypeId(Type type) => _value = type.FullName!;

        public bool Equals(MessageTypeId other) => _value == other._value;

        public override bool Equals(object? obj) => obj is MessageTypeId other && Equals(other);

        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString() => _value;

        public static MessageTypeId FromMessage(IMessage message) => new MessageTypeId(message.GetType());

        public static MessageTypeId Of<T>() where T : IMessage => new MessageTypeId(typeof(T));
    }
}
