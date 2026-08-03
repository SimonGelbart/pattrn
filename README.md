# Pattrn

Pattrn is an immutable segmented-pattern index for read-heavy .NET
applications. Registrations are compiled into deterministic snapshots that
support exact, best-prefix, and all-prefix matching. It is intended for
workloads where lookups greatly outnumber registration changes.

## Project status

Pattrn is maintained as a focused immutable matching index. Active feature
expansion is paused. Correctness, compatibility, packaging, and documentation
fixes remain welcome. New capabilities require evidence from a concrete
application.

The project targets .NET 10. The compiled index is immutable and safe for
concurrent readers; builders and registration sources are mutable only while a
new snapshot is being prepared.

## Installation

```xml
<PackageReference Include="Pattrn" Version="0.1.0-alpha.1" />
```

The package is published under the MIT license. Package metadata, readmes, and
the supported validation surface are kept in this repository.

## Core example

Patterns are explicit segmented values. A literal matches through the
configured segment comparer, while wildcard, parameter, and catch-all intent
is represented by a segment kind.

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

For a hot read path, size a caller-owned destination with the corresponding
upper-bound method:

```csharp
var destination = new string[index.GetMatchCountUpperBound(["orders", "123"])];
index.TryMatchValues(["orders", "123"], destination, out var written);
```

## Supported match modes

- Exact matching requires the pattern to consume the complete input.
- Best-prefix matching returns registrations at the deepest accepted prefix.
- All-prefix enumeration returns every accepted prefix from shallowest to
  deepest.
- Detailed APIs expose registration identity, consumed depth, and named
  captures when an application needs that information.
- Caller-buffer APIs avoid owning result arrays when the supplied buffers are
  sufficient.

Result ordering, duplicate-value behavior, captures, empty paths, and comparer
semantics are defined in [the semantic contract](docs/semantics.md).

## Intended workload

Pattrn fits indexes built at startup, configuration reload, deployment, or
another deliberate batch boundary and then queried many times. Rebuilding a
snapshot is the update operation. Applications should publish the complete new
snapshot atomically and let existing readers finish against the old one.

## Limitations

Pattrn is not a dynamic subscription registry, HTTP routing framework, message
broker, rules engine, cache, distributed key-value store, authorization
framework, or event-processing platform. It does not provide persistence,
retries, delivery, durability, or messaging semantics.

Indexes are immutable after compilation. Frequent individual mutations are a
poor fit because affected registrations must be compiled again. Owning result
APIs allocate, and a specialized domain trie may be simpler or faster for a
known workload. See [the limitations](docs/limitations.md) and
[the performance notes](docs/performance.md) before choosing an implementation.

## Package classification

| Package | Role | Support posture |
|---|---|---|
| `Pattrn` | Generic segmented-pattern index | Supported |
| `Pattrn.Strings` | String splitting and normalization convenience APIs | Maintenance-only |
| `Pattrn.DependencyInjection` | Thin DI registration helpers | Maintenance-only |

The core package has no string, DI, or framework dependency. Companion packages
translate their input into the core model; they do not change generic matching
semantics.

## Documentation

- [Product scope](docs/product.md)
- [Semantic contract](docs/semantics.md)
- [Performance notes](docs/performance.md)
- [Limitations](docs/limitations.md)
- [Documentation hub](docs/README.md)
- [Core API reference](docs/reference/api.md)
- [Validation and AOT guidance](docs/reference/validation.md)
- [Architecture decisions](docs/adr/README.md)

## Maintenance policy

The maintenance priority is correctness, deterministic behavior, compatibility,
package quality, trimming/AOT validation, and clear documentation. Performance
changes require focused benchmark evidence on representative data. New product
features are out of scope unless a concrete application demonstrates that the
feature is necessary and compatible with the immutable read-heavy model.
