using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class StableMatchingContractTests
{
    [Test]
    public void DetailedMatchesExposePatternIdentityAndRegistrationOrder()
    {
        var builder = PattrnIndex<string, string>.Builder();
        builder.Add(["orders", "new"], "literal", patternId: "orders-new");
        builder.AddPattern(
            [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")],
            "parameter",
            patternId: "orders-by-id");

        var index = builder.Build(MatchOptions.PreserveDuplicates);
        var matches = new PatternMatch<string>[index.GetMatchCountUpperBound(["orders", "new"] )];
        var captures = new PatternCapture<string>[index.GetCaptureCountUpperBound(["orders", "new"] )];

        var matchCount = index.MatchDetailed(["orders", "new"], matches, captures, out var captureCount);

        ShouldEqual(matchCount, 2);
        ShouldEqual(captureCount, 1);

        var literal = matches.Single(match => match.Value == "literal");
        ShouldEqual(literal.PatternId, "orders-new");
        ShouldEqual(literal.RegistrationOrder, 0);
        ShouldEqual(literal.Kind, PatternMatchKind.Exact);

        var parameter = matches.Single(match => match.Value == "parameter");
        ShouldEqual(parameter.PatternId, "orders-by-id");
        ShouldEqual(parameter.RegistrationOrder, 1);
        ShouldEqual(parameter.Kind, PatternMatchKind.Parameter);
        ShouldEqual(captures[parameter.CaptureStart], new PatternCapture<string>("id", "new", 1));
    }

    [Test]
    public void DetailedArrayResultsExposePatternIdentityAndRegistrationOrder()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .AddPattern(
                [PatternSegment<string>.Literal("customers"), PatternSegment<string>.Parameter("customerId")],
                "handler",
                patternId: "customers-by-id")
            .Build();

        var result = index.MatchDetailedToArray(["customers", "42"]).Single();

        ShouldEqual(result.Value, "handler");
        ShouldEqual(result.PatternId, "customers-by-id");
        ShouldEqual(result.RegistrationOrder, 0);
        ShouldEqual(result.Captures[0], new PatternCapture<string>("customerId", "42", 1));
    }

    [Test]
    public void DuplicateRegistrationPoliciesAssignOrderOnlyToAcceptedRegistrations()
    {
        var ignored = PattrnIndex<string, string>
            .Builder()
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Ignore)
            .Add(["orders", "new"], "first", patternId: "first")
            .Add(["orders", "new"], "second", patternId: "ignored")
            .Build(MatchOptions.PreserveDuplicates)
            .MatchDetailedToArray(["orders", "new"])
            .Single();

        ShouldEqual(ignored.Value, "first");
        ShouldEqual(ignored.PatternId, "first");
        ShouldEqual(ignored.RegistrationOrder, 0);

        var replaced = PattrnIndex<string, string>
            .Builder()
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Replace)
            .Add(["orders", "new"], "first", patternId: "first")
            .Add(["orders", "new"], "second", patternId: "replacement")
            .Build(MatchOptions.PreserveDuplicates)
            .MatchDetailedToArray(["orders", "new"])
            .Single();

        ShouldEqual(replaced.Value, "second");
        ShouldEqual(replaced.PatternId, "replacement");
        ShouldEqual(replaced.RegistrationOrder, 1);
    }

    [Test]
    public void IdentifiedRangeRegistrationsFlowToDetailedMatches()
    {
        var builder = PattrnIndex<string, string>.Builder();
        builder.AddPatternRange(
        [
            (new[] { PatternSegment<string>.Literal("topics"), PatternSegment<string>.Parameter("name") }.AsEnumerable(), "topic-handler", "topics-by-name")
        ]);

        var match = builder.Build().MatchDetailedToArray(["topics", "alpha"]).Single();

        ShouldEqual(match.Value, "topic-handler");
        ShouldEqual(match.PatternId, "topics-by-name");
        ShouldEqual(match.RegistrationOrder, 0);
    }
}
