# ADR 0007: Use fixed ranking with consumer-side sorting

## Status

Accepted

## Date

2026-06-21

## Context

Pattrn must return deterministic results when literal, parameter, wildcard, catch-all, duplicate, and prefix registrations overlap. It also needs to stay domain-neutral. A public ranking plugin or comparer would be attractive for advanced routing-like domains, but it would widen the beta surface before real users have shown that extensibility is necessary.

## Decision

Pattrn uses fixed built-in ranking for the core matching engine.

The stable pre-beta contract is:

- literal matches outrank named parameters;
- named parameters outrank anonymous wildcards;
- anonymous wildcards outrank terminal catch-alls;
- equal structural specificity preserves registration order;
- default value deduplication keeps the first accepted value in deterministic rank order;
- prefix mode emits accepted prefix-node registrations before deeper descendant registrations;
- route constraints, defaults, and optional metadata do not add core specificity;
- detailed matches expose metadata for consumer-side sorting.

Do not add a public ranking comparer, ranking plugin, `SpecificityOptions`, or route-aware precedence hook before beta.

## Consequences

The core remains smaller, faster, and easier to validate. Ranking behavior can be documented and tested without committing to an extensibility model too early.

Consumers with domain-specific precedence can sort detailed matches themselves using `Specificity`, `RegistrationOrder`, `PatternId`, captures, and their own value metadata.

Some advanced scenarios will need a small amount of consumer-side code instead of a first-class Pattrn ranking extension point.

## Alternatives considered

- Add a public ranking comparer now: rejected because it creates a large compatibility surface before there is user demand.
- Add route-aware precedence to the core: rejected because route semantics belong outside the domain-neutral engine.
- Hide specificity metadata entirely: rejected because consumers need enough information to explain and sort matches.

## Follow-up work

Keep `docs/reference/matching-semantics.md` and ranking tests aligned with this ADR. Revisit ranking extensibility only if real consumers need a first-class hook.
