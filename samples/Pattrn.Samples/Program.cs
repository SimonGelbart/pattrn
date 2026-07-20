using Pattrn;

var subscriptions = PattrnIndex<string, string>
    .Builder()
    .AddPattern(
        [
            PatternSegment<string>.Literal("market"),
            PatternSegment<string>.Literal("NASDAQ"),
            PatternSegment<string>.Literal("MSFT")
        ],
        "exact-msft")
    .AddPattern(
        [
            PatternSegment<string>.Literal("market"),
            PatternSegment<string>.Literal("NASDAQ"),
            PatternSegment<string>.Wildcard()
        ],
        "any-nasdaq")
    .AddPattern(
        [
            PatternSegment<string>.Literal("market"),
            PatternSegment<string>.Wildcard(),
            PatternSegment<string>.Literal("MSFT")
        ],
        "any-market-msft")
    .Build();

Console.WriteLine("Exact + wildcard matches:");
foreach (var match in subscriptions.MatchToArray(["market", "NASDAQ", "MSFT"]))
{
    Console.WriteLine($"- {match}");
}

var prefixSubscriptions = PattrnIndex<string, string>
    .Builder()
    .Add(["market", "NASDAQ"], "all-nasdaq")
    .Add(["market", "NASDAQ", "MSFT"], "exact-msft")
    .Build();

Console.WriteLine();
Console.WriteLine("Prefix matches:");
foreach (var match in prefixSubscriptions.MatchPrefixToArray(["market", "NASDAQ", "MSFT"]))
{
    Console.WriteLine($"- {match}");
}

var destination = new string[prefixSubscriptions.GetPrefixMatchCountUpperBound(["market", "NASDAQ", "MSFT"] )];
if (prefixSubscriptions.TryMatchPrefix(["market", "NASDAQ", "MSFT"], destination, out var written))
{
    Console.WriteLine();
    Console.WriteLine($"Span-based matching wrote {written} value(s).");
}
