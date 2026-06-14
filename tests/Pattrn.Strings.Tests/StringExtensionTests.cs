using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Strings.Tests;

public sealed class StringExtensionTests
{
    [Test]
    public void DottedHelpersAddAndMatchStringPaths()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.AddDotted("market.NASDAQ.MSFT", "exact");
        builder.AddDotted("market.NASDAQ.*", "wildcard");

        var index = builder.Build();

        ShouldSetEqual(index.MatchDottedToArray("market.NASDAQ.MSFT"), ["exact", "wildcard"]);
    }

    [Test]
    public void DottedHelpersSupportCustomSeparator()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.AddDotted("market/NASDAQ/*", "wildcard", separator: '/');

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchDottedToArray("market/NASDAQ/MSFT", separator: '/'), ["wildcard"]);
    }

    [Test]
    public void DottedRemoveRemovesOneRegistration()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.AddDotted("market.NASDAQ.*", "client");

        var removed = builder.RemoveDotted("market.NASDAQ.*", "client");

        ShouldBeTrue(removed, "Expected dotted registration to be removed.");
        ShouldEqual(builder.Build().MatchDottedToArray("market.NASDAQ.MSFT").Length, 0);
    }

    [Test]
    public void DottedMatchCanWriteToDestinationSpan()
    {
        var builder = PattrnIndexBuilder<string, int>.Create("*");
        builder.AddDotted("market.NASDAQ.MSFT", 1);
        builder.AddDotted("market.NASDAQ.*", 2);
        var index = builder.Build();
        Span<int> destination = stackalloc int[2];

        var written = index.MatchDotted("market.NASDAQ.MSFT", destination);

        ShouldEqual(written, 2);
        ShouldSetEqual(destination[..written].ToArray(), [1, 2]);
    }
}

public sealed class StringExtensionValidationTests
{
    [Test]
    public void DottedHelpersRejectEmptyPathBecauseCoreSpanApisRepresentEmptyPathsExplicitly()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        var exception = ShouldThrow<ArgumentException>(() => builder.AddDotted("", "value"));

        ShouldEqual(exception.ParamName, "pattern");
    }

    [Test]
    public void DottedHelpersRejectEmptySegments()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        var exception = ShouldThrow<ArgumentException>(() => builder.AddDotted("market..MSFT", "value"));

        ShouldEqual(exception.ParamName, "pattern");
    }
}

public sealed class SeparatedStringExtensionTests
{
    [Test]
    public void AddSeparatedSupportsFluentChains()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        var returned = builder
            .AddSeparated("market/NASDAQ/MSFT", "exact", '/')
            .AddSeparated("market/NASDAQ/*", "wildcard", '/');

        ShouldBeTrue(ReferenceEquals(builder, returned), "Expected separated string add helper to return the same builder.");
        ShouldSetEqual(builder.Build().MatchSeparatedToArray("market/NASDAQ/MSFT", '/'), ["exact", "wildcard"]);
    }

    [Test]
    public void DottedAddSupportsFluentChains()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        var returned = builder
            .AddDotted("market.NASDAQ.MSFT", "exact")
            .AddDotted("market.NASDAQ.*", "wildcard");

        ShouldBeTrue(ReferenceEquals(builder, returned), "Expected dotted add helper to return the same builder.");
        ShouldSetEqual(builder.Build().MatchDottedToArray("market.NASDAQ.MSFT"), ["exact", "wildcard"]);
    }

    [Test]
    public void SeparatedHelpersCanRemoveAndInspectPatterns()
    {
        var builder = PattrnIndex<string, string>.Builder("*")
            .AddSeparated("market/NASDAQ/MSFT", "client-1", '/')
            .AddSeparated("market/NASDAQ/MSFT", "client-2", '/');

        ShouldBeTrue(builder.ContainsSeparated("market/NASDAQ/MSFT", '/'), "Expected separated helper to find registered pattern.");
        ShouldEqual(builder.RemoveAllSeparated("market/NASDAQ/MSFT", '/'), 2);
        ShouldBeFalse(builder.ContainsSeparated("market/NASDAQ/MSFT", '/'), "Expected separated helper to observe removed pattern.");
    }

    [Test]
    public void TryMatchDottedReturnsFalseWhenDestinationIsTooSmall()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .AddDotted("market.NASDAQ.MSFT", 1)
            .AddDotted("market.NASDAQ.*", 2)
            .Build();
        Span<int> destination = stackalloc int[1];

        var succeeded = index.TryMatchDotted("market.NASDAQ.MSFT", destination, out var written);

        ShouldBeFalse(succeeded, "Expected dotted TryMatch to fail without throwing.");
        ShouldEqual(written, 0);
    }

    [Test]
    public void TryMatchSeparatedReturnsTrueWhenDestinationIsLargeEnough()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .AddSeparated("market/NASDAQ/MSFT", 1, '/')
            .Build();
        Span<int> destination = stackalloc int[1];

        var succeeded = index.TryMatchSeparated("market/NASDAQ/MSFT", destination, out var written, '/');

        ShouldBeTrue(succeeded, "Expected separated TryMatch to succeed.");
        ShouldEqual(written, 1);
        ShouldEqual(destination[0], 1);
    }
}

