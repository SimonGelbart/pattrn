# Examples

These examples show common uses of the current pre-beta Pattrn surface.

## Topic routing

```csharp
var index = PattrnIndex<string, Action<string>>
    .Builder()
    .Add(["orders", "created"], message => Console.WriteLine($"created: {message}"))
    .AddPattern(
        [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Wildcard()],
        message => Console.WriteLine($"any order event: {message}"))
    .Build();

foreach (var handler in index.MatchToArray(["orders", "created"]))
{
    handler("order-123");
}
```

## String topic helpers

Install `Pattrn.Strings` for dotted or separated string helpers.

```csharp
var index = PattrnIndex<string, string>
    .Builder("*")
    .AddDotted("orders.created", "created-handler")
    .AddDotted("orders.*", "any-order-handler")
    .Build();

var handlers = index.MatchDottedToArray("orders.created");
```

## File-system-like paths

```csharp
var index = PattrnIndex<string, string>
    .Builder()
    .AddPattern(
        [PatternSegment<string>.Literal("home"), PatternSegment<string>.Wildcard(), PatternSegment<string>.Literal("downloads")],
        "download-cleanup-rule")
    .AddPattern(
        [PatternSegment<string>.Literal("var"), PatternSegment<string>.Literal("log"), PatternSegment<string>.Wildcard()],
        "log-rule")
    .Build();

var rules = index.MatchToArray(["home", "simon", "downloads"]);
```
