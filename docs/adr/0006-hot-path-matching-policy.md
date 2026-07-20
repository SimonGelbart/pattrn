# ADR 0006: Keep normal matching synchronous and diagnostics off the hot path

## Status

Accepted

## Date

2026-06-21

## Context

Pattrn traverses an immutable in-memory index. Matching has no I/O requirement, while explanations, rejected-candidate analysis, and validation may allocate and perform extra work.

## Decision

Normal matching remains synchronous and allocation-conscious. Do not add artificial async matching APIs to the core. Diagnostics, explanations, and validation remain opt-in and must not be required by the normal matching path.

## Consequences

The hot API stays small and predictable, and callers do not pay diagnostic costs unless they request them. Asynchronous loading or domain work remains outside Pattrn.

## Alternatives considered

- Add `MatchAsync` for API symmetry: rejected because matching is CPU-local and does not perform asynchronous I/O.
- Compute explanations on every match: rejected because it would add avoidable allocations and latency.
