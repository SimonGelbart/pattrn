using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class BuildHardeningTests
{
    [Test]
    public void BuildHandlesVeryDeepExactPatternWithoutRecursiveCompilerStackGrowth()
    {
        var pattern = CreateSegments("segment", 10_000);
        var builder = PattrnIndex<string, string>.Builder("*");

        builder.Add(pattern, "deep");
        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(pattern), ["deep"]);
    }

    [Test]
    public void BuildHandlesVeryDeepWildcardPatternWithoutRecursiveCompilerStackGrowth()
    {
        var pattern = CreateRepeatedSegment("*", 10_000);
        var path = CreateSegments("value", 10_000);
        var builder = PattrnIndex<string, string>.Builder("*");

        builder.Add(pattern, "deep-wildcard");
        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(path), ["deep-wildcard"]);
    }

    [Test]
    public void RemovePrunesLeafChildrenAfterLazyChildDictionaryAllocation()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        builder.Add(["root", "child"], "value");

        ShouldBeTrue(builder.Remove(["root", "child"], "value"), "Expected registration to be removed.");
        ShouldBeFalse(builder.Contains(["root", "child"]), "Expected pattern to be pruned.");
        ShouldEqual(builder.PatternCount, 0);
        ShouldEqual(builder.RegistrationCount, 0);

        var index = builder.Build();
        ShouldEqual(index.MatchToArray(["root", "child"]).Length, 0);
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
