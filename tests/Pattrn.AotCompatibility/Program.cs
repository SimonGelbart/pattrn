using Microsoft.Extensions.DependencyInjection;
using Pattrn;
using Pattrn.DependencyInjection;

RunCoreSmoke();
RunStringsSmoke();
RunDependencyInjectionSmoke();

Console.WriteLine("Pattrn AOT compatibility smoke checks passed.");
return 0;

static void RunCoreSmoke()
{
    var index = PattrnIndex<string, string>
        .Builder()
        .AddPattern([Literal("market"), Literal("NASDAQ"), Literal("MSFT")], "literal", "literal")
        .AddPattern([Literal("orders"), Parameter("id")], "parameter", "parameter")
        .AddPattern([Literal("market"), Literal("NASDAQ"), Wildcard()], "wildcard", "wildcard")
        .AddPattern([Literal("files"), CatchAll("path")], "catch-all", "catch-all")
        .Build();

    RequireContains(index.MatchToArray(["market", "NASDAQ", "MSFT"]), "literal", "literal match");

    var parameterMatch = index.MatchDetailedToArray(["orders", "42"]).Single(match => match.Value == "parameter");
    Require(parameterMatch.Captures.Single(capture => capture.Name == "id").Value == "42", "parameter capture");

    RequireContains(index.MatchToArray(["market", "NASDAQ", "AAPL"]), "wildcard", "wildcard match");

    var catchAllMatch = index.MatchDetailedToArray(["files", "a", "b", "c.txt"]).Single(match => match.Value == "catch-all");
    Require(catchAllMatch.Captures.Count == 3, "catch-all capture count");

    var explanation = index.Explain(["orders", "42"]);
    Require(explanation.Matches.Any(match => match.Value == "parameter"), "explain accepted match");
}

static void RunStringsSmoke()
{
    var defaultIndex = StringPattrnIndexBuilder
        .CreateTokenized<string>('.', "*")
        .Add("market.NASDAQ.*", "string-wildcard")
        .Build();

    RequireContains(defaultIndex.MatchToArray("market.NASDAQ.MSFT"), "string-wildcard", "string tokenized match");

    var options = new StringNormalizationOptions('/')
    {
        CaseSensitivity = StringCaseSensitivity.OrdinalIgnoreCase,
        EmptySegmentBehavior = StringEmptySegmentBehavior.Ignore,
        TrimBehavior = StringSegmentTrimBehavior.TrimWhitespace
    };

    var normalizedIndex = options
        .CreateStringBuilder<string>()
        .AddPattern([Literal("api"), Parameter("resource")], "normalized", "normalized")
        .Build();

    RequireContains(normalizedIndex.MatchToArray("// API / Users /"), "normalized", "normalized string match");
}

static void RunDependencyInjectionSmoke()
{
    var services = new ServiceCollection();
    services.AddPattrnIndex<string, string>(registration => registration
        .Configure(builder => builder.AddPattern([Literal("di"), Parameter("name")], "default-di", "default-di")));
    services.AddPattrnIndex<string, string>("named", registration => registration
        .Configure(builder => builder.AddPattern([Literal("named"), Wildcard()], "named-di", "named-di")));

    using var provider = services.BuildServiceProvider(validateScopes: true);

    var defaultIndex = provider.GetRequiredService<IPattrnIndex<string, string>>();
    RequireContains(defaultIndex.MatchToArray(["di", "service"]), "default-di", "default DI match");

    var namedIndex = provider.GetRequiredKeyedService<IPattrnIndex<string, string>>("named");
    RequireContains(namedIndex.MatchToArray(["named", "service"]), "named-di", "keyed DI match");

    var pattrnProvider = provider.GetRequiredService<IPattrnProvider<string, string>>();
    RequireContains(pattrnProvider.GetRequired("named").MatchToArray(["named", "provider"]), "named-di", "provider DI match");
}

static PatternSegment<string> Literal(string value) => PatternSegment<string>.Literal(value);
static PatternSegment<string> Parameter(string name) => PatternSegment<string>.Parameter(name);
static PatternSegment<string> Wildcard() => PatternSegment<string>.Wildcard();
static PatternSegment<string> CatchAll(string name) => PatternSegment<string>.CatchAll(name);

static void RequireContains(IReadOnlyCollection<string> values, string expected, string scenario)
{
    Require(values.Contains(expected, StringComparer.Ordinal), scenario);
}

static void Require(bool condition, string scenario)
{
    if (!condition)
    {
        throw new InvalidOperationException($"AOT compatibility smoke check failed: {scenario}.");
    }
}
