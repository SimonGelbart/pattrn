# Pattrn.Strings

`Pattrn.Strings` contains convenience helpers for separated string paths such as dotted topics, configuration keys, command paths, or slash-separated names.

The core package works with already-segmented paths, for example `ReadOnlySpan<string>`. This companion package converts strings into segments before delegating to the core index. The conversion policy is explicit so URL, filesystem, route, and application-specific semantics do not leak into the generic core.

## Install

Install both packages when you want string helpers. Use the current pre-beta package version from the package feed or repository package metadata:

```xml
<PackageReference Include="Pattrn" Version="..." />
<PackageReference Include="Pattrn.Strings" Version="..." />
```

## Basic usage

For new string-based code, prefer the string-path builder facade. It stores the normalization options once and returns a string-path wrapper, so callers do not repeat options on every registration and match:

```csharp
using Pattrn;

var index = StringPattrnIndexBuilder
    .CreateTokenized<string>('.', "*")
    .Add("market.NASDAQ.*", "client-a")
    .Build();

var matches = index.MatchToArray("market.NASDAQ.MSFT");
```

Slash-separated paths are the default, and explicit `StringNormalizationOptions` can be supplied when needed:

```csharp
var index = StringPattrnIndexBuilder
    .CreateSlash<string>()
    .Add("market/NASDAQ/MSFT", "client-a")
    .Build();

var matches = index.MatchToArray("market/NASDAQ/MSFT");
```

The older extension methods remain available when callers want to work directly with `PattrnIndexBuilder<string, TValue>` and `IPattrnIndex<string, TValue>`:

```csharp
var coreIndex = PattrnIndex<string, string>
    .Builder("*")
    .AddDotted("market.NASDAQ.*", "client-a")
    .Build();

var coreMatches = coreIndex.MatchDottedToArray("market.NASDAQ.MSFT");
```

## Explicit normalization options

Use `StringNormalizationOptions` when a domain needs explicit splitting and normalization behavior:

```csharp
var options = new StringNormalizationOptions('/')
{
    CaseSensitivity = StringCaseSensitivity.OrdinalIgnoreCase,
    EmptySegmentBehavior = StringEmptySegmentBehavior.Ignore,
    TrimBehavior = StringSegmentTrimBehavior.TrimWhitespace,
    NormalizeSegment = static segment => segment.ToLowerInvariant()
};

var index = options.CreateStringBuilder<string>()
    .Add("/ API / Users /", "users")
    .Build();

var matches = index.MatchToArray("//api//USERS/");
```

The options object can control:

- the separator character;
- ordinal case-sensitive or case-insensitive segment comparison for created builders;
- whether empty segments are rejected or ignored;
- whether segments are trimmed;
- an optional custom segment-normalization delegate.

The default char-based helpers and facade factories still reject empty segments and preserve segments exactly, matching earlier behavior.

## Explicit pattern segments from strings

The string facade also accepts explicit generic pattern segments and normalizes literal string segments before registration:

```csharp
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

var detailed = index.MatchDetailedToArray("market.NASDAQ.MSFT.QUOTE");
```

Use this path when wildcard, parameter, or catch-all intent should be explicit instead of encoded with a reserved token.

## Trimming and Native AOT

`Pattrn.Strings` is supported for trimming and Native AOT when validated with the repository AOT compatibility harness. The package uses explicit string splitting and normalization before delegating to the core matcher. Consuming applications remain responsible for validating their own custom normalization delegates and surrounding dependencies. See [trimming and Native AOT compatibility](../reference/aot-trimming.md) for validation scope, commands, warning policy, and limits.

## Boundary with the core package

`StringNormalizationOptions` lives in `Pattrn.Strings`. The core package remains generic and only sees already-split `string` segments plus the comparer configured on the builder.

This keeps URL decoding, filesystem separator rules, route semantics, glob syntax, and application-specific normalization outside the core.

## Performance note

These helpers allocate because they split strings into segment arrays. Use the core span-based APIs for hot paths.
