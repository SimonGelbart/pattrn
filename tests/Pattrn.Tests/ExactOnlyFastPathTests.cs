using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class ExactOnlyFastPathTests
{
    [Test]
    public void ExactOnlyIndexMatchesThroughLinearReadPath()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .Add(["root", "a", "b", "c"], 1)
            .Add(["root", "a", "b", "d"], 2)
            .Build();

        ShouldSequenceEqual(index.MatchPrefixToArray(["root", "a", "b", "c"]), [1]);
        ShouldEqual(index.GetMatchCountUpperBound(["root", "a", "b", "c"]), 1);
    }

    [Test]
    public void ExactOnlyIndexReturnsNoMatchWithoutMutatingDestination()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .Add(["root", "a"], 1)
            .Build();
        var destination = new[] { 42 };

        var succeeded = index.TryMatchPrefix(["root", "missing"], destination, out var written);

        ShouldEqual(written, 0);
        ShouldSequenceEqual(destination, [42]);
        ShouldEqual(index.GetMatchCountUpperBound(["root", "missing"]), 0);
    }

    [Test]
    public void ExactOnlyPrefixIndexCollectsOnlyPrefixValuesAlongTheLinearPath()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .Add(["root"], 1)
            .Add(["root", "a"], 2)
            .Add(["root", "a", "b"], 3)
            .Add(["root", "x"], 4)
            .Build(MatchOptions.Prefix);
        Span<int> destination = stackalloc int[index.GetPrefixMatchCountUpperBound(["root", "a", "b", "c"])];

        var succeeded = index.TryMatchPrefix(["root", "a", "b", "c"], destination, out var written);

        ShouldEqual(written, 3);
        ShouldSequenceEqual(destination[..written].ToArray(), [1, 2, 3]);
    }

    [Test]
    public void ExactOnlyPrefixIndexStopsAtFirstMissingSegmentButKeepsEarlierPrefixes()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .Add(["root"], 1)
            .Add(["root", "a"], 2)
            .Build(MatchOptions.Prefix);

        ShouldEqual(index.GetPrefixMatchCountUpperBound(["root", "a", "missing"]), 2);
        ShouldSequenceEqual(index.MatchPrefixToArray(["root", "a", "missing"]), [1, 2]);
    }
}
