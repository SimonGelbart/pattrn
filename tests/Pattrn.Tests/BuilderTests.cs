using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class BuilderTests
{
    [Test]
    public void MultipleValuesOnSamePatternCountAsOneDistinctPattern()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");

        builder.Add(["market", "NASDAQ"], "client-1");
        builder.Add(["market", "NASDAQ"], "client-2");

        ShouldEqual(builder.PatternCount, 1);
        ShouldEqual(builder.RegistrationCount, 2);

        var index = builder.Build();

        ShouldEqual(index.PatternCount, 1);
        ShouldEqual(index.RegistrationCount, 2);
        ShouldSetEqual(index.MatchToArray(["market", "NASDAQ"]), ["client-1", "client-2"]);
    }

    [Test]
    public void RemoveExistingRegistrationUpdatesCountsAndMatches()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.Add(["market", "NASDAQ"], "client-1");
        builder.Add(["market", "NASDAQ"], "client-2");

        var removed = builder.Remove(["market", "NASDAQ"], "client-1");

        ShouldBeTrue(removed, "Expected the registration to be removed.");
        ShouldEqual(builder.PatternCount, 1);
        ShouldEqual(builder.RegistrationCount, 1);
        ShouldSequenceEqual(builder.Build().MatchToArray(["market", "NASDAQ"]), ["client-2"]);
    }

    [Test]
    public void RemovingLastValueRemovesDistinctPatternCount()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.Add(["market", "NASDAQ"], "client-1");

        var removed = builder.Remove(["market", "NASDAQ"], "client-1");

        ShouldBeTrue(removed, "Expected the registration to be removed.");
        ShouldEqual(builder.PatternCount, 0);
        ShouldEqual(builder.RegistrationCount, 0);
        ShouldEqual(builder.Build().MatchToArray(["market", "NASDAQ"]).Length, 0);
    }

    [Test]
    public void RemoveMissingRegistrationReturnsFalseAndDoesNotChangeCounts()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.Add(["market", "NASDAQ"], "client-1");

        var removed = builder.Remove(["market", "NASDAQ"], "missing-client");

        ShouldBeFalse(removed, "Did not expect a missing registration to be removed.");
        ShouldEqual(builder.PatternCount, 1);
        ShouldEqual(builder.RegistrationCount, 1);
    }

    [Test]
    public void RemoveSupportsMemoryAndEnumerableConvenienceOverloads()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        string[] pattern = ["market", "NASDAQ"];
        builder.Add(pattern.AsMemory(), "client-1");
        builder.Add((IEnumerable<string>)pattern, "client-2");

        ShouldBeTrue(builder.Remove(pattern.AsMemory(), "client-1"), "Expected memory overload to remove a registration.");
        ShouldBeTrue(builder.Remove((IEnumerable<string>)pattern, "client-2"), "Expected enumerable overload to remove a registration.");
        ShouldEqual(builder.PatternCount, 0);
        ShouldEqual(builder.RegistrationCount, 0);
    }

    [Test]
    public void BuiltIndexIsImmutableSnapshotOfBuilderState()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.Add(["market", "NASDAQ", "MSFT"], "before-build");

        var firstIndex = builder.Build();

        builder.Add(["market", "NASDAQ", "AAPL"], "after-build");
        var secondIndex = builder.Build();

        ShouldSequenceEqual(firstIndex.MatchToArray(["market", "NASDAQ", "MSFT"]), ["before-build"]);
        ShouldEqual(firstIndex.MatchToArray(["market", "NASDAQ", "AAPL"]).Length, 0);
        ShouldSequenceEqual(secondIndex.MatchToArray(["market", "NASDAQ", "AAPL"]), ["after-build"]);
    }

    [Test]
    public void CaseInsensitiveSegmentComparerIsUsedForExactSegmentsAndWildcardDetection()
    {
        var builder = PattrnIndexBuilder<string, string>.Create(
            wildcardSegment: "*",
            segmentComparer: StringComparer.OrdinalIgnoreCase);

        builder.Add(["Market", "Nasdaq", "*"], "case-insensitive");

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(["market", "NASDAQ", "msft"]), ["case-insensitive"]);
    }

    [Test]
    public void CustomValueComparerIsUsedForRemovalAndDeduplication()
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        var builder = PattrnIndexBuilder<string, string>.Create("*", valueComparer: comparer);
        builder.Add(["market", "NASDAQ", "MSFT"], "Client-A");
        builder.Add(["market", "NASDAQ", "*"], "client-a");

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(["market", "NASDAQ", "MSFT"]), ["Client-A"]);
        ShouldBeTrue(builder.Remove(["market", "NASDAQ", "*"], "CLIENT-A"), "Expected removal to use the custom value comparer.");
    }
}
