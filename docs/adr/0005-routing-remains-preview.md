# ADR 0005: Routing remains preview and outside the initial stable scope

## Status

Accepted

## Context

Routing is important, but route syntax and route precedence are easy to confuse with ASP.NET Core or other framework-specific behavior.

## Historical context

The current implementation includes route-template parsing, preserved constraints/defaults/optional metadata, optional/defaulted expansion metadata, and optional route-layer constraint validation. That work is useful, but it is broader and more domain-specific than the stable core/string/DI candidate scope.

## Decision

`Pattrn.Routing` remains preview and is not required for the first stable product scope.

Routing must remain framework-neutral and compile into the generic core model. It must not claim to replace ASP.NET Core endpoint routing.

## Consequences

Core stabilization can proceed without being blocked by route API naming and semantics.

Routing users can still experiment with the companion package while preview status communicates that its API may change.

## Alternatives considered

- Stabilize routing together with the core: rejected because it expands beta scope and risks leaking route assumptions into the core.
- Remove routing from the repository: rejected because useful implementation and tests already exist and provide feedback for companion-package boundaries.

## Follow-up work

Document current routing behavior accurately, especially where older compatibility docs are stale. Revisit routing stability after the core/string/DI beta surface is clear.
