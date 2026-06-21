# Pattrn

Fast immutable segmented-pattern matching for .NET 10.

`Pattrn` indexes segmented patterns once, then resolves matching inputs quickly and predictably. The core package is generic and dependency-light; string paths, dependency-injection helpers, and route-template syntax live in companion packages.

Pattrn is pre-beta. The current package version is intentionally simple and centralized. Roadmap milestones describe product readiness, not release-train numbers.

## Packages

| Package | Purpose | Status |
|---|---|---|
| `Pattrn` | Core immutable segmented-pattern index. | Pre-beta stable candidate. |
| `Pattrn.Strings` | String splitting, normalization, and string-path ergonomics. | Pre-beta stable candidate. |
| `Pattrn.DependencyInjection` | Thin Microsoft.Extensions.DependencyInjection integration. | Pre-beta stable candidate. |
| `Pattrn.Routing` | Framework-neutral route-template parsing helpers built on the generic core. | Preview. |

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

Tokenized segmented APIs still exist as opt-in compact convenience APIs. They use a configured wildcard segment value, such as `"*"`, as a single-segment wildcard. The default builder is tokenless, so `Add(...)` registers literal-only segments unless the builder was created with a wildcard token.

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

Named parameters and catch-alls are exposed through detailed matches:

```csharp
var detailed = index.MatchDetailedToArray(["orders", "123"]);
var matchPatternId = detailed[0].PatternId;
var registrationOrder = detailed[0].RegistrationOrder;
var id = detailed[0].Captures.Single(capture => capture.Name == "id").Value;
```

For troubleshooting and tooling, use diagnostics-oriented explanation APIs. Rejected-candidate diagnostics are opt-in so the default hot path stays allocation-conscious:

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

String helpers allocate because they split strings into segments. Keep hot paths on the core span APIs. Use `StringNormalizationOptions` and the string-path facade when a string domain needs explicit separators, case-insensitive matching, trimming, empty-segment handling, or custom segment normalization.

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
- span-based hot path for low-allocation matching;
- optional diagnostics outside the default matching path.

## Documentation

Start with the [documentation index](docs/README.md).

Key pages:

- [Roadmap](docs/roadmap.md)
- [Architecture decisions](docs/adr/README.md)
- [Project profile](docs/reference/project-profile.md)
- [Validation](docs/reference/validation.md)
- [API overview](docs/reference/api.md)
- [Pattern segments](docs/reference/pattern-segments.md)
- [Matching semantics](docs/reference/matching-semantics.md)
- [Ranking and specificity](docs/reference/ranking-specificity.md)
- [Duplicate behavior](docs/reference/duplicate-behavior.md)
- [Diagnostics](docs/reference/diagnostics.md)
- [Benchmarks](docs/development/benchmarks.md)

## Product direction

The next product work is project foundation and ADR cleanup, followed by ranking/specificity documentation, internal architecture cleanup, diagnostics hardening, serialization-friendly registrations, benchmark/CI hardening, documentation and samples, and then a beta feedback surface.

Routing remains preview. Globbing, ASP.NET Core helpers, source generators, analyzers, custom ranking plugins, and multidimensional matching helpers are deferred until after the core/string/DI product is ready for beta feedback.
