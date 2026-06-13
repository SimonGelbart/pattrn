using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class MatchCountUpperBoundTests
{
    [Test]
    public void ExactModeUpperBoundCountsOnlyBranchesThatCanReachThePath()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .Add(["market", "NASDAQ", "MSFT"], 1)
            .Add(["market", "NASDAQ", "*"], 2)
            .Add(["market", "NYSE", "IBM"], 3)
            .Add(["unrelated"], 4)
            .Build();

        var upperBound = index.GetMatchCountUpperBound(["market", "NASDAQ", "MSFT"]);

        ShouldEqual(upperBound, 2);
        Span<int> destination = stackalloc int[upperBound];
        var written = index.Match(["market", "NASDAQ", "MSFT"], destination);
        ShouldEqual(written, 2);
    }

    [Test]
    public void PrefixModeUpperBoundIncludesPrefixPatternValues()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .Add(["market"], 1)
            .Add(["market", "NASDAQ"], 2)
            .Add(["market", "NASDAQ", "*"], 3)
            .Add(["market", "NYSE"], 4)
            .Build(MatchOptions.Prefix);

        var upperBound = index.GetMatchCountUpperBound(["market", "NASDAQ", "MSFT"]);

        ShouldEqual(upperBound, 3);
        Span<int> destination = stackalloc int[upperBound];
        var written = index.Match(["market", "NASDAQ", "MSFT"], destination);
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
        var written = index.Match(["market", "NASDAQ", "MSFT"], destination);

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

        var upperBound = index.GetMatchCountUpperBound(["market", "NASDAQ", "MSFT"]);
        var destination = new string[upperBound];
        var written = index.Match(["market", "NASDAQ", "MSFT"], destination);

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
}
