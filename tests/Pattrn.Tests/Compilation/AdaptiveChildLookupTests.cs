using TUnit.Assertions.Enums;

namespace Pattrn.Tests.Compilation;

public sealed class AdaptiveChildLookupTests
{
    [Test]
    public async Task WideExactFanOutMatchesExpectedChild()
    {
        var builder = PattrnIndex<string, int>.Builder("*");

        for (var i = 0; i < 128; i++)
        {
            builder.Add(["root", $"segment-{i}"], i);
        }

        var index = builder.Build();

        await Assert.That(index.MatchValuesToArray(["root", "segment-73"])).IsEquivalentTo([73], CollectionOrdering.Matching);
        await Assert.That(index.GetMatchCountUpperBound(["root", "segment-73"])).IsEqualTo(1);
        await Assert.That(index.MatchValuesToArray(["root", "missing"]).Length).IsEqualTo(0);
    }

    [Test]
    public async Task WideExactFanOutUsesConfiguredSegmentComparer()
    {
        var builder = PattrnIndex<string, int>.Builder("*", StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < 128; i++)
        {
            builder.Add(["Root", $"Segment-{i}"], i);
        }

        var index = builder.Build();

        await Assert.That(index.MatchValuesToArray(["root", "segment-73"])).IsEquivalentTo([73], CollectionOrdering.Matching);
        await Assert.That(index.MatchValuesToArray(["root", "missing"]).Length).IsEqualTo(0);
    }

    [Test]
    public async Task WideExactFanOutHandlesHashCollisions()
    {
        var builder = PattrnIndex<string, int>.Builder("*", new ConstantHashStringComparer());

        for (var i = 0; i < 128; i++)
        {
            builder.Add(["root", $"segment-{i}"], i);
        }

        var index = builder.Build();

        await Assert.That(index.MatchValuesToArray(["root", "segment-73"])).IsEquivalentTo([73], CollectionOrdering.Matching);
        await Assert.That(index.MatchValuesToArray(["root", "missing"]).Length).IsEqualTo(0);
    }

    [Test]
    public async Task WideExactFanOutPreservesWildcardMatching()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        for (var i = 0; i < 128; i++)
        {
            builder.Add(["root", $"segment-{i}"], $"exact-{i}");
        }

        builder.Add(["root", "*"], "wildcard");

        var index = builder.Build();

        await Assert.That(index.MatchValuesToArray(["root", "segment-73"])).IsEquivalentTo(["exact-73", "wildcard"]);
        await Assert.That(index.MatchValuesToArray(["root", "other"])).IsEquivalentTo(["wildcard"], CollectionOrdering.Matching);
    }

    private sealed class ConstantHashStringComparer : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y) => StringComparer.Ordinal.Equals(x, y);

        public int GetHashCode(string obj) => 42;
    }
}

