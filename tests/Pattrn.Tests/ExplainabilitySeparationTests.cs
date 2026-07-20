using TUnit.Assertions.Enums;

namespace Pattrn.Tests;

public sealed class ExplainabilitySeparationTests
{
    [Test]
    public async Task ExplainReturnsAcceptedDetailedMatchesWithoutChangingHotPathResults()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .AddPattern(
                [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")],
                "handler",
                name: "orders-by-id")
            .Build();

        var hotMatches = index.MatchValuesToArray(["orders", "123"]);
        var explanation = index.Explain(["orders", "123"]);

        await Assert.That(hotMatches).IsEquivalentTo(["handler"], CollectionOrdering.Matching);
        await Assert.That(explanation.HasMatches).IsTrue().Because("Explanation should report accepted matches.");
        await Assert.That(explanation.MatchCount).IsEqualTo(1);
        await Assert.That(explanation.Matches[0].Value).IsEqualTo("handler");
        await Assert.That(explanation.Matches[0].Captures[0]).IsEqualTo(new PatternCapture<string>("id", "123", 1));
        await Assert.That(explanation.Path).IsEquivalentTo(["orders", "123"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task ExplainDoesNotCollectRejectedCandidatesByDefault()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .Add(["orders", "new"], "handler")
            .Build();

        var explanation = index.Explain(["customers", "42"]);

        await Assert.That(explanation.HasMatches).IsFalse().Because("The path should not match.");
        await Assert.That(explanation.RejectedCandidates.Count).IsEqualTo(0);
        await Assert.That(explanation.ExplanationOptions.IncludeRejectedCandidates).IsFalse().Because("Rejected-candidate diagnostics should be opt-in.");
    }

    [Test]
    public async Task ExplainCanCollectRejectedCandidatesWhenRequested()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .Add(["orders", "new"], "handler")
            .Build();

        var explanation = index.Explain(["customers", "42"], PatternExplanationOptions.IncludeRejections);

        await Assert.That(explanation.HasMatches).IsFalse().Because("The path should not match.");
        await Assert.That(explanation.ExplanationOptions.IncludeRejectedCandidates).IsTrue().Because("Explanation should record the requested diagnostic option.");
        await Assert.That(explanation.RejectedCandidates.Count).IsEqualTo(1);
        await Assert.That(explanation.RejectedCandidates[0].PathDepth).IsEqualTo(0);
        await Assert.That(explanation.RejectedCandidates[0].ReasonKind).IsEqualTo(PatternRejectedCandidateReasonKind.LiteralMismatch);
        await Assert.That(string.IsNullOrWhiteSpace(explanation.RejectedCandidates[0].Reason)).IsFalse().Because("Human-readable reason text should remain populated.");
    }

    [Test]
    public async Task ExplainReportsEndOfInputRejectionWhenPathStopsBeforeTerminalRegistration()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .Add(["orders", "new"], "handler")
            .Build();

        var explanation = index.Explain(["orders"], new PatternExplanationOptions(includeRejectedCandidates: true));

        await Assert.That(explanation.HasMatches).IsFalse().Because("The shorter path should not match in exact mode.");
        await Assert.That(explanation.RejectedCandidates.Count).IsEqualTo(1);
        await Assert.That(explanation.RejectedCandidates[0].PathDepth).IsEqualTo(1);
        await Assert.That(explanation.RejectedCandidates[0].ReasonKind).IsEqualTo(PatternRejectedCandidateReasonKind.PathTooShort);
        await Assert.That(string.IsNullOrWhiteSpace(explanation.RejectedCandidates[0].Reason)).IsFalse().Because("Human-readable reason text should remain populated.");
    }

    [Test]
    public async Task ExplainReportsBranchMismatchReasonKindForWildcardIndexes()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Wildcard()], "handler")
            .Build();

        var explanation = index.Explain(["customers", "42"], PatternExplanationOptions.IncludeRejections);

        await Assert.That(explanation.HasMatches).IsFalse().Because("The path should not match.");
        await Assert.That(explanation.RejectedCandidates.Count).IsEqualTo(1);
        await Assert.That(explanation.RejectedCandidates[0].PathDepth).IsEqualTo(0);
        await Assert.That(explanation.RejectedCandidates[0].ReasonKind).IsEqualTo(PatternRejectedCandidateReasonKind.BranchNotMatched);
        await Assert.That(string.IsNullOrWhiteSpace(explanation.RejectedCandidates[0].Reason)).IsFalse().Because("Human-readable reason text should remain populated.");
    }

    [Test]
    public async Task RejectedCandidateReasonKindNumericValuesAreStable()
    {
        var none = (int)PatternRejectedCandidateReasonKind.None;
        var literalMismatch = (int)PatternRejectedCandidateReasonKind.LiteralMismatch;
        var pathTooShort = (int)PatternRejectedCandidateReasonKind.PathTooShort;
        var branchNotMatched = (int)PatternRejectedCandidateReasonKind.BranchNotMatched;

        await Assert.That(none).IsEqualTo(0);
        await Assert.That(literalMismatch).IsEqualTo(1);
        await Assert.That(pathTooShort).IsEqualTo(5);
        await Assert.That(branchNotMatched).IsEqualTo(7);
    }
}
