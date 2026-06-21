# Matching semantics

`Pattrn` matches an incoming segmented path against registered segmented patterns.

The semantics are intentionally small and explicit so the library remains useful outside the starter-kit domain.

## Terms

A **path** is a sequence of segments:

```text
NASDAQ.MSFT
market.NASDAQ.MSFT
```

A **pattern** is also a sequence of segments. Pattern segments can be exact segments or the configured single-segment wildcard.

```text
NASDAQ.MSFT    exact pattern
NASDAQ.*       wildcard pattern
NASDAQ         prefix-capable pattern when prefix mode is enabled
```

The wildcard segment is configured when the builder is created:

```csharp
var builder = PattrnIndex<string, string>.Builder("*");
```

The generic pattern-segment API avoids reserving a segment value for wildcard intent:

```csharp
builder.AddPattern(
    [
        PatternSegment<string>.Literal("orders"),
        PatternSegment<string>.Parameter("id")
    ],
    "order-handler");
```

`Literal` matches exact values, including values equal to the configured wildcard token. `Wildcard` and `Parameter` match exactly one input segment. `CatchAll` matches zero or more remaining input segments and must be terminal. Parameter and named catch-all names are returned by detailed match APIs as `PatternCapture<TSegment>` values.

## Exact-length matching by default

Default matching requires the pattern and input path to have the same number of segments.

```text
pattern: NASDAQ
path:    NASDAQ.MSFT
result:  no match by default
```

This default is deliberate. The starter kit's reference project behaves like a subscription tree where shorter subscriptions can match longer message paths. A general-purpose library should not assume that prefix behavior unless the caller opts in.

## Explicit prefix matching

Prefix matching is enabled at build time:

```csharp
var index = builder.Build(MatchOptions.Prefix);
```

With prefix matching enabled, a pattern can match the beginning of a longer input path.

```text
pattern: NASDAQ
path:    NASDAQ.MSFT
result:  match
```

A pattern longer than the input path never matches, even when prefix matching is enabled.

```text
pattern: NASDAQ.MSFT.QUOTE
path:    NASDAQ.MSFT
result:  no match
```

## Terminal catch-all

The generic pattern-segment API supports terminal catch-alls:

```csharp
builder.AddPattern(
    [
        PatternSegment<string>.Literal("files"),
        PatternSegment<string>.CatchAll("path")
    ],
    "handler");
```

A catch-all matches zero or more remaining input segments. It must be the final segment in the registered pattern.

```text
pattern: files.<catch-all>
path:    files
result:  match

pattern: files.<catch-all>
path:    files.a.b.c
result:  match
```

Named catch-alls are exposed as one capture per remaining input segment. The core does not join segments into strings, decode paths, or parse route-template syntax; companion packages can add those domain-specific behaviors.

## Single-segment wildcard

The wildcard matches exactly one input segment.

```text
pattern: NASDAQ.*
path:    NASDAQ.MSFT
result:  match
```

It does not match zero segments.

```text
pattern: NASDAQ.*
path:    NASDAQ
result:  no match
```

When prefix matching is enabled, a wildcard pattern can become a prefix only after the wildcard has consumed one segment.

```text
pattern: NASDAQ.*
path:    NASDAQ.MSFT.QUOTE
result:  match when prefix matching is enabled
```

## Empty patterns

The core segmented API allows an empty pattern.

By default, an empty pattern matches only an empty input path.

```text
pattern: <empty>
path:    <empty>
result:  match
```

With prefix matching enabled, the empty pattern matches every input path because it is the prefix of every path.

```text
pattern: <empty>
path:    NASDAQ.MSFT
result:  match when prefix matching is enabled
```

The dotted string helpers deliberately reject empty strings. Use the core segmented APIs when an empty pattern or empty path is intentional.

## Overlapping patterns

All matching patterns contribute their values.

```text
patterns:
  NASDAQ
  NASDAQ.*
  *.MSFT
  NASDAQ.MSFT

path:
  NASDAQ.MSFT

prefix mode result:
  all four patterns match
```

## Deduplication

Deduplication is enabled by default. If overlapping patterns resolve to equal values, the value is returned once according to the builder's value comparer.

```csharp
var index = builder.Build(); // DeduplicateValues is true by default
```

Deduplication can be disabled:

```csharp
var index = builder.Build(MatchOptions.PreserveDuplicates);
```

When deduplication is disabled, repeated registrations or overlapping patterns can return the same value multiple times.

## Result ordering

When multiple structural branches match the same input at the same depth, results are emitted deterministically by generic specificity:

```text
literal > named parameter > anonymous wildcard > terminal catch-all
```

`PatternMatch<TValue>.Specificity` and `PatternMatchResult<TSegment, TValue>.Specificity` expose the specificity value for detailed results. Higher values are more specific. The broad ordering above is compatibility-covered; exact numeric weights remain an implementation detail.

Registrations with the same structural specificity preserve registration order when duplicate preservation is enabled. When default value deduplication suppresses equal values, the first accepted value in deterministic rank order wins.

Prefix mode is deterministic but traversal ordered. A registration at a prefix node is emitted before deeper descendant registrations. Use detailed match metadata for consumer-side sorting if a scenario needs one global ranking order across prefix and descendant matches.

See [ranking and specificity](ranking-specificity.md) for the full contract and consumer-side sorting guidance.

## Threading

The builder is mutable and not thread-safe.

The compiled index is immutable and safe for concurrent readers.
