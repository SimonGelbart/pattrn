using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class ApiTests
{
    [Test]
    public void StaticBuilderFactoryCreatesConfiguredBuilder()
    {
        var builder = PattrnIndex<string, string>.Builder(
            wildcardSegment: "*",
            segmentComparer: StringComparer.OrdinalIgnoreCase,
            valueComparer: StringComparer.OrdinalIgnoreCase);

        ShouldEqual(builder.WildcardSegment, "*");
        ShouldEqual(builder.SegmentComparer, StringComparer.OrdinalIgnoreCase);
        ShouldEqual(builder.ValueComparer, StringComparer.OrdinalIgnoreCase);

        builder.Add(["Market", "NASDAQ", "*"], "Client-A");
        builder.Add(["market", "nasdaq", "MSFT"], "client-a");

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(["market", "nasdaq", "msft"]), ["client-a"]);
    }

    [Test]
    public void DirectBuilderFactoryRejectsNullWildcardSegment()
    {
        var exception = ShouldThrow<ArgumentNullException>(
            () => PattrnIndexBuilder<string, string>.Create((string)null!));

        ShouldEqual(exception.ParamName, "wildcardSegment");
    }

    [Test]
    public void StaticBuilderFactoryRejectsNullWildcardSegment()
    {
        var exception = ShouldThrow<ArgumentNullException>(
            () => PattrnIndex<string, string>.Builder((string)null!));

        ShouldEqual(exception.ParamName, "wildcardSegment");
    }

    [Test]
    public void MemoryMatchOverloadWritesToDestination()
    {
        var builder = PattrnIndex<string, int>.Builder("*");
        builder.Add(["market", "NASDAQ", "MSFT"], 1);
        builder.Add(["market", "NASDAQ", "*"], 2);
        var index = builder.Build();
        ReadOnlyMemory<string> path = new[] { "market", "NASDAQ", "MSFT" };
        Span<int> destination = stackalloc int[2];

        var written = index.Match(path, destination);

        ShouldEqual(written, 2);
        ShouldSetEqual(destination[..written].ToArray(), [1, 2]);
    }

    [Test]
    public void MatchOptionsDefaultEqualsExplicitDefaults()
    {
        var defaultOptions = default(MatchOptions);
        var explicitOptions = MatchOptions.Default;

        ShouldBeTrue(defaultOptions == explicitOptions, "Default options should equal explicit defaults.");
        ShouldEqual(defaultOptions.GetHashCode(), explicitOptions.GetHashCode());
        defaultOptions.Deconstruct(out var prefixMatchMode, out var duplicateValueMatchMode);
        ShouldEqual(prefixMatchMode, PrefixMatchMode.Disabled);
        ShouldEqual(duplicateValueMatchMode, DuplicateValueMatchMode.Deduplicate);
        ShouldBeFalse(defaultOptions.IncludePrefixMatches, "Prefix matching should be disabled by default.");
        ShouldBeTrue(defaultOptions.DeduplicateValues, "Deduplication should be enabled by default.");
    }

    [Test]
    public void MatchOptionsInequalityDistinguishesConfiguredValues()
    {
        var defaultOptions = default(MatchOptions);
        var prefixOptions = MatchOptions.Prefix;
        var noDedupOptions = MatchOptions.PreserveDuplicates;

        ShouldBeTrue(defaultOptions != prefixOptions, "Prefix option should affect equality.");
        ShouldBeTrue(defaultOptions != noDedupOptions, "Deduplication option should affect equality.");
    }
}

public sealed class MatchOptionsPolishTests
{
    [Test]
    public void StaticDefaultOptionsEqualDefaultValue()
    {
        ShouldBeTrue(MatchOptions.Default == default(MatchOptions), "Static Default should equal the default struct value.");
    }

    [Test]
    public void StaticPrefixOptionsEnablePrefixMatchingAndKeepDeduplication()
    {
        ShouldEqual(MatchOptions.Prefix.PrefixMatchMode, PrefixMatchMode.IncludePrefixPatterns);
        ShouldEqual(MatchOptions.Prefix.DuplicateValueMatchMode, DuplicateValueMatchMode.Deduplicate);
        ShouldBeTrue(MatchOptions.Prefix.IncludePrefixMatches, "Prefix options should enable prefix matching.");
        ShouldBeTrue(MatchOptions.Prefix.DeduplicateValues, "Prefix options should preserve default deduplication.");
    }

    [Test]
    public void StaticPreserveDuplicatesOptionsPreserveDuplicateRegistrations()
    {
        ShouldEqual(MatchOptions.PreserveDuplicates.PrefixMatchMode, PrefixMatchMode.Disabled);
        ShouldEqual(MatchOptions.PreserveDuplicates.DuplicateValueMatchMode, DuplicateValueMatchMode.PreserveDuplicates);
        ShouldBeFalse(MatchOptions.PreserveDuplicates.DeduplicateValues, "PreserveDuplicates should disable deduplication.");
    }

    [Test]
    public void MatchOptionsRejectUndefinedEnumValues()
    {
        ShouldThrow<ArgumentOutOfRangeException>(() => new MatchOptions((PrefixMatchMode)42));
        ShouldThrow<ArgumentOutOfRangeException>(() => new MatchOptions(duplicateValueMatchMode: (DuplicateValueMatchMode)42));
    }
}
