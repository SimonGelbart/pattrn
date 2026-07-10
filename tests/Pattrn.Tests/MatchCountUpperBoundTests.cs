using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class MatchCountUpperBoundTests
{
    [Test]
    public void ExactModeUpperBoundCountsOnlyBranchesThatCanReachThePath()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .Add(["market", "NASDAQ"], 1)
            .Add(["market", "NASDAQ", "MSFT"], 2)
            .Add(["market", "NASDAQ", "*"], 3)
            .Add(["market", "NYSE", "IBM"], 4)
            .Add(["unrelated"], 5)
            .Build(MatchOptions.Prefix);

        var upperBound = index.GetMatchCountUpperBound(["market", "NASDAQ", "MSFT"]);

        ShouldEqual(upperBound, 2);
        Span<int> destination = stackalloc int[upperBound];
        var succeeded = index.TryMatch(["market", "NASDAQ", "MSFT"], destination, out var written);
        ShouldBeTrue(succeeded, "Expected exact TryMatch to succeed with exact upper-bound capacity.");
        ShouldEqual(written, 2);
        ShouldSetEqual(destination[..written].ToArray(), [2, 3]);
    }

    [Test]
    public void PrefixUpperBoundIncludesPrefixPatternValues()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .Add(["market"], 1)
            .Add(["market", "NASDAQ"], 2)
            .Add(["market", "NASDAQ", "*"], 3)
            .Add(["market", "NYSE"], 4)
            .Build(MatchOptions.Prefix);

        var upperBound = index.GetPrefixMatchCountUpperBound(["market", "NASDAQ", "MSFT"]);

        ShouldEqual(upperBound, 3);
        Span<int> destination = stackalloc int[upperBound];
        var succeeded = index.TryMatchPrefix(["market", "NASDAQ", "MSFT"], destination, out var written);
        ShouldEqual(written, 3);
        ShouldSetEqual(destination[..written].ToArray(), [1, 2, 3]);
    }

    [Test]
    public void DeduplicatedUpperBoundCanBeLargerThanActualWrittenCount()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .Add(["market", "NASDAQ", "MSFT"], "client-a")
            .Add(["market", "NASDAQ", "*"], "client-a")
            .Build();

        var upperBound = index.GetMatchCountUpperBound(["market", "NASDAQ", "MSFT"]);
        var destination = new string[upperBound];
        var succeeded = index.TryMatch(["market", "NASDAQ", "MSFT"], destination, out var written);

        ShouldEqual(upperBound, 2);
        ShouldEqual(written, 1);
        ShouldEqual(destination[0], "client-a");
    }

    [Test]
    public void PreserveDuplicatesUpperBoundEqualsWrittenCountForMatchedRegistrations()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .Add(["market", "NASDAQ", "MSFT"], "client-a")
            .Add(["market", "NASDAQ", "*"], "client-a")
            .Build(MatchOptions.PreserveDuplicates);

        var upperBound = index.GetPrefixMatchCountUpperBound(["market", "NASDAQ", "MSFT"]);
        var destination = new string[upperBound];
        var succeeded = index.TryMatchPrefix(["market", "NASDAQ", "MSFT"], destination, out var written);

        ShouldEqual(upperBound, 2);
        ShouldEqual(written, 2);
    }

    [Test]
    public void MemoryUpperBoundOverloadMatchesSpanOverload()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .Add(["a", "b"], 1)
            .Add(["a", "*"], 2)
            .Build();
        ReadOnlyMemory<string> path = new[] { "a", "b" };

        ShouldEqual(index.GetMatchCountUpperBound(path), index.GetMatchCountUpperBound(path.Span));
    }

    [Test]
    public void PrefixMemoryUpperBoundOverloadMatchesSpanOverload()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .Add(["a"], 1)
            .Add(["a", "b"], 2)
            .Add(["a", "*"], 3)
            .Build();
        ReadOnlyMemory<string> path = new[] { "a", "b" };

        ShouldEqual(index.GetPrefixMatchCountUpperBound(path), index.GetPrefixMatchCountUpperBound(path.Span));
    }
}
