

using System;
using System.Collections.Generic;
using System.Linq;

namespace Homework.Routing
{
    /// <summary>
    /// Message content pattern used for routable subscription.
    /// </summary>
    public readonly struct ContentPattern : IEquatable<ContentPattern>
    {
        /// <summary>
        /// Matches any message.
        /// </summary>
        public static readonly ContentPattern Any = new();

        private readonly IReadOnlyList<string>? _parts;

        public ContentPattern(IReadOnlyList<string> parts) => _parts = parts;

        public ContentPattern(params string[] parts) => _parts = parts;

        public IReadOnlyList<string> Parts => _parts ?? Array.Empty<string>();

        public bool Equals(ContentPattern other) => Parts.SequenceEqual(other.Parts);

        public override bool Equals(object? obj) => obj is ContentPattern other && Equals(other);

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            foreach (var part in Parts)
            {
                hashCode.Add(part);
            }

            return hashCode.ToHashCode();
        }

        public override string ToString() => "[" + string.Join(", ", Parts) + "]";

        public static ContentPattern Split(string parts) => new(parts.Split('.'));
    }
}
