# ADR 0007: Matching APIs remain synchronous

## Status

Accepted

## Context

Pattrn matches against an immutable in-memory compiled index. Matching does not require I/O and should be usable in hot paths.

## Historical context

The current public APIs are synchronous. Existing performance work protects span-based matching, caller-provided buffers, and allocation-conscious hot paths. Async matching would add API surface without a matching I/O operation.

## Decision

Do not add async matching APIs to the core.

Consumers that perform asynchronous work after a match should keep that work outside Pattrn.

## Consequences

The API surface stays smaller and avoids misleading users into thinking matching performs I/O.

Consumers can still wrap matching in their own async workflows when the surrounding domain operation is asynchronous.

## Alternatives considered

- Add `MatchAsync` for symmetry with web frameworks: rejected because it would be artificial.
- Add async diagnostics or validation: deferred unless a future companion package genuinely needs I/O.

## Follow-up work

Keep examples synchronous. Revisit only if a future feature introduces unavoidable asynchronous I/O outside the core.
