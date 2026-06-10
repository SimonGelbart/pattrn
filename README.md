# Pattrn

Fast immutable segmented-pattern matching for .NET 10.

`Pattrn` indexes segmented path patterns once, then resolves matching inputs quickly and predictably. The core package is generic and dependency-free; route strings, separated strings, and dependency-injection helpers live in companion packages.

## Packages

| Package | Purpose | Status |
|---|---|---|
| `Pattrn` | Core immutable segmented-pattern index. | Preview, stable candidate. |
| `Pattrn.Strings` | Dotted/separated string helpers. | Preview, stable candidate. |
| `Pattrn.DependencyInjection` | Microsoft.Extensions.DependencyInjection integration. | Preview, stable candidate. |
| `Pattrn.Routing` | Route-template parsing helpers built on the generic core. | Preview/experimental. |

All `3.0.0-alpha.*` packages are preview packages. `3.0.0-alpha.31` is the performance guardrails and speed triage update: the alpha.30 BenchmarkDotNet run is now committed as the current baseline, speed is a roadmap gate, and exact-only core matching has direct fast paths for value and detailed span/array matching. Tokenless builders remain the default and explicit `PatternSegment<TSegment>` registrations remain the primary core model. See the [consolidated roadmap](docs/roadmap.md), [performance guardrails](docs/benchmark-results/performance-guardrails.md), [package maturity](docs/release/package-maturity.md), and the [state-of-the-art architecture review](docs/design/state-of-the-art-architecture-review.md).

## Core usage

The primary core model is explicit generic pattern segments. This avoids reserving a magic segment value for wildcards and keeps the core independent from route, glob, URL, filesystem, or application semantics.

```csharp
using Pattrn;

var index = PattrnIndex<string, string>
    .Builder()
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
        "any-nasdaq")
    .Build();

var matches = index.MatchToArray(["market", "NASDAQ", "MSFT"]);
```

For hot paths, prefer caller-provided buffers:

```csharp
var path = new[] { "market", "NASDAQ", "MSFT" };
var buffer = new string[index.GetMatchCountUpperBound(path)];
var written = index.Match(path, buffer);
```

## Generic pattern segments

`PatternSegment<TSegment>` is the documentation-first registration model for the core package. It represents literals, anonymous wildcards, named parameters, and terminal catch-alls without relying on string syntax or a reserved token.

The tokenized segmented API still exists as an opt-in compact convenience API. It uses a configured wildcard segment value, such as `"*"`, as a single-segment wildcard. The default builder is tokenless, so `Add(...)` registers literal-only segments unless the builder was created with a wildcard token. Prefer `PatternSegment<TSegment>` when correctness depends on distinguishing a literal value from a wildcard:

```csharp
var builder = PattrnIndex<string, string>.Builder();

builder.AddPattern(
    [
        PatternSegment<string>.Literal("orders"),
        PatternSegment<string>.Parameter("id")
    ],
    "order-handler");

var index = builder.Build();
var matches = index.MatchToArray(["orders", "123"]);
```

Named parameters and catch-alls are exposed through the detailed accepted-match API:

```csharp
var detailed = index.MatchDetailedToArray(["orders", "123"]);
var matchPatternId = detailed[0].PatternId;
var registrationOrder = detailed[0].RegistrationOrder;
var id = detailed[0].Captures.Single(capture => capture.Name == "id").Value;
```

For troubleshooting and tooling, use the diagnostics-oriented explanation API. Rejected-candidate diagnostics are opt-in so the default hot path stays allocation-conscious:

```csharp
var explanation = index.Explain(
    ["orders", "123"],
    PatternExplanationOptions.IncludeRejections);

var accepted = explanation.Matches;
var rejected = explanation.RejectedCandidates;
```

Terminal catch-alls are generic and segmented:

```csharp
builder.AddPattern(
    [
        PatternSegment<string>.Literal("files"),
        PatternSegment<string>.CatchAll("path")
    ],
    "file-handler");
```

Named catch-alls return one `PatternCapture<TSegment>` per captured segment. String joining, URL decoding, optional route syntax, and constraint parsing belong in companion packages.

## Matching modes

The default mode is exact-length matching with deduplicated values.

```csharp
var prefixIndex = PattrnIndex<string, string>
    .Builder()
    .Add(["market", "NASDAQ"], "all-nasdaq")
    .Add(["market", "NASDAQ", "MSFT"], "exact-msft")
    .Build(MatchOptions.Prefix);
```

