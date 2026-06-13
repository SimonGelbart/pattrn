using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class CompatibilitySemanticsTests
{
    [Test]
    public void DetailedMatchesAreOrderedByDeterministicTraversalSpecificity()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.CatchAll("tail")], "catch-all")
            .Add(["orders", "*"], "wildcard")
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "parameter")
            .Add(["orders", "new"], "literal")
            .Build(MatchOptions.PreserveDuplicates);

        var matches = index.MatchDetailedToArray(["orders", "new"]);

        ShouldSequenceEqual(matches.Select(match => match.Value), ["literal", "parameter", "wildcard", "catch-all"]);
        ShouldSequenceEqual(
            matches.Select(match => match.Kind),
            [PatternMatchKind.Exact, PatternMatchKind.Parameter, PatternMatchKind.Wildcard, PatternMatchKind.CatchAll]);
        ShouldBeTrue(matches[0].Specificity > matches[1].Specificity, "Literal matches should be more specific than parameter matches.");
        ShouldBeTrue(matches[1].Specificity > matches[2].Specificity, "Parameter matches should be more specific than anonymous wildcard matches.");
        ShouldBeTrue(matches[2].Specificity > matches[3].Specificity, "Anonymous wildcard matches should be more specific than catch-all matches.");
    }

    [Test]
    public void CapturesAreWrittenInPatternOrderAndUseInputSegmentIndexes()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .AddPattern(
                [
                    PatternSegment<string>.Literal("customers"),
                    PatternSegment<string>.Parameter("customerId"),
                    PatternSegment<string>.Literal("orders"),
                    PatternSegment<string>.Parameter("orderId"),
                    PatternSegment<string>.CatchAll("tail")
                ],
                "handler")
            .Build();

        var matches = index.MatchDetailedToArray(["customers", "42", "orders", "99", "items", "7"]);

        ShouldEqual(matches.Length, 1);
        ShouldSequenceEqual(
            matches[0].Captures,
            [
                new PatternCapture<string>("customerId", "42", 1),
                new PatternCapture<string>("orderId", "99", 3),
                new PatternCapture<string>("tail", "items", 4),
                new PatternCapture<string>("tail", "7", 5)
            ]);
    }

    [Test]
    public void CatchAllMatchesEmptyRemainderAndManySegmentRemainder()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .AddPattern([PatternSegment<string>.Literal("files"), PatternSegment<string>.CatchAll("path")], "files")
            .Build();

        var emptyRemainder = index.MatchDetailedToArray(["files"]);
        var manyRemainder = index.MatchDetailedToArray(["files", "a", "b", "c.txt"]);

        ShouldEqual(emptyRemainder.Length, 1);
        ShouldEqual(emptyRemainder[0].Captures.Count, 0);
        ShouldEqual(manyRemainder.Length, 1);
        ShouldSequenceEqual(
            manyRemainder[0].Captures,
            [
                new PatternCapture<string>("path", "a", 1),
                new PatternCapture<string>("path", "b", 2),
                new PatternCapture<string>("path", "c.txt", 3)
            ]);
    }

    [Test]
    public void DuplicatePoliciesHaveStableRegistrationSemantics()
    {
        var append = PattrnIndex<string, string>
            .Builder("*")
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Append)
            .Add(["orders", "new"], "first")
            .Add(["orders", "new"], "second")
            .Build(MatchOptions.PreserveDuplicates);

        var replace = PattrnIndex<string, string>
            .Builder("*")
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Replace)
            .Add(["orders", "new"], "first")
            .Add(["orders", "new"], "second")
            .Build(MatchOptions.PreserveDuplicates);

        var ignore = PattrnIndex<string, string>
            .Builder("*")
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Ignore)
            .Add(["orders", "new"], "first")
            .Add(["orders", "new"], "second")
            .Build(MatchOptions.PreserveDuplicates);

        ShouldSequenceEqual(append.MatchToArray(["orders", "new"]), ["first", "second"]);
        ShouldSequenceEqual(replace.MatchToArray(["orders", "new"]), ["second"]);
        ShouldSequenceEqual(ignore.MatchToArray(["orders", "new"]), ["first"]);
        ShouldThrow<InvalidOperationException>(() => PattrnIndex<string, string>
            .Builder("*")
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Throw)
            .Add(["orders", "new"], "first")
            .Add(["orders", "new"], "second"));
    }

    [Test]
    public void DetailedLowAllocationApiUsesStableCaptureSlices()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "one")
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("name")], "two")
            .Build(MatchOptions.PreserveDuplicates);

        var path = new[] { "orders", "123" };
        var matches = new PatternMatch<string>[index.GetMatchCountUpperBound(path)];
        var captures = new PatternCapture<string>[index.GetCaptureCountUpperBound(path)];

        var matchCount = index.MatchDetailed(path, matches, captures, out var captureCount);

        ShouldEqual(matchCount, 2);
        ShouldEqual(captureCount, 2);
        ShouldEqual(matches[0].CaptureStart, 0);
        ShouldEqual(matches[0].CaptureCount, 1);
        ShouldEqual(captures[matches[0].CaptureStart], new PatternCapture<string>("id", "123", 1));
        ShouldEqual(matches[1].CaptureStart, 1);
        ShouldEqual(matches[1].CaptureCount, 1);
        ShouldEqual(captures[matches[1].CaptureStart], new PatternCapture<string>("name", "123", 1));
    }

    [Test]
    public void TryMatchDetailedFailureLeavesReportedCountsAtZero()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "handler")
            .Build();

        var matches = new PatternMatch<string>[1];
        var captures = Array.Empty<PatternCapture<string>>();

        var succeeded = index.TryMatchDetailed(["orders", "123"], matches, captures, out var matchesWritten, out var capturesWritten);

        ShouldBeFalse(succeeded, "The capture destination is too small.");
        ShouldEqual(matchesWritten, 0);
        ShouldEqual(capturesWritten, 0);
    }
}
