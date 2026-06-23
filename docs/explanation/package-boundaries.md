# Package boundaries

Pattrn is organized as a small core plus companion packages. The boundary is intentional: the core package matches already-segmented inputs, while companion packages translate common syntaxes into that generic model.

## Core package

`Pattrn` owns the generic segmented-pattern model:

- literal segments;
- parameters;
- single-segment wildcards;
- terminal catch-alls;
- captures;
- duplicate behavior;
- deterministic ranking metadata;
- optional diagnostics.

The core does not own HTTP, ASP.NET Core routing, OpenAPI, URL decoding, filesystem behavior, glob syntax, dependency injection, or string normalization.

## String helpers

`Pattrn.Strings` owns string splitting and normalization:

- separators;
- case sensitivity;
- empty-segment handling;
- trimming;
- custom segment normalization;
- string-path builder and index facades.

String helpers are convenience APIs. They split strings into segments before delegating to the core matcher.

## Dependency injection

`Pattrn.DependencyInjection` owns service registration helpers. It lets applications build immutable indexes at startup and register them as services without putting DI concepts into the core matcher.

## Routing preview

`Pattrn.Routing` owns framework-neutral route-template parsing and route-layer validation. It compiles route templates into the generic core model, but it is not an ASP.NET Core router and does not implement framework route precedence.

Routing remains preview until explicitly stabilized.

## Why this split matters

Keeping these boundaries separate makes the core easier to reason about and avoids turning a generic segmented matcher into a router, globber, URL parser, or dependency-injection abstraction.

When a domain needs its own syntax or policy, prefer a thin translator above the core instead of adding that policy to `Pattrn` itself.

## Related docs

- [Project profile](../reference/project-profile.md)
- [Architecture boundaries](../reference/architecture/boundaries.md)
- [Core API](../reference/api.md)
- [String package README](../packages/pattrn-strings.md)
- [Dependency injection package README](../packages/pattrn-dependency-injection.md)
- [Routing package README](../packages/pattrn-routing.md)
