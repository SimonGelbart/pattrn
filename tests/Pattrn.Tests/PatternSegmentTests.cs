using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class PatternSegmentTests
{
    [Test]
    public void DefaultPatternSegmentIsWildcard()
    {
        var segment = default(PatternSegment<string>);

        ShouldEqual(segment.Kind, PatternSegmentKind.Wildcard);
        ShouldBeTrue(segment.IsWildcard, "Default pattern segment should be a wildcard.");
        ShouldBeFalse(segment.IsLiteral, "Default pattern segment should not be literal.");
        ShouldBeFalse(segment.IsParameter, "Default pattern segment should not be parameter.");
        ShouldBeFalse(segment.IsCatchAll, "Default pattern segment should not be catch-all.");
        ShouldEqual(segment.ParameterName, null);
    }

    [Test]
    public void LiteralSegmentStoresLiteralValue()
    {
        var segment = PatternSegment<string>.Literal("orders");

        ShouldEqual(segment.Kind, PatternSegmentKind.Literal);
        ShouldBeTrue(segment.IsLiteral, "Literal factory should create a literal segment.");
        ShouldEqual(segment.LiteralValue, "orders");
        ShouldEqual(segment.ParameterName, null);
    }

    [Test]
    public void ParameterSegmentStoresParameterName()
    {
        var segment = PatternSegment<string>.Parameter("id");

        ShouldEqual(segment.Kind, PatternSegmentKind.Parameter);
        ShouldBeTrue(segment.IsParameter, "Parameter factory should create a parameter segment.");
        ShouldEqual(segment.ParameterName, "id");
    }

    [Test]
    public void CatchAllSegmentStoresOptionalName()
    {
        var anonymous = PatternSegment<string>.CatchAll();
        var named = PatternSegment<string>.CatchAll("path");

        ShouldEqual(anonymous.Kind, PatternSegmentKind.CatchAll);
        ShouldBeTrue(anonymous.IsCatchAll, "CatchAll factory should create a catch-all segment.");
        ShouldEqual(anonymous.ParameterName, null);
        ShouldEqual(named.Kind, PatternSegmentKind.CatchAll);
        ShouldBeTrue(named.IsCatchAll, "Named CatchAll factory should create a catch-all segment.");
        ShouldEqual(named.ParameterName, "path");
    }

    [Test]
    public void WildcardSegmentHasNoLiteralValue()
    {
        var segment = PatternSegment<string>.Wildcard();

        ShouldEqual(segment.Kind, PatternSegmentKind.Wildcard);
        ShouldThrow<InvalidOperationException>(() => _ = segment.LiteralValue);
    }

    [Test]
    public void FactoriesRejectInvalidArguments()
    {
        ShouldThrow<ArgumentNullException>(() => PatternSegment<string>.Literal(null!));
        ShouldThrow<ArgumentNullException>(() => PatternSegment<string>.Parameter(null!));
        ShouldThrow<ArgumentException>(() => PatternSegment<string>.Parameter(""));
        ShouldThrow<ArgumentException>(() => PatternSegment<string>.Parameter("   "));
        ShouldThrow<ArgumentNullException>(() => PatternSegment<string>.CatchAll(null!));
        ShouldThrow<ArgumentException>(() => PatternSegment<string>.CatchAll(""));
        ShouldThrow<ArgumentException>(() => PatternSegment<string>.CatchAll("   "));
    }

    [Test]
    public void EqualityUsesKindAndAssociatedValue()
    {
        ShouldBeTrue(PatternSegment<string>.Literal("orders") == PatternSegment<string>.Literal("orders"), "Same literals should be equal.");
        ShouldBeFalse(PatternSegment<string>.Literal("orders") == PatternSegment<string>.Literal("customers"), "Different literals should not be equal.");
        ShouldBeTrue(PatternSegment<string>.Parameter("id") == PatternSegment<string>.Parameter("id"), "Same parameter names should be equal.");
        ShouldBeFalse(PatternSegment<string>.Parameter("id") == PatternSegment<string>.Parameter("name"), "Different parameter names should not be equal.");
        ShouldBeTrue(PatternSegment<string>.Wildcard() == default(PatternSegment<string>), "Anonymous wildcard should equal default wildcard.");
        ShouldBeTrue(PatternSegment<string>.CatchAll("path") == PatternSegment<string>.CatchAll("path"), "Same catch-all names should be equal.");
        ShouldBeFalse(PatternSegment<string>.CatchAll("path") == PatternSegment<string>.CatchAll("other"), "Different catch-all names should not be equal.");
    }
}

public sealed class PatternSegmentBuilderTests
{
    [Test]
    public void BuilderAcceptsGenericPatternSegments()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.AddPattern(
            [
                PatternSegment<string>.Literal("orders"),
                PatternSegment<string>.Parameter("id")
            ],
            "order-handler");

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(["orders", "123"]), ["order-handler"]);
        ShouldSequenceEqual(index.MatchToArray(["orders", "new"]), ["order-handler"]);
        ShouldSequenceEqual(index.MatchToArray(["customers", "123"]), []);
    }

    [Test]
    public void LiteralPatternSegmentCanRepresentConfiguredWildcardSegmentValue()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.AddPattern([PatternSegment<string>.Literal("*")], "literal-star");
        builder.AddPattern([PatternSegment<string>.Wildcard()], "wildcard");

        var index = builder.Build();

        ShouldSetEqual(index.MatchToArray(["*"]), ["literal-star", "wildcard"]);
        ShouldSequenceEqual(index.MatchToArray(["anything"]), ["wildcard"]);
    }

    [Test]
    public void ContainsAndRemoveSupportGenericPatternSegments()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        var pattern = new[]
        {
            PatternSegment<string>.Literal("customers"),
            PatternSegment<string>.Parameter("customerId"),
            PatternSegment<string>.Literal("orders")
        };

        builder.AddPattern(pattern, "value");

        ShouldBeTrue(builder.ContainsPattern(pattern), "Builder should contain the pattern-segment registration.");
        ShouldBeTrue(builder.RemovePattern(pattern, "value"), "Builder should remove the pattern-segment registration.");
        ShouldBeFalse(builder.ContainsPattern(pattern), "Builder should not contain the removed registration.");
    }

    [Test]
    public void RemoveAllSupportsGenericPatternSegments()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        var pattern = new[]
        {
            PatternSegment<string>.Literal("commands"),
            PatternSegment<string>.Wildcard()
        };

        builder.AddPattern(pattern, "a");
        builder.AddPattern(pattern, "b");

        var removed = builder.RemoveAllPattern(pattern);

        ShouldEqual(removed, 2);
        ShouldEqual(builder.RegistrationCount, 0);
        ShouldEqual(builder.PatternCount, 0);
    }

    [Test]
    public void AddRangeSupportsGenericPatternSegments()
    {
        var builder = PattrnIndex<string, int>.Builder("*");
        var registrations = new (IEnumerable<PatternSegment<string>> Pattern, int Value)[]
        {
            ([PatternSegment<string>.Literal("a")], 1),
            ([PatternSegment<string>.Literal("b"), PatternSegment<string>.Parameter("id")], 2)
        };

        builder.AddPatternRange(registrations);
        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(["a"]), [1]);
        ShouldSequenceEqual(index.MatchToArray(["b", "42"]), [2]);
    }
}
