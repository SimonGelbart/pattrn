# API overview

The core API is intentionally small and segmented-path-first.

## Build an index

The preferred core registration model is explicit `PatternSegment<TSegment>` values:

```csharp
var builder = PattrnIndex<string, string>.Builder();

builder
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
        "any-nasdaq");

var index = builder.Build();
```

The default builder is tokenless. `AddPattern(...)` callers do not need to reserve any segment value for wildcard behavior.

## Match to an array

```csharp
var matches = index.MatchToArray(["market", "NASDAQ", "MSFT"]);
```

## Match into a caller-provided buffer

```csharp
var path = new[] { "market", "NASDAQ", "MSFT" };
var destination = new string[index.GetMatchCountUpperBound(path)];
var written = index.Match(path, destination);
```

## Interface surface

`IPattrnIndex<TSegment, TValue>` keeps hot matching explicit and exposes explanation matching as a separate diagnostics-oriented operation:

```csharp
int GetMatchCountUpperBound(ReadOnlySpan<TSegment> path);
int Match(ReadOnlySpan<TSegment> path, Span<TValue> destination);
bool TryMatch(ReadOnlySpan<TSegment> path, Span<TValue> destination, out int written);
TValue[] MatchToArray(ReadOnlySpan<TSegment> path);

int GetCaptureCountUpperBound(ReadOnlySpan<TSegment> path);
int MatchDetailed(
    ReadOnlySpan<TSegment> path,
    Span<PatternMatch<TValue>> matches,
    Span<PatternCapture<TSegment>> captures,
    out int capturesWritten);
bool TryMatchDetailed(
    ReadOnlySpan<TSegment> path,
    Span<PatternMatch<TValue>> matches,
    Span<PatternCapture<TSegment>> captures,
    out int matchesWritten,
    out int capturesWritten);
PatternMatchResult<TSegment, TValue>[] MatchDetailed(ReadOnlySpan<TSegment> path);
PatternMatchResult<TSegment, TValue>[] MatchDetailedToArray(ReadOnlySpan<TSegment> path);

PatternMatchExplanation<TSegment, TValue> Explain(
    ReadOnlySpan<TSegment> path,
    PatternExplanationOptions options = default);
```

Convenience overloads for `ReadOnlyMemory<T>` and `IEnumerable<T>` are extension methods in the core package. Dotted and separated string helpers live in `Pattrn.Strings`.

## String normalization boundary

The core package does not split or normalize strings. It receives already-segmented paths and uses the builder's segment comparer.

`Pattrn.Strings` owns string conversion policy through `StringNormalizationOptions`. For new string-based code, the ergonomic facade stores those options once:

```csharp
var options = new StringNormalizationOptions('/')
{
    CaseSensitivity = StringCaseSensitivity.OrdinalIgnoreCase,
    EmptySegmentBehavior = StringEmptySegmentBehavior.Ignore,
    TrimBehavior = StringSegmentTrimBehavior.TrimWhitespace,
    NormalizeSegment = static segment => segment.ToLowerInvariant()
};

var index = options.CreateStringBuilder<string>()
    .Add("/ API / Users /", "users", patternId: "users")
    .Build();

var matches = index.MatchToArray("//api//USERS/");
```

The facade is convenience-oriented: `StringPattrnIndexBuilder<TValue>` wraps `PattrnIndexBuilder<string, TValue>`, and `StringPattrnIndex<TValue>` wraps `IPattrnIndex<string, TValue>`. Advanced callers can still use `CoreBuilder`, `CoreIndex`, or the older `AddSeparated` / `MatchSeparatedToArray` extension methods directly.

This boundary keeps URL decoding, filesystem rules, route semantics, glob syntax, and application-specific normalization out of the generic core and the generic string layer.


## Generic pattern segments

The builder accepts domain-neutral `PatternSegment<TSegment>` registrations through explicit `*Pattern` methods. These are the primary core registration APIs. The method names intentionally avoid overload ambiguity with empty collection expressions passed to the tokenized convenience `Add` API.

```csharp
var builder = PattrnIndex<string, string>.Builder();

builder.AddPattern(
    [
        PatternSegment<string>.Literal("orders"),
        PatternSegment<string>.Parameter("id")
    ],
    "order-handler",
    patternId: "orders-by-id");
```

Available methods:

```csharp
AddPattern(...);
AddPatternRange(...);
ContainsPattern(...);
RemovePattern(...);
RemoveAllPattern(...);
```

