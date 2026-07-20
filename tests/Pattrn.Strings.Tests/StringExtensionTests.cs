using TUnit.Assertions.Enums;

namespace Pattrn.Strings.Tests;

public sealed class StringExtensionTests
{
    [Test]
    public async Task DottedHelpersAddAndMatchStringPaths()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.AddDotted("market.NASDAQ.MSFT", "exact");
        builder.AddDotted("market.NASDAQ.*", "wildcard");

        var index = builder.Build();

        await Assert.That(index.MatchDottedToArray("market.NASDAQ.MSFT")).IsEquivalentTo(["exact", "wildcard"]);
    }

    [Test]
    public async Task DottedHelpersSupportCustomSeparator()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.AddDotted("market/NASDAQ/*", "wildcard", separator: '/');

        var index = builder.Build();

        await Assert.That(index.MatchDottedToArray("market/NASDAQ/MSFT", separator: '/')).IsEquivalentTo(["wildcard"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task DottedRemoveRemovesOneRegistration()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.AddDotted("market.NASDAQ.*", "client");

        var removed = builder.RemoveDotted("market.NASDAQ.*", "client");

        await Assert.That(removed).IsTrue().Because("Expected dotted registration to be removed.");
        await Assert.That(builder.Build().MatchDottedToArray("market.NASDAQ.MSFT").Length).IsEqualTo(0);
    }

    [Test]
    public async Task DottedMatchCanWriteToDestinationSpan()
    {
        var builder = PattrnIndexBuilder<string, int>.Create("*");
        builder.AddDotted("market.NASDAQ.MSFT", 1);
        builder.AddDotted("market.NASDAQ.*", 2);
        var index = builder.Build();
        Span<int> destination = stackalloc int[2];

        var written = index.MatchDotted("market.NASDAQ.MSFT", destination);
        var matchedValues = destination[..written].ToArray();

        await Assert.That(written).IsEqualTo(2);
        await Assert.That(matchedValues).IsEquivalentTo([1, 2]);
    }

    [Test]
    public async Task CaseInsensitiveFacadeContainsAndRemovesCanonicalRegistration()
    {
        var options = new StringNormalizationOptions('/')
        {
            CaseSensitivity = StringCaseSensitivity.OrdinalIgnoreCase
        };
        var builder = StringPattrnIndexBuilder.Create<string>(options)
            .Add("Market/NASDAQ", "handler");

        await Assert.That(builder.Contains("market/nasdaq")).IsTrue().Because("Expected comparer-aware containment.");
        await Assert.That(builder.Remove("market/nasdaq", "handler")).IsTrue().Because("Expected comparer-aware removal.");
        await Assert.That(builder.Contains("market/nasdaq")).IsFalse().Because("Expected registration to be removed.");
    }
}

public sealed class StringExtensionValidationTests
{
    [Test]
    public async Task DottedHelpersRejectEmptyPathBecauseCoreSpanApisRepresentEmptyPathsExplicitly()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        await Assert.That(() => builder.AddDotted("", "value"))
            .Throws<ArgumentException>()
            .WithParameterName("pattern");
    }

    [Test]
    public async Task DottedHelpersRejectEmptySegments()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        await Assert.That(() => builder.AddDotted("market..MSFT", "value"))
            .Throws<ArgumentException>()
            .WithParameterName("pattern");
    }
}

public sealed class SeparatedStringExtensionTests
{
    [Test]
    public async Task AddSeparatedSupportsFluentChains()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        var returned = builder
            .AddSeparated("market/NASDAQ/MSFT", "exact", '/')
            .AddSeparated("market/NASDAQ/*", "wildcard", '/');

