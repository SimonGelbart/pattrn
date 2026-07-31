

using System;

namespace Homework.Routing
{
    public readonly struct ClientId : IEquatable<ClientId>
    {
        private readonly string _value;

        public ClientId(string value) => _value = value;

        public bool Equals(ClientId other) => _value == other._value;

        public override bool Equals(object? obj) => obj is ClientId other && Equals(other);

        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString() => _value;
    }
}
