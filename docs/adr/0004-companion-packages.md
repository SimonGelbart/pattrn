# ADR 0004: Strings, DI, and Routing are companion packages

## Status

Accepted

## Context

Pattrn needs ergonomics for common use cases without forcing those concerns into the core package.

## Historical context

The current repository already contains `Pattrn`, `Pattrn.Strings`, `Pattrn.DependencyInjection`, and `Pattrn.Routing`. Historical docs sometimes present these as one package family while also warning that route and string semantics should stay outside the core.

## Decision

Keep string helpers, dependency-injection helpers, and route-template helpers in companion packages.

- `Pattrn.Strings` owns separators, string splitting, case sensitivity, trimming, empty-segment behavior, and string normalization.
- `Pattrn.DependencyInjection` owns thin registration helpers.
- `Pattrn.Routing` owns framework-neutral route-template parsing and route-layer validation while it remains preview.

## Consequences

The package family can serve common backend use cases while preserving the core boundary.

Companion packages must stay thin and should translate their syntax into the generic core model.

## Alternatives considered

- Put all helpers in the core package: rejected because it would make the core depend on string, routing, and DI concerns.
- Split every helper into a separate repository: rejected because the package family is still small and benefits from shared tests and validation.

## Follow-up work

Before beta, confirm the stable package names and decide whether `Pattrn.Routing` stays preview.
