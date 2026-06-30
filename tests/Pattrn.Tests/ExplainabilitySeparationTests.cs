using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class ExplainabilitySeparationTests
{
    [Test]
    public void ExplainReturnsAcceptedDetailedMatchesWithoutChangingHotPathResults()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .AddPattern(
                [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")],
                "handler",
                patternId: "orders-by-id")
            .Build();

        var hotMatches = index.MatchToArray(["orders", "123"]);
        var explanation = index.Explain(["orders", "123"]);

        ShouldSequenceEqual(hotMatches, ["handler"]);
        ShouldBeTrue(explanation.HasMatches, "Explanation should report accepted matches.");
        ShouldEqual(explanation.MatchCount, 1);
        ShouldEqual(explanation.Matches[0].Value, "handler");
        ShouldEqual(explanation.Matches[0].PatternId, "orders-by-id");
        ShouldEqual(explanation.Matches[0].Captures[0], new PatternCapture<string>("id", "123", 1));
        ShouldSequenceEqual(explanation.Path, ["orders", "123"]);
    }

    [Test]
    public void ExplainDoesNotCollectRejectedCandidatesByDefault()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .Add(["orders", "new"], "handler")
            .Build();

        var explanation = index.Explain(["customers", "42"]);

        ShouldBeFalse(explanation.HasMatches, "The path should not match.");
        ShouldEqual(explanation.RejectedCandidates.Count, 0);
        ShouldBeFalse(explanation.ExplanationOptions.IncludeRejectedCandidates, "Rejected-candidate diagnostics should be opt-in.");
    }

    [Test]
    public void ExplainCanCollectRejectedCandidatesWhenRequested()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .Add(["orders", "new"], "handler")
            .Build();

        var explanation = index.Explain(["customers", "42"], PatternExplanationOptions.IncludeRejections);

        ShouldBeFalse(explanation.HasMatches, "The path should not match.");
        ShouldBeTrue(explanation.ExplanationOptions.IncludeRejectedCandidates, "Explanation should record the requested diagnostic option.");
        ShouldEqual(explanation.RejectedCandidates.Count, 1);
        ShouldEqual(explanation.RejectedCandidates[0].PathDepth, 0);
        ShouldEqual(explanation.RejectedCandidates[0].ReasonKind, PatternRejectedCandidateReasonKind.LiteralMismatch);
        ShouldBeFalse(string.IsNullOrWhiteSpace(explanation.RejectedCandidates[0].Reason), "Human-readable reason text should remain populated.");
    }

    [Test]
    public void ExplainReportsEndOfInputRejectionWhenPathStopsBeforeTerminalRegistration()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .Add(["orders", "new"], "handler")
            .Build();

        var explanation = index.Explain(["orders"], new PatternExplanationOptions(includeRejectedCandidates: true));

        ShouldBeFalse(explanation.HasMatches, "The shorter path should not match in exact mode.");
        ShouldEqual(explanation.RejectedCandidates.Count, 1);
        ShouldEqual(explanation.RejectedCandidates[0].PathDepth, 1);
        ShouldEqual(explanation.RejectedCandidates[0].ReasonKind, PatternRejectedCandidateReasonKind.PathTooShort);
        ShouldBeFalse(string.IsNullOrWhiteSpace(explanation.RejectedCandidates[0].Reason), "Human-readable reason text should remain populated.");
    }

    [Test]
    public void ExplainReportsBranchMismatchReasonKindForWildcardIndexes()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Wildcard()], "handler")
            .Build();

        var explanation = index.Explain(["customers", "42"], PatternExplanationOptions.IncludeRejections);

        ShouldBeFalse(explanation.HasMatches, "The path should not match.");
        ShouldEqual(explanation.RejectedCandidates.Count, 1);
        ShouldEqual(explanation.RejectedCandidates[0].PathDepth, 0);
        ShouldEqual(explanation.RejectedCandidates[0].ReasonKind, PatternRejectedCandidateReasonKind.BranchNotMatched);
        ShouldBeFalse(string.IsNullOrWhiteSpace(explanation.RejectedCandidates[0].Reason), "Human-readable reason text should remain populated.");
    }

    [Test]
    public void RejectedCandidateReasonKindNumericValuesAreStable()
    {
        ShouldEqual((int)PatternRejectedCandidateReasonKind.None, 0);
        ShouldEqual((int)PatternRejectedCandidateReasonKind.LiteralMismatch, 1);
        ShouldEqual((int)PatternRejectedCandidateReasonKind.PathTooShort, 5);
        ShouldEqual((int)PatternRejectedCandidateReasonKind.BranchNotMatched, 7);
    }
}
