# ADR 0010: Canonical registrations and explicit match families

## Status

Accepted

## Context

The phase-01 API needs stable registration identity, immutable builder snapshots,
diagnostics that can be consumed without parsing text, and unambiguous result
semantics. A single option-controlled prefix mode made exact matching, best-prefix
matching, and prefix enumeration easy to confuse. Legacy builder and compiler
paths also discarded registration metadata.

## Decision

`PattrnRegistration<TSegment, TValue>` is the canonical immutable input. Each
registration has a non-empty `RegistrationId`, an immutable pattern snapshot, a
value, and an optional name. Builders store only an ordered canonical list;
`Replace`, `Remove`, `Clear`, and `ToRegistrations` operate on that list. The
compiler has one authoritative path and preserves IDs, names, and registration
order in the compiled result.

`MatchOptions` contains only duplicate-value behavior. Exact matching, best-prefix
matching, and all-prefix enumeration are selected by distinct method families.
Exact and prefix result records expose only value, registration identity, and
consumed segment count. Detailed results own captures; caller-buffer detailed
APIs use capture slices.

Compilation diagnostics use stable `PTRN` codes, explicit error/warning severity,
optional registration identity, and optional zero-based pattern-segment
attribution. Overlap analysis is not part of this phase.

## Consequences

The public API is breaking but the semantics are explicit and framework-neutral.
Companion packages translate their domain inputs into canonical registrations and
use the explicit result families. Fixed specificity and registration-order
sorting remain implementation details; no ranking metadata is exposed.

## Alternatives considered

- Retaining the parallel legacy compiler or builder would preserve conflicting
  semantics and was rejected.
- Keeping prefix selection in `MatchOptions` was rejected because it conflates
  three distinct query contracts.
- Adding overlap diagnostics was deferred because it requires a separate analysis
  model and is outside phase 01.
