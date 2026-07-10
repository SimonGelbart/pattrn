using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class CatchAllPatternTests
{
    [Test]
    public void CatchAllMatchesZeroOrMoreRemainingSegments()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.AddPattern(
            [
                PatternSegment<string>.Literal("files"),
                PatternSegment<string>.CatchAll()
            ],
            "handler");

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchPrefixToArray(["files"]), ["handler"]);
        ShouldSequenceEqual(index.MatchPrefixToArray(["files", "a", "b", "c.txt"]), ["handler"]);
        ShouldSequenceEqual(index.MatchPrefixToArray(["other", "a"]), []);
    }

    [Test]
    public void NamedCatchAllCapturesRemainingSegments()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.AddPattern(
            [
                PatternSegment<string>.Literal("files"),
                PatternSegment<string>.CatchAll("path")
            ],
            "handler");

        var index = builder.Build();
        var matches = index.MatchDetailedToArray(["files", "a", "b", "c.txt"]);

        ShouldEqual(matches.Length, 1);
        ShouldEqual(matches[0].Value, "handler");
        ShouldEqual(matches[0].Kind, PatternMatchKind.CatchAll);
        ShouldEqual(matches[0].Captures.Length, 1);
        ShouldEqual(matches[0].Captures[0], new PatternCapture<string>("path", ["a", "b", "c.txt"], 1));
    }

    [Test]
    public void UnnamedCatchAllProducesNoCapture()
    {
        var index = PattrnIndex<string, string>.Builder()
            .AddPattern([PatternSegment<string>.Literal("files"), PatternSegment<string>.CatchAll()], "handler")
            .Build();

        var matches = index.MatchDetailedToArray(["files", "a", "b.txt"]);

        ShouldEqual(matches.Length, 1);
        ShouldEqual(matches[0].Value, "handler");
        ShouldEqual(matches[0].Captures.Length, 0);
    }

    [Test]
    public void NamedCatchAllCanMatchEmptyRemainder()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.AddPattern(
            [
                PatternSegment<string>.Literal("files"),
                PatternSegment<string>.CatchAll("path")
            ],
            "handler");

        var index = builder.Build();
        var matches = index.MatchDetailedToArray(["files"]);

        ShouldEqual(matches.Length, 1);
        ShouldEqual(matches[0].Value, "handler");
        ShouldEqual(matches[0].Kind, PatternMatchKind.CatchAll);
        ShouldEqual(matches[0].Captures.Length, 1);
        ShouldEqual(matches[0].Captures[0], new PatternCapture<string>("path", [], 1));
    }

    [Test]
    public void CatchAllIsLessSpecificThanExactParameterAndWildcard()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.AddPattern([PatternSegment<string>.Literal("files"), PatternSegment<string>.CatchAll()], "catch-all");
        builder.AddPattern([PatternSegment<string>.Literal("files"), PatternSegment<string>.Wildcard()], "wildcard");
        builder.AddPattern([PatternSegment<string>.Literal("files"), PatternSegment<string>.Parameter("id")], "parameter");
        builder.AddPattern([PatternSegment<string>.Literal("files"), PatternSegment<string>.Literal("new")], "literal");

        var index = builder.Build(MatchOptions.PreserveDuplicates);
        var matches = index.MatchDetailedToArray(["files", "new"]);

        ShouldSetEqual(matches.Select(match => match.Value), ["literal", "parameter", "wildcard", "catch-all"]);
        ShouldBeTrue(matches.Single(match => match.Value == "literal").Specificity > matches.Single(match => match.Value == "parameter").Specificity, "Literal should score higher than parameter.");
        ShouldBeTrue(matches.Single(match => match.Value == "parameter").Specificity > matches.Single(match => match.Value == "wildcard").Specificity, "Parameter should score higher than wildcard.");
        ShouldBeTrue(matches.Single(match => match.Value == "wildcard").Specificity > matches.Single(match => match.Value == "catch-all").Specificity, "Wildcard should score higher than catch-all.");
    }

    [Test]
    public void NonTerminalCatchAllIsRejectedWithCurrentCompilerUnsupportedWording()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        var exception = ShouldThrow<ArgumentException>(() => builder.AddPattern(
            [
                PatternSegment<string>.Literal("files"),
                PatternSegment<string>.CatchAll("path"),
                PatternSegment<string>.Literal("tail")
            ],
            "handler"));

        ShouldBeTrue(
            exception.Message.Contains("not supported by this version", StringComparison.Ordinal),
            "The non-terminal catch-all error should describe current compiler support rather than permanent conceptual invalidity.");
    }

    [Test]
    public void DuplicateCaptureNamesWithinOnePatternAreRejected()
    {
        var builder = PattrnIndex<string, string>.Builder();

        ShouldThrow<ArgumentException>(() => builder.AddPattern(
            [
                PatternSegment<string>.Literal("orders"),
                PatternSegment<string>.Parameter("id"),
                PatternSegment<string>.Parameter("id")
            ],
            "handler"));
    }

    [Test]
    public void ParameterAndCatchAllWithSameNameWithinOnePatternAreRejected()
    {
        var builder = PattrnIndex<string, string>.Builder();

        ShouldThrow<ArgumentException>(() => builder.AddPattern(
            [
                PatternSegment<string>.Literal("files"),
                PatternSegment<string>.Parameter("path"),
                PatternSegment<string>.CatchAll("path")
            ],
            "handler"));
    }

    [Test]
    public void SameCaptureNameMayBeReusedInDifferentPatterns()
    {
        var index = PattrnIndex<string, string>.Builder()
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "orders")
            .AddPattern([PatternSegment<string>.Literal("customers"), PatternSegment<string>.Parameter("id")], "customers")
            .Build();

        ShouldSequenceEqual(index.MatchPrefixToArray(["orders", "123"]), ["orders"]);
        ShouldSequenceEqual(index.MatchPrefixToArray(["customers", "123"]), ["customers"]);
    }

    [Test]
    public void CaptureNameDuplicatesAreCaseSensitive()
    {
        var index = PattrnIndex<string, string>.Builder()
            .AddPattern(
                [
                    PatternSegment<string>.Literal("orders"),
                    PatternSegment<string>.Parameter("id"),
                    PatternSegment<string>.Parameter("ID")
                ],
                "handler")
            .Build();

        var match = index.MatchDetailedToArray(["orders", "123", "456"]).Single();

        ShouldSequenceEqual(match.Captures.Select(capture => capture.Name), ["id", "ID"]);
    }

    [Test]
    public void ContainsAndRemoveSupportCatchAllPatterns()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        var pattern = new[]
        {
            PatternSegment<string>.Literal("files"),
            PatternSegment<string>.CatchAll("path")
        };

        builder.AddPattern(pattern, "handler");

        ShouldBeTrue(builder.ContainsPattern(pattern), "Builder should contain the catch-all registration.");
        ShouldBeTrue(builder.RemovePattern(pattern, "handler"), "Builder should remove the catch-all registration.");
        ShouldBeFalse(builder.ContainsPattern(pattern), "Builder should not contain the removed catch-all registration.");
    }

    [Test]
    public void PrefixMatchingIncludesCatchAllForEmptyRemainder()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.AddPattern([PatternSegment<string>.Literal("files"), PatternSegment<string>.CatchAll()], "catch-all");
        builder.AddPattern([PatternSegment<string>.Literal("files")], "files");

        var index = builder.Build(MatchOptions.Prefix);

        ShouldSetEqual(index.MatchPrefixToArray(["files"]), ["files", "catch-all"]);
    }
}