        await Assert.That(ReferenceEquals(builder, returned)).IsTrue().Because("Expected separated string add helper to return the same builder.");
        await Assert.That(builder.Build().MatchSeparatedToArray("market/NASDAQ/MSFT", '/')).IsEquivalentTo(["exact", "wildcard"]);
    }

    [Test]
    public async Task DottedAddSupportsFluentChains()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        var returned = builder
            .AddDotted("market.NASDAQ.MSFT", "exact")
            .AddDotted("market.NASDAQ.*", "wildcard");

        await Assert.That(ReferenceEquals(builder, returned)).IsTrue().Because("Expected dotted add helper to return the same builder.");
        await Assert.That(builder.Build().MatchDottedToArray("market.NASDAQ.MSFT")).IsEquivalentTo(["exact", "wildcard"]);
    }

    [Test]
    public async Task SeparatedHelpersCanRemoveAndInspectPatterns()
    {
        var builder = PattrnIndex<string, string>.Builder("*")
            .AddSeparated("market/NASDAQ/MSFT", "client-1", '/')
            .AddSeparated("market/NASDAQ/MSFT", "client-2", '/');

        await Assert.That(builder.ContainsSeparated("market/NASDAQ/MSFT", '/')).IsTrue().Because("Expected separated helper to find registered pattern.");
        await Assert.That(builder.RemoveAllSeparated("market/NASDAQ/MSFT", '/')).IsEqualTo(2);
        await Assert.That(builder.ContainsSeparated("market/NASDAQ/MSFT", '/')).IsFalse().Because("Expected separated helper to observe removed pattern.");
    }

    [Test]
    public async Task TryMatchDottedReturnsFalseWhenDestinationIsTooSmall()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .AddDotted("market.NASDAQ.MSFT", 1)
            .AddDotted("market.NASDAQ.*", 2)
            .Build();
        Span<int> destination = stackalloc int[1];

        var succeeded = index.TryMatchDotted("market.NASDAQ.MSFT", destination, out var written);

        await Assert.That(succeeded).IsFalse().Because("Expected dotted TryMatch to fail without throwing.");
        await Assert.That(written).IsEqualTo(0);
    }

    [Test]
    public async Task TryMatchSeparatedReturnsTrueWhenDestinationIsLargeEnough()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .AddSeparated("market/NASDAQ/MSFT", 1, '/')
            .Build();
        Span<int> destination = stackalloc int[1];

        var succeeded = index.TryMatchSeparated("market/NASDAQ/MSFT", destination, out var written, '/');
        var matchedValue = destination[0];

        await Assert.That(succeeded).IsTrue().Because("Expected separated TryMatch to succeed.");
        await Assert.That(written).IsEqualTo(1);
        await Assert.That(matchedValue).IsEqualTo(1);
    }
}

public sealed class StringIdentityTests
{
    [Test]
    public void SeparatedAddFlowsPatternIdentityToDetailedMatches()
    {
        var builder = PattrnIndexBuilder<string, string>.Create();
        builder.AddSeparated("config.feature.enabled", "value", '.', name: "feature-enabled");

        var match = builder.Build().MatchDetailedToArray(["config", "feature", "enabled"]).Single();

    }
}

