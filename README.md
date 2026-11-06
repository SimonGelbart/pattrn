# Pattrn

Archived testing field for segmented-pattern matching in .NET 10.

This repository is not under active development. It was used to explore matching semantics, immutable indexing, package boundaries, and low-allocation implementation choices.

The `experiment/pattrn-lean-core` branch is a separate continuation check: it exists to test whether a smaller version of the idea is worth continuing. The main branch remains as a record of the experiment.

## Packages

| Package | Purpose | Status |
|---|---|---|
| `Pattrn` | Core immutable segmented-pattern index. | Pre-beta stable candidate. |
| `Pattrn.Strings` | String splitting, normalization, and string-path ergonomics. | Pre-beta stable candidate. |
| `Pattrn.DependencyInjection` | Thin Microsoft.Extensions.DependencyInjection integration. | Pre-beta stable candidate. |
| `Pattrn.Routing` | Framework-neutral route-template parsing helpers built on the generic core. | Preview. |

## First-use example

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
        "order-handler",
        name: "orders-by-id")
    .Build();

var matches = index.MatchValuesToArray(["orders", "123"]);
```

## Start with the docs hub

Use the [documentation index](docs/README.md) for package guidance, tutorials, and the current behavior reference.

Key links:

- [Documentation index](docs/README.md)
- [Core package docs](docs/packages/pattrn.md)
- [API overview](docs/reference/api.md)
- [Matching semantics](docs/reference/matching-semantics.md)
- [Architecture decisions](docs/adr/README.md)
