using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class AdaptiveChildLookupTests
{
    [Test]
    public void WideExactFanOutMatchesExpectedChild()
    {
        var builder = PattrnIndex<string, int>.Builder("*");

        for (var i = 0; i < 128; i++)
        {
            builder.Add(["root", $"segment-{i}"], i);
        }

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(["root", "segment-73"]), [73]);
        ShouldEqual(index.GetMatchCountUpperBound(["root", "segment-73"]), 1);
        ShouldEqual(index.MatchToArray(["root", "missing"]).Length, 0);
    }

    [Test]
    public void WideExactFanOutUsesConfiguredSegmentComparer()
    {
        var builder = PattrnIndex<string, int>.Builder("*", StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < 128; i++)
        {
            builder.Add(["Root", $"Segment-{i}"], i);
        }

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(["root", "segment-73"]), [73]);
        ShouldEqual(index.MatchToArray(["root", "missing"]).Length, 0);
    }

    [Test]
    public void WideExactFanOutHandlesHashCollisions()
    {
        var builder = PattrnIndex<string, int>.Builder("*", new ConstantHashStringComparer());

        for (var i = 0; i < 128; i++)
        {
            builder.Add(["root", $"segment-{i}"], i);
        }

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(["root", "segment-73"]), [73]);
        ShouldEqual(index.MatchToArray(["root", "missing"]).Length, 0);
    }

    [Test]
    public void WideExactFanOutPreservesWildcardMatching()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        for (var i = 0; i < 128; i++)
        {
            builder.Add(["root", $"segment-{i}"], $"exact-{i}");
        }

        builder.Add(["root", "*"], "wildcard");

        var index = builder.Build();

        ShouldSetEqual(index.MatchToArray(["root", "segment-73"]), ["exact-73", "wildcard"]);
        ShouldSequenceEqual(index.MatchToArray(["root", "other"]), ["wildcard"]);
    }

    private sealed class ConstantHashStringComparer : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y) => StringComparer.Ordinal.Equals(x, y);

        public int GetHashCode(string obj) => 42;
    }
}
