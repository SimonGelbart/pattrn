using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class DuplicateHeavyMatchingTests
{
    [Test]
    public void DuplicateHeavyOverlappingBranchesReturnUniqueValuesOnceByDefault()
    {
        var builder = PattrnIndex<string, int>.Builder("*");

        for (var value = 0; value < 64; value++)
        {
            builder.Add(["market", "M42", "S42"], value);
            builder.Add(["market", "M42", "*"], value);
            builder.Add(["market", "*", "S42"], value);
            builder.Add(["market", "*", "*"], value);
        }

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(["market", "M42", "S42"]), Enumerable.Range(0, 64));
    }

    [Test]
    public void DuplicateHeavyOverlappingBranchesCanWriteUniqueValuesToCallerSpan()
    {
        var builder = PattrnIndex<string, int>.Builder("*");

        for (var value = 0; value < 64; value++)
        {
            builder.Add(["market", "M42", "S42"], value);
            builder.Add(["market", "M42", "*"], value);
            builder.Add(["market", "*", "S42"], value);
            builder.Add(["market", "*", "*"], value);
        }

        var index = builder.Build();
        var destination = new int[index.MatchCountUpperBound];

        var written = index.Match(["market", "M42", "S42"], destination);

        ShouldEqual(written, 64);
        ShouldSequenceEqual(destination[..written], Enumerable.Range(0, 64));
    }

    [Test]
    public void DuplicateHeavyOverlappingBranchesPreserveRawMatchesWhenDeduplicationIsDisabled()
    {
        var builder = PattrnIndex<string, int>.Builder("*");

        for (var value = 0; value < 64; value++)
        {
            builder.Add(["market", "M42", "S42"], value);
            builder.Add(["market", "M42", "*"], value);
            builder.Add(["market", "*", "S42"], value);
            builder.Add(["market", "*", "*"], value);
        }

        var index = builder.Build(MatchOptions.PreserveDuplicates);
        var matches = index.MatchToArray(["market", "M42", "S42"]);

        ShouldEqual(matches.Length, 256);

        foreach (var value in Enumerable.Range(0, 64))
        {
            ShouldEqual(matches.Count(match => match == value), 4);
        }
    }

    [Test]
    public void RepeatedValueOnSamePatternIsDeduplicatedByDefault()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["market", "NASDAQ", "MSFT"], "client-a");
        builder.Add(["market", "NASDAQ", "MSFT"], "client-a");

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(["market", "NASDAQ", "MSFT"]), ["client-a"]);
    }

    [Test]
    public void RepeatedValueOnSamePatternIsPreservedWhenDeduplicationIsDisabled()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["market", "NASDAQ", "MSFT"], "client-a");
        builder.Add(["market", "NASDAQ", "MSFT"], "client-a");

        var index = builder.Build(MatchOptions.PreserveDuplicates);

        ShouldSequenceEqual(index.MatchToArray(["market", "NASDAQ", "MSFT"]), ["client-a", "client-a"]);
    }

    [Test]
    public void DuplicateHeavyDetailedMatchingDeduplicatesEqualValuesOnceByDefault()
    {
        var builder = PattrnIndex<string, int>.Builder("*");

        for (var value = 0; value < 64; value++)
        {
            builder.Add(["market", "M42", "S42"], value);
            builder.Add(["market", "M42", "*"], value);
            builder.Add(["market", "*", "S42"], value);
            builder.Add(["market", "*", "*"], value);
        }

        var index = builder.Build();
        var matches = new PatternMatch<int>[index.GetMatchCountUpperBound(["market", "M42", "S42"])];
        var captures = new PatternCapture<string>[Math.Max(1, index.GetCaptureCountUpperBound(["market", "M42", "S42"]))];

        var matchCount = index.MatchDetailed(["market", "M42", "S42"], matches, captures, out var captureCount);

        ShouldEqual(matchCount, 64);
        ShouldEqual(captureCount, 0);
        ShouldSequenceEqual(matches[..matchCount].Select(match => match.Value), Enumerable.Range(0, 64));
    }

    [Test]
    public void DuplicateHeavyDetailedMatchingPreservesRawMatchesWhenDeduplicationIsDisabled()
    {
        var builder = PattrnIndex<string, int>.Builder("*");

        for (var value = 0; value < 64; value++)
        {
            builder.Add(["market", "M42", "S42"], value);
            builder.Add(["market", "M42", "*"], value);
            builder.Add(["market", "*", "S42"], value);
            builder.Add(["market", "*", "*"], value);
        }

        var index = builder.Build(MatchOptions.PreserveDuplicates);
        var matches = index.MatchDetailed(["market", "M42", "S42"]);

        ShouldEqual(matches.Length, 256);
        foreach (var value in Enumerable.Range(0, 64))
        {
            ShouldEqual(matches.Count(match => match.Value == value), 4);
        }
    }

}