public sealed class StringIdentityTests
{
    [Test]
    public void SeparatedAddFlowsPatternIdentityToDetailedMatches()
    {
        var builder = PattrnIndexBuilder<string, string>.Create();
        builder.AddSeparated("config.feature.enabled", "value", '.', patternId: "feature-enabled");

        var match = builder.Build().MatchDetailedToArray(["config", "feature", "enabled"]).Single();

        ShouldEqual(match.PatternId, "feature-enabled");
        ShouldEqual(match.RegistrationOrder, 0);
    }
}

public sealed class StringNormalizationOptionsTests
{
    [Test]
    public void OptionsCanCreateCaseInsensitiveStringBuilder()
    {
        var options = new StringNormalizationOptions('/')
        {
            CaseSensitivity = StringCaseSensitivity.OrdinalIgnoreCase
        };
        var index = options.CreateBuilder<string>()
            .AddSeparated("API/Users", "value", options)
            .Build();

        ShouldSequenceEqual(index.MatchSeparatedToArray("api/users", options), ["value"]);
    }

    [Test]
    public void OptionsCanIgnoreEmptySegmentsTrimAndNormalizeSegments()
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

        ShouldSequenceEqual(index.MatchSeparatedToArray("//api//USERS/", options), ["value"]);
    }

    [Test]
    public void DefaultOptionsKeepRejectingEmptySegments()
    {
        var options = StringNormalizationOptions.Dotted;

        var exception = ShouldThrow<ArgumentException>(() => options.Split("market..MSFT", "pattern"));

        ShouldEqual(exception.ParamName, "pattern");
    }

    [Test]
    public void TokenizedBuilderFactoryUsesOptionsComparer()
    {
        var options = new StringNormalizationOptions('.')
        {
            CaseSensitivity = StringCaseSensitivity.OrdinalIgnoreCase
        };
        var index = options.CreateTokenizedBuilder<string>("*")
            .AddSeparated("market.NASDAQ.*", "value", options)
            .Build();

        ShouldSequenceEqual(index.MatchSeparatedToArray("MARKET.nasdaq.msft", options), ["value"]);
    }
}

public sealed class StringPattrnIndexBuilderFacadeTests
{
    [Test]
    public void FacadeBuildsAndMatchesSlashSeparatedPathsWithoutRepeatingOptions()
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
            .Add("/ API / Users /", "users", patternId: "users")
            .Build();

        ShouldSequenceEqual(index.MatchToArray("//api//USERS/"), ["users"]);
        var detailed = index.MatchDetailedToArray("api/users").Single();
        ShouldEqual(detailed.PatternId, "users");
    }

    [Test]
    public void FacadeSupportsExplicitPatternSegmentsWithoutWildcardTokens()
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
                patternId: "market-handler")
            .Build();

        var match = index.MatchDetailedToArray("market.NASDAQ.MSFT.QUOTE").Single();

        ShouldEqual(match.Value, "handler");
        ShouldEqual(match.PatternId, "market-handler");
        ShouldSequenceEqual(match.Captures.Select(capture => capture.Name).ToArray(), ["exchange", "symbol", "symbol"]);
        ShouldSequenceEqual(match.Captures.Select(capture => capture.Value).ToArray(), ["NASDAQ", "MSFT", "QUOTE"]);
    }

    [Test]
    public void FacadeTokenizedBuilderKeepsWildcardConvenienceOptIn()
    {
        var index = StringPattrnIndexBuilder
            .CreateTokenized<string>('.', "*")
            .Add("market.NASDAQ.*", "wildcard")
            .Build();

        ShouldSequenceEqual(index.MatchToArray("market.NASDAQ.MSFT"), ["wildcard"]);
    }

    [Test]
    public void OptionsCanCreateStringBuilderFacade()
    {
        var options = new StringNormalizationOptions('.')
        {
            CaseSensitivity = StringCaseSensitivity.OrdinalIgnoreCase
        };
        var index = options.CreateStringBuilder<string>()
            .Add("Market.NASDAQ.MSFT", "value")
            .Build();

        ShouldSequenceEqual(index.MatchToArray("market.nasdaq.msft"), ["value"]);
    }

    [Test]
    public void FacadeForwardsMutationHelpersToCoreBuilder()
    {
        var builder = StringPattrnIndexBuilder
            .CreateSlash<string>()
            .Add("market/NASDAQ/MSFT", "one")
            .Add("market/NASDAQ/MSFT", "two");

        ShouldBeTrue(builder.Contains("market/NASDAQ/MSFT"), "Expected facade to find normalized registration.");
        ShouldBeTrue(builder.Remove("market/NASDAQ/MSFT", "one"), "Expected facade to remove one registration.");
        ShouldEqual(builder.RemoveAll("market/NASDAQ/MSFT"), 1);
        ShouldBeFalse(builder.Contains("market/NASDAQ/MSFT"), "Expected facade to observe removed registration.");
    }
}
