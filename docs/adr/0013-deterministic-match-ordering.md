# ADR 0013: Deterministic match ordering

## Status

Accepted

## Date

2026-07-23

## Context

Pattrn exposes exact matching, deepest best-prefix matching, and all-prefix
enumeration as separate result families. Their returned order is observable
behavior, but the generic core must not expose route-specific ranking metadata or
custom ranking hooks.

## Decision

The core uses fixed internal specificity and deterministic registration order:

- exact and best-prefix results sort by specificity descending, then input
  registration order ascending for true ties;
- best-prefix results are restricted to the deepest accepted consumed depth
  before specificity ordering is applied;
- prefix enumeration sorts consumed depth ascending, then specificity descending,
  then registration order ascending;
- a zero-length terminal catch-all is lower-ranked than a non-catch-all
  registration at the same consumed depth;
- default value deduplication retains the first value accepted by this ordering;
- duplicate-preserving matching retains every accepted result in this ordering;
- normal, value-only, owning detailed, and caller-buffer detailed projections
  preserve the corresponding operation's order;
- specificity, rank scores, and registration order remain internal.

This decision refines the fixed-ranking policy in [ADR 0007](0007-use-fixed-ranking-with-consumer-side-sorting.md)
and coordinates the explicit operation families established by [ADR 0010](0010-canonical-registrations-and-explicit-match-families.md).

## Consequences

Consumers can select the first result when generic specificity is sufficient and
can apply domain-specific priority to their registered values afterward. The
implementation must test every result projection against the same ordering
contract, including root prefixes, true ties, and zero-length catch-alls.

No public ranking comparer, numeric specificity field, registration-order field,
or route-aware precedence hook is added.

## Alternatives considered

- Exposing ranking metadata was rejected because it would make implementation
  details part of the stable result contract.
- Allowing a caller-provided ranking comparer was rejected because route and
  domain precedence belong outside the generic core.
- Treating prefix depth as ordinary specificity was rejected because best-prefix
  selection and prefix enumeration have distinct contracts.
