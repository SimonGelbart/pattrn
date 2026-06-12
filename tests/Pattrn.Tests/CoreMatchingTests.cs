using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class CoreMatchingTests
{
    [Test]
    public void ExactPathMatchesRegisteredValue()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.Add(["market", "NASDAQ", "MSFT"], "exact-msft");

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(["market", "NASDAQ", "MSFT"]), ["exact-msft"]);
    }

    [Test]
    public void NonMatchingPathReturnsEmptyArray()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.Add(["market", "NASDAQ", "MSFT"], "exact-msft");

        var index = builder.Build();

        ShouldEqual(index.MatchToArray(["market", "NASDAQ", "AAPL"]).Length, 0);
    }

    [Test]
    public void WildcardCanMatchFirstMiddleAndLastSegments()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.Add(["*", "NASDAQ", "MSFT"], "first");
        builder.Add(["market", "*", "MSFT"], "middle");
        builder.Add(["market", "NASDAQ", "*"], "last");

        var index = builder.Build();

        ShouldSetEqual(index.MatchToArray(["market", "NASDAQ", "MSFT"]), ["first", "middle", "last"]);
    }

    [Test]
    public void MultipleWildcardsCanMatchTheSamePath()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.Add(["market", "*", "*"], "market-any-any");
        builder.Add(["*", "NASDAQ", "*"], "any-nasdaq-any");
        builder.Add(["*", "*", "MSFT"], "any-any-msft");

        var index = builder.Build();

        ShouldSetEqual(
            index.MatchToArray(["market", "NASDAQ", "MSFT"]),
            ["market-any-any", "any-nasdaq-any", "any-any-msft"]);
    }

    [Test]
    public void OverlappingExactAndWildcardPatternsReturnAllMatches()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.Add(["market", "NASDAQ", "MSFT"], "exact");
        builder.Add(["market", "NASDAQ", "*"], "any-nasdaq");
        builder.Add(["market", "*", "MSFT"], "any-market-msft");

        var index = builder.Build();

        ShouldSetEqual(index.MatchToArray(["market", "NASDAQ", "MSFT"]), ["exact", "any-nasdaq", "any-market-msft"]);
    }

    [Test]
    public void EmptyPatternMatchesOnlyEmptyPathByDefault()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.Add([], "root");

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray([]), ["root"]);
        ShouldEqual(index.MatchToArray(["market"]).Length, 0);
    }

    [Test]
    public void PrefixMatchingIsExplicitlyDisabledByDefault()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.Add(["NASDAQ"], "nasdaq-prefix");

        var index = builder.Build();

        ShouldEqual(index.MatchToArray(["NASDAQ", "MSFT"]).Length, 0);
    }

    [Test]
    public void PrefixMatchingCanBeEnabledForStarterKitCompatibleBehavior()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.Add(["NASDAQ"], "nasdaq-prefix");
        builder.Add(["NASDAQ", "MSFT"], "exact-msft");

        var index = builder.Build(MatchOptions.Prefix);

        ShouldSetEqual(index.MatchToArray(["NASDAQ", "MSFT"]), ["nasdaq-prefix", "exact-msft"]);
    }

    [Test]
    public void EmptyPatternMatchesAnyPathWhenPrefixMatchingIsEnabled()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.Add([], "root-prefix");

        var index = builder.Build(MatchOptions.Prefix);

        ShouldSequenceEqual(index.MatchToArray(["market", "NASDAQ", "MSFT"]), ["root-prefix"]);
    }

    [Test]
    public void DeduplicationIsEnabledByDefaultAcrossOverlappingPatterns()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.Add(["market", "NASDAQ", "MSFT"], "same-consumer");
        builder.Add(["market", "NASDAQ", "*"], "same-consumer");

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(["market", "NASDAQ", "MSFT"]), ["same-consumer"]);
    }

    [Test]
    public void DeduplicationCanBeDisabled()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.Add(["market", "NASDAQ", "MSFT"], "same-consumer");
        builder.Add(["market", "NASDAQ", "*"], "same-consumer");

        var index = builder.Build(MatchOptions.PreserveDuplicates);

        ShouldSequenceEqual(index.MatchToArray(["market", "NASDAQ", "MSFT"]).Order(), ["same-consumer", "same-consumer"]);
    }

    [Test]
    public void CallerProvidedDestinationReceivesMatchesWithoutAllocatingResultArray()
    {
        var builder = PattrnIndexBuilder<string, int>.Create("*");
        builder.Add(["market", "NASDAQ", "MSFT"], 1);
        builder.Add(["market", "NASDAQ", "*"], 2);

        var index = builder.Build();
        Span<int> destination = stackalloc int[2];

        var written = index.Match(["market", "NASDAQ", "MSFT"], destination);

        ShouldEqual(written, 2);
        ShouldSetEqual(destination[..written].ToArray(), [1, 2]);
    }

    [Test]
    public void CallerProvidedDestinationThrowsWhenTooSmall()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.Add(["market", "NASDAQ", "MSFT"], "exact");
        builder.Add(["market", "NASDAQ", "*"], "wildcard");

        var index = builder.Build();
        var destination = new string[1];

        ShouldThrow<ArgumentException>(() => index.Match(["market", "NASDAQ", "MSFT"], destination));
    }
}