`MatchOptions` separates path matching behavior from duplicate value emission. See [matching semantics](docs/reference/matching-semantics.md) and [duplicate behavior](docs/reference/duplicate-behavior.md).

## Builder diagnostics

Builders can report advisory diagnostics before an index is compiled:

```csharp
var builder = PattrnIndex<string, string>
    .Builder()
    .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Wildcard()], "wildcard")
    .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Literal("new")], "literal");

var diagnostics = builder.GetDiagnostics();
```

Diagnostics are generic and do not change matching behavior. Build validation is opt-in:

```csharp
var index = builder
    .ValidateOnBuild(PatternDiagnosticSeverity.Info)
    .Build();
```

## Companion packages

### Strings

```csharp
using Pattrn;

var index = StringPattrnIndexBuilder
    .CreateTokenized<string>('.', "*")
    .Add("market.NASDAQ.*", "client-a")
    .Build();

var matches = index.MatchToArray("market.NASDAQ.MSFT");
```

String helpers allocate because they split strings into segments. Keep hot paths on the core span APIs. Use `StringNormalizationOptions` and the string-path facade when a string domain needs explicit separators, case-insensitive matching, trimming, empty-segment handling, or custom segment normalization. The older `AddDotted`, `AddSeparated`, `MatchDottedToArray`, and `MatchSeparatedToArray` extension methods remain available for direct core-builder/index workflows.

### Dependency injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using Pattrn;
using Pattrn.DependencyInjection;

services.AddPattrnIndex<string, string>(registration => registration
    .UseWildcard("*")
    .Configure(builder => builder.Add(["market", "NASDAQ", "*"], "client-a")));

var index = provider.GetRequiredService<IPattrnIndex<string, string>>();
```

### Routing

Route-like string syntax lives in `Pattrn.Routing`, not in the core package.

```csharp
using Pattrn;
using Pattrn.Routing;

var index = PattrnIndex<string, string>
    .Builder()
    .AddRoute("/orders/{id}", "order-handler")
    .AddRoute("/files/{*path}", "file-handler")
    .Build();

var matches = index.MatchRouteDetailedToArray("/orders/123");
```

The routing package supports literal segments, named parameters, terminal named catch-alls, preserved constraints/defaults/optional suffix metadata, optional/defaulted suffix expansion metadata through `RouteTemplateExpansion`, and optional route-layer constraint validation. URL decoding, ASP.NET Core integration, OpenAPI semantics, and endpoint metadata remain outside the generic core.

## Design principles

The core package intentionally stays small:

- no DI dependency;
- no string parsing dependency;
- no async matching API;
- immutable compiled index for concurrent reads;
- span-based hot path for low-allocation matching.

## Documentation

Start with the [documentation index](docs/README.md).

Key pages:

- [Consolidated roadmap](docs/roadmap.md)
- [API overview](docs/reference/api.md)
- [Pattern segments](docs/reference/pattern-segments.md)
- [Matching semantics](docs/reference/matching-semantics.md)
- [Compatibility semantics](docs/reference/compatibility-semantics.md)
- [Benchmarks](docs/development/benchmarks.md)
- [Building offline](docs/development/building-offline.md)
- [Beta checklist](docs/release/beta-checklist.md)

## Benchmark and API status

`3.0.0-alpha.31` is a performance-first release. It commits the alpha.30 BenchmarkDotNet run as the current baseline, adds explicit performance guardrails, and protects exact-only matching with direct fast paths that bypass the general writer path when no wildcard/parameter/catch-all branches and no prefix matching are involved.

See [latest full benchmark results](docs/benchmark-results/latest.md), [latest routing benchmark results](docs/benchmark-results/routing-latest.md), and [performance guardrails](docs/benchmark-results/performance-guardrails.md). Routing helpers remain preview: string convenience helpers are ergonomic, while hot callers should prefer pre-split segments or caller-provided route split buffers.

## Beta readiness

The consolidated roadmap is now a living beta-readiness roadmap. Alpha.31 completed performance guardrails and hot-path triage. The next implementation step is alpha.32 specificity and ranking customization, followed by diagnostics hardening, serialization-friendly registrations, developer-experience cleanup, and beta API/performance review. Routing and diagnostics remain preview. ASP.NET Core integration, OpenAPI integration, globbing, analyzers, source generators, and metadata-aware matching are deferred.

## Building offline

Use the scripts in `eng/` with the provided offline NuGet bundle. See [building offline](docs/development/building-offline.md).

## License

MIT.
