# Pattrn

`Pattrn` is the supported immutable segmented-pattern index. It accepts
already-segmented input, compiles registrations into a deterministic snapshot,
and supports exact, best-prefix, and all-prefix matching.

## Install

```xml
<PackageReference Include="Pattrn" Version="0.1.0-alpha.1" />
```

## Core model

Use explicit `PatternSegment<TSegment>` values for literals, wildcards,
parameters, and terminal catch-alls:

```csharp
using Pattrn.Matching;
using Pattrn.Patterns;

var index = PattrnIndex<string, string>
    .Builder()
    .AddPattern(
        [
            PatternSegment<string>.Literal("orders"),
            PatternSegment<string>.Parameter("id")
        ],
        "order-handler")
    .Build();

var matches = index.MatchValuesToArray(["orders", "123"]);
```

The core does not parse URLs, route templates, filesystem globs, or arbitrary
expressions. Tokenized literal-only convenience methods remain available when
an application intentionally configures a wildcard token.

## Match families

- `Match...` methods require complete input consumption.
- `MatchPrefix...` methods select the deepest accepted prefix.
- `EnumeratePrefix...` methods return all accepted prefix depths.
- `...Values` methods project values and apply the configured duplicate-value
  behavior.
- `Try...` methods write into caller-owned buffers.
- Detailed methods expose registration identity and named captures.

See [the semantic contract](../semantics.md) for ordering, duplicates, empty
paths, comparer behavior, and capture rules.

## Immutable lifecycle

Builders are single-writer registration sources. `Build` or `Compile` validates
the complete set and returns an immutable index suitable for concurrent reads.
For updates, build a replacement snapshot and publish it at the application
boundary.

## Companion packages

`Pattrn.Strings` is a maintenance-only convenience layer for splitting and
normalizing strings before matching. `Pattrn.DependencyInjection` is a
maintenance-only registration helper for compiled indexes. The experimental
`Pattrn.Routing` package is not part of the supported core product story.

## Trimming and Native AOT

The repository validates `Pattrn` with the trimming and Native AOT harness. The
core implementation does not use reflection or dynamic code generation. See
[the validation reference](../reference/aot-trimming.md) for the exact harness,
warning policy, and limits.

## Performance posture

The core is intended for read-heavy workloads. Caller-buffer APIs can avoid
owning result arrays, while owning and detailed APIs allocate by contract.
Compilation is a complete snapshot build; frequent individual mutations are not
the target workload. Benchmark your own registration count, comparer, input
shape, result count, and update cadence.
