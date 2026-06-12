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

        ShouldSequenceEqual(index.MatchToArray(["files"]), ["handler"]);
        ShouldSequenceEqual(index.MatchToArray(["files", "a", "b", "c.txt"]), ["handler"]);
        ShouldSequenceEqual(index.MatchToArray(["other", "a"]), []);
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
        ShouldEqual(matches[0].Captures.Count, 3);
        ShouldEqual(matches[0].Captures[0], new PatternCapture<string>("path", "a", 1));
        ShouldEqual(matches[0].Captures[1], new PatternCapture<string>("path", "b", 2));
        ShouldEqual(matches[0].Captures[2], new PatternCapture<string>("path", "c.txt", 3));
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
        ShouldEqual(matches[0].Captures.Count, 0);
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
    public void CatchAllMustBeTerminal()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        ShouldThrow<ArgumentException>(() => builder.AddPattern(
            [
                PatternSegment<string>.Literal("files"),
                PatternSegment<string>.CatchAll("path"),
                PatternSegment<string>.Literal("tail")
            ],
            "handler"));
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

        ShouldSetEqual(index.MatchToArray(["files"]), ["files", "catch-all"]);
    }
}
