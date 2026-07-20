using TUnit.Assertions.Enums;

namespace Pattrn.Tests;

public sealed class BuildHardeningTests
{
    [Test]
    public async Task BuildHandlesVeryDeepExactPatternWithoutRecursiveCompilerStackGrowth()
    {
        var pattern = CreateSegments("segment", 10_000);
        var builder = PattrnIndex<string, string>.Builder("*");

        builder.Add(pattern, "deep");
        var index = builder.Build();

        await Assert.That(index.MatchValuesToArray(pattern)).IsEquivalentTo(["deep"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task BuildHandlesVeryDeepWildcardPatternWithoutRecursiveCompilerStackGrowth()
    {
        var pattern = CreateRepeatedSegment("*", 10_000);
        var path = CreateSegments("value", 10_000);
        var builder = PattrnIndex<string, string>.Builder("*");

        builder.Add(pattern, "deep-wildcard");
        var index = builder.Build();

        await Assert.That(index.MatchValuesToArray(path)).IsEquivalentTo(["deep-wildcard"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task RemovePrunesLeafChildrenAfterLazyChildDictionaryAllocation()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        var id = builder.Add(
            (IReadOnlyList<PatternSegment<string>>)[PatternSegment<string>.Literal("root"), PatternSegment<string>.Literal("child")],
            "value");

        await Assert.That(builder.Remove(id)).IsTrue().Because("Expected registration to be removed.");
        await Assert.That(builder.Contains(["root", "child"])).IsFalse().Because("Expected pattern to be pruned.");
        await Assert.That(builder.PatternCount).IsEqualTo(0);
        await Assert.That(builder.RegistrationCount).IsEqualTo(0);

        var index = builder.Build();
        await Assert.That(index.MatchValuesToArray(["root", "child"]).Length).IsEqualTo(0);
    }

    private static string[] CreateSegments(string prefix, int count)
    {
        var segments = new string[count];
        for (var i = 0; i < segments.Length; i++)
        {
            segments[i] = $"{prefix}-{i}";
        }

        return segments;
    }

    private static string[] CreateRepeatedSegment(string segment, int count)
    {
        var segments = new string[count];
        Array.Fill(segments, segment);
        return segments;
    }
}