public sealed class AllocationSensitiveMatchingTests
{
    [Test]
    public void DestinationCapacityIsMeasuredAfterDeduplicationWhenDeduplicationIsEnabled()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["NASDAQ"], "same-client");
        builder.Add(["NASDAQ", "*"], "same-client");
        builder.Add(["NASDAQ", "MSFT"], "same-client");
        var index = builder.Build(MatchOptions.Prefix);
        var destination = new string[1];

        var written = index.Match(["NASDAQ", "MSFT"], destination);

        ShouldEqual(written, 1);
        ShouldSequenceEqual(destination[..written].ToArray(), ["same-client"]);
    }

    [Test]
    public void DestinationCapacityUsesRawMatchCountWhenDeduplicationIsDisabled()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["NASDAQ"], "same-client");
        builder.Add(["NASDAQ", "*"], "same-client");
        builder.Add(["NASDAQ", "MSFT"], "same-client");
        var index = builder.Build(new MatchOptions(PrefixMatchMode.IncludePrefixPatterns, DuplicateValueMatchMode.PreserveDuplicates));
        var destination = new string[1];

        ShouldThrow<ArgumentException>(() => index.Match(["NASDAQ", "MSFT"], destination));
    }

    [Test]
    public void SpanDestinationIsNotMutatedWhenThereAreNoMatches()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["NASDAQ", "MSFT"], "client");
        var index = builder.Build();
        var destination = new[] { "sentinel" };

        var written = index.Match(["NYSE", "IBM"], destination);

        ShouldEqual(written, 0);
        ShouldSequenceEqual(destination, ["sentinel"]);
    }
}

public sealed class MatchCapacityTests
{
    [Test]
    public void MatchCountUpperBoundIsSafeDestinationCapacityUpperBound()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["market"], "market-prefix");
        builder.Add(["market", "NASDAQ", "MSFT"], "exact");
        builder.Add(["market", "NASDAQ", "*"], "wildcard-symbol");
        builder.Add(["market", "*", "MSFT"], "wildcard-market");
        builder.Add(["market", "*", "*"], "wildcard-both");

        var index = builder.Build(new MatchOptions(PrefixMatchMode.IncludePrefixPatterns, DuplicateValueMatchMode.PreserveDuplicates));
        var destination = new string[index.MatchCountUpperBound];

        var written = index.Match(["market", "NASDAQ", "MSFT"], destination);

        ShouldEqual(written, index.MatchCountUpperBound);
        ShouldSetEqual(
            destination[..written],
            ["market-prefix", "exact", "wildcard-symbol", "wildcard-market", "wildcard-both"]);
    }
}

public sealed class ChildMapRefactorCoverageTests
{
    [Test]
    public void SparseDeepExactPathsRemainCorrect()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["a", "b", "c", "d", "e", "f", "g", "h"], "deep");
        builder.Add(["a", "b", "c", "d", "e", "f", "g", "*"], "deep-wildcard");

        var index = builder.Build();

        ShouldSetEqual(index.MatchToArray(["a", "b", "c", "d", "e", "f", "g", "h"]), ["deep", "deep-wildcard"]);
        ShouldSequenceEqual(index.MatchToArray(["a", "b", "c", "d", "e", "f", "g", "x"]), ["deep-wildcard"]);
    }

    [Test]
    public void LargeFanOutExactPathsRemainCorrect()
    {
        var builder = PattrnIndex<string, int>.Builder("*");
        for (var i = 0; i < 32; i++)
        {
            builder.Add(["root", $"child-{i}"], i);
        }

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(["root", "child-17"]), [17]);
        ShouldEqual(index.MatchToArray(["root", "child-99"]).Length, 0);
    }

    [Test]
    public void SmallFanOutExactPathsRemainCorrect()
    {
        var builder = PattrnIndex<string, int>.Builder("*");
        builder.Add(["root", "a"], 1);
        builder.Add(["root", "b"], 2);
        builder.Add(["root", "c"], 3);

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(["root", "b"]), [2]);
    }
}
