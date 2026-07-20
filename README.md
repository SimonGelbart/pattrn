# Pattrn

Fast immutable segmented-pattern matching for .NET 10.

`Pattrn` indexes segmented patterns once, then resolves matching inputs quickly and predictably. The core package is generic and dependency-light; string paths, dependency-injection helpers, and route-template syntax live in companion packages.

Pattrn is pre-beta. The public surface may still change while the core contracts are being exercised by real users.

## Packages

| Package | Purpose | Status |
|---|---|---|
| `Pattrn` | Core immutable segmented-pattern index. | Pre-beta stable candidate. |
| `Pattrn.Strings` | String splitting, normalization, and string-path ergonomics. | Pre-beta stable candidate. |
| `Pattrn.DependencyInjection` | Thin Microsoft.Extensions.DependencyInjection integration. | Pre-beta stable candidate. |
| `Pattrn.Routing` | Framework-neutral route-template parsing helpers built on the generic core. | Preview. |

## First-use example

```csharp
using Pattrn;

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
