# Build your first Pattrn index

This tutorial introduces the core `Pattrn` package. It uses explicit pattern segments because that is the current documentation-first model for the pre-beta core API.

Use companion packages only after the segmented core model is clear:

- use `Pattrn.Strings` when your input starts as separated strings;
- use `Pattrn.DependencyInjection` when the index should be built by Microsoft.Extensions.DependencyInjection;
- use `Pattrn.Routing` when you need preview route-template syntax.

## 1. Create a builder

```csharp
using Pattrn;

var builder = PattrnIndex<string, string>.Builder();
```

The default builder is tokenless. Calls to `Add(...)` register literal segments. Use `AddPattern(...)` when you need parameters, wildcards, or catch-alls.

## 2. Add explicit patterns

```csharp
builder
    .AddPattern(
        [
            PatternSegment<string>.Literal("market"),
            PatternSegment<string>.Literal("NASDAQ"),
            PatternSegment<string>.Literal("MSFT")
        ],
        "exact-msft",
        patternId: "market-nasdaq-msft")
    .AddPattern(
        [
            PatternSegment<string>.Literal("market"),
            PatternSegment<string>.Literal("NASDAQ"),
            PatternSegment<string>.Wildcard()
        ],
        "any-nasdaq",
        patternId: "market-nasdaq-any");
```

A literal segment must match exactly. A wildcard segment matches one segment. The core does not parse route templates, filesystem globs, URLs, or dotted strings.

## 3. Build the immutable index

```csharp
var index = builder.Build();
```

The builder is mutable. The built index is immutable and safe for concurrent readers.

## 4. Match segmented input

```csharp
var matches = index.MatchToArray(["market", "NASDAQ", "MSFT"]);
```

The result contains the values from matching registrations. Use the reference docs for the full matching, duplicate, and ranking contracts.

## 5. Use detailed matches when you need metadata

```csharp
var detailed = index.MatchDetailedToArray(["market", "NASDAQ", "MSFT"]);

foreach (var match in detailed)
{
    Console.WriteLine(match.PatternId);
    Console.WriteLine(match.RegistrationOrder);
    Console.WriteLine(match.Specificity);
}
```

Detailed matches expose pattern identity, registration order, specificity metadata, and captures.

## 6. Next steps

- Read the [core API reference](../reference/api.md).
- Read [generic pattern segments](../reference/pattern-segments.md).
- Read [matching semantics](../reference/matching-semantics.md).
- Read [ranking and specificity](../reference/ranking-specificity.md).
- Use [string helpers](../packages/pattrn-strings.md) for separated string paths.
- Use [dependency injection](../packages/pattrn-dependency-injection.md) for service registration.
- Use [routing](../packages/pattrn-routing.md) only when preview route-template syntax is appropriate.