public sealed class StringNormalizationOptionsTests
{
    [Test]
    public async Task OptionsCanCreateCaseInsensitiveStringBuilder()
    {
        var options = new StringNormalizationOptions('/')
        {
            CaseSensitivity = StringCaseSensitivity.OrdinalIgnoreCase
        };
        var index = options.CreateBuilder<string>()
            .AddSeparated("API/Users", "value", options)
            .Build();

        await Assert.That(index.MatchSeparatedToArray("api/users", options)).IsEquivalentTo(["value"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task OptionsCanIgnoreEmptySegmentsTrimAndNormalizeSegments()
    {
        var options = new StringNormalizationOptions('/')
        {
            EmptySegmentBehavior = StringEmptySegmentBehavior.Ignore,
            TrimBehavior = StringSegmentTrimBehavior.TrimWhitespace,
            NormalizeSegment = static segment => segment.ToLowerInvariant()
        };
        var index = options.CreateBuilder<string>()
            .AddSeparated("/ API / Users /", "value", options)
            .Build();

        await Assert.That(index.MatchSeparatedToArray("//api//USERS/", options)).IsEquivalentTo(["value"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task DefaultOptionsKeepRejectingEmptySegments()
    {
        var options = StringNormalizationOptions.Dotted;

        await Assert.That(() => options.Split("market..MSFT", "pattern"))
            .Throws<ArgumentException>()
            .WithParameterName("pattern");
    }

    [Test]
    public async Task TokenizedBuilderFactoryUsesOptionsComparer()
    {
        var options = new StringNormalizationOptions('.')
        {
            CaseSensitivity = StringCaseSensitivity.OrdinalIgnoreCase
        };
        var index = options.CreateTokenizedBuilder<string>("*")
            .AddSeparated("market.NASDAQ.*", "value", options)
            .Build();

        await Assert.That(index.MatchSeparatedToArray("MARKET.nasdaq.msft", options)).IsEquivalentTo(["value"], CollectionOrdering.Matching);
    }
}

public sealed class StringPattrnIndexBuilderFacadeTests
{
    [Test]
    public async Task FacadeBuildsAndMatchesSlashSeparatedPathsWithoutRepeatingOptions()
    {
        var options = new StringNormalizationOptions('/')
        {
            CaseSensitivity = StringCaseSensitivity.OrdinalIgnoreCase,
            EmptySegmentBehavior = StringEmptySegmentBehavior.Ignore,
            TrimBehavior = StringSegmentTrimBehavior.TrimWhitespace,
            NormalizeSegment = static segment => segment.ToLowerInvariant()
        };

        var index = StringPattrnIndexBuilder
            .Create<string>(options)
            .Add("/ API / Users /", "users", name: "users")
            .Build();

        await Assert.That(index.MatchValuesToArray("//api//USERS/")).IsEquivalentTo(["users"], CollectionOrdering.Matching);
        var detailed = index.MatchDetailedToArray("api/users").Single();
    }

    [Test]
    public async Task FacadeSupportsExplicitPatternSegmentsWithoutWildcardTokens()
    {
        var index = StringPattrnIndexBuilder
            .CreateDotted<string>()
            .AddPattern(
                [
                    PatternSegment<string>.Literal("market"),
                    PatternSegment<string>.Parameter("exchange"),
                    PatternSegment<string>.CatchAll("symbol")
                ],
                "handler",
                name: "market-handler")
            .Build();

        var match = index.MatchDetailedToArray("market.NASDAQ.MSFT.QUOTE").Single();

        await Assert.That(match.Value).IsEqualTo("handler");
        await Assert.That(match.Captures.Select(capture => capture.Name).ToArray()).IsEquivalentTo(["exchange", "symbol"], CollectionOrdering.Matching);
        await Assert.That(match.Captures[0].Values).IsEquivalentTo(["NASDAQ"], CollectionOrdering.Matching);
        await Assert.That(match.Captures[1].Values).IsEquivalentTo(["MSFT", "QUOTE"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task FacadeTokenizedBuilderKeepsWildcardConvenienceOptIn()
    {
        var index = StringPattrnIndexBuilder
            .CreateTokenized<string>('.', "*")
            .Add("market.NASDAQ.*", "wildcard")
            .Build();

        await Assert.That(index.MatchValuesToArray("market.NASDAQ.MSFT")).IsEquivalentTo(["wildcard"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task OptionsCanCreateStringBuilderFacade()
    {
        var options = new StringNormalizationOptions('.')
        {
            CaseSensitivity = StringCaseSensitivity.OrdinalIgnoreCase
        };
        var index = options.CreateStringBuilder<string>()
            .Add("Market.NASDAQ.MSFT", "value")
            .Build();

        await Assert.That(index.MatchValuesToArray("market.nasdaq.msft")).IsEquivalentTo(["value"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task FacadeForwardsMutationHelpersToCoreBuilder()
    {
        var builder = StringPattrnIndexBuilder
            .CreateSlash<string>()
            .Add("market/NASDAQ/MSFT", "one")
            .Add("market/NASDAQ/MSFT", "two");

        await Assert.That(builder.Contains("market/NASDAQ/MSFT")).IsTrue().Because("Expected facade to find normalized registration.");
        await Assert.That(builder.Remove("market/NASDAQ/MSFT", "one")).IsTrue().Because("Expected facade to remove one registration.");
        await Assert.That(builder.RemoveAll("market/NASDAQ/MSFT")).IsEqualTo(1);
        await Assert.That(builder.Contains("market/NASDAQ/MSFT")).IsFalse().Because("Expected facade to observe removed registration.");
    }
}