`PatternSegment<TSegment>.Literal(value)` always registers an exact literal value. `PatternSegment<TSegment>.Wildcard()` and `PatternSegment<TSegment>.Parameter(name)` both register a single-segment wildcard branch. Named parameters are exposed by the detailed match APIs. `Any()`, the `Wildcard` property, and the `CatchAllWildcard` property were removed before beta in favor of explicit factory methods.

See [generic pattern segments](pattern-segments.md).



## Tokenized convenience registration

`Add(ReadOnlySpan<TSegment>, TValue)` remains available. On a tokenless builder it registers literal-only segments. On a tokenized builder created with a wildcard segment, it registers compact tokenized patterns:

```csharp
var builder = PattrnIndex<string, string>.Builder("*");

builder
    .Add(["market", "NASDAQ", "MSFT"], "exact-msft")
    .Add(["market", "NASDAQ", "*"], "any-nasdaq");
```

Use tokenized builders only when reserving a wildcard token in the segment domain is acceptable. Use the default tokenless builder plus `AddPattern(...)` when a segment value such as `"*"` must be representable as a literal.

## Detailed match results

Use the detailed APIs when you need match metadata, pattern identity, registration order, or named-parameter captures. The low-allocation API writes match descriptors and captures into separate caller-provided spans:

```csharp
var path = new[] { "orders", "123" };
var matches = new PatternMatch<string>[index.GetMatchCountUpperBound(path)];
var captures = new PatternCapture<string>[index.GetCaptureCountUpperBound(path)];

var written = index.MatchDetailed(path, matches, captures, out var capturesWritten);

var first = matches[0];
var patternId = first.PatternId;
var registrationOrder = first.RegistrationOrder;
var firstCaptures = captures.AsSpan(first.CaptureStart, first.CaptureCount);
```

`MatchDetailed(...)` and `MatchDetailedToArray(...)` are convenience APIs that allocate per-match capture arrays. Keep latency-sensitive read paths on `Match` or `MatchDetailed` with caller-provided buffers.

Detailed matches expose `PatternId`, `RegistrationOrder`, `Kind`, `Specificity`, and capture slice metadata. `PatternId` is optional caller-provided identity; `RegistrationOrder` is a deterministic zero-based order assigned when the builder accepts the registration. See [compatibility semantics](compatibility-semantics.md) for the ordering contract currently covered by tests.

## Explanation results

Use `Explain(...)` for troubleshooting, logging, developer tools, and user-facing diagnostics. It intentionally allocates a `PatternMatchExplanation<TSegment, TValue>` object containing:

- a copy of the explained input path;
- accepted detailed matches;
- the index `MatchOptions`;
- the explanation options used;
- path-specific match and capture upper bounds;
- rejected-candidate hints when explicitly requested.

Rejected-candidate diagnostics are disabled by default:

```csharp
var explanation = index.Explain(["orders", "123"]);
```

Opt in only for troubleshooting paths where the extra traversal and allocation are acceptable:

```csharp
var explanation = index.Explain(
    ["customers", "42"],
    PatternExplanationOptions.IncludeRejections);
```

`Explain(...)` does not replace the hot APIs. Keep request-routing, policy checks, and repeated read paths on `Match`, `TryMatch`, `MatchToArray`, or span-based `MatchDetailed`.

## Builder duplicate and validation policies

The builder appends duplicate structural patterns by default:

```csharp
var builder = PattrnIndex<string, string>.Builder();
```

Use an explicit registration-time policy when duplicates should be rejected or collapsed:

```csharp
builder.UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Throw);
```

Supported behaviors are `Append`, `Throw`, `Replace`, and `Ignore`. This is independent of `DuplicateValueMatchMode`, which affects values emitted by a compiled index at match time.

Build validation is opt-in and evaluates generic diagnostics:

```csharp
builder.ValidateOnBuild(PatternDiagnosticSeverity.Warning);
```

Use the predicate overload for domain-specific strictness:

```csharp
builder.ValidateOnBuild(diagnostic => diagnostic.Kind == PatternDiagnosticKind.OverlappingWildcard);
```


## Try method failure semantics

`TryMatch` and `TryMatchDetailed` do not publish partial results. When a destination span is too small, the method returns `false`, reports zero written counts, and does not write to caller-provided destination spans. Use `Match` or the span-based `MatchDetailed` when you prefer an exception on insufficient capacity.

## Preview status

All packages are still alpha/preview. The core is the stable direction, but public API names may still change before beta. See [release decisions](../release/release-decisions.md).
