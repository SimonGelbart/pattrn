# ADR 0014: Builders are single-writer and compiled indexes are concurrent-reader safe

## Status

Accepted

## Context

Pattrn separates registration-time construction from read-time matching. The builder is mutable construction state: callers add, replace, remove, validate, and diagnose registrations before compiling an immutable index.

Historical architecture notes documented that compiled indexes are safe for concurrent readers while builders are intentionally mutable and not thread-safe. That decision still shapes the public API and should be captured as an ADR rather than preserved only in discarded design drafts.

## Decision

`PattrnIndexBuilder<TSegment, TValue>` and related builder facades remain single-writer mutable construction objects. Pattrn does not promise concurrent mutation safety for builders.

Compiled indexes are immutable snapshots and are safe for concurrent read operations after construction.

Callers that discover or load registrations concurrently should collect those registrations first, order them deterministically, and then apply them to one builder from a single writer before calling `Build()`.

## Consequences

The core avoids locking, snapshot, and mutation-order complexity on the registration path. This keeps the implementation smaller and preserves deterministic registration-order tie-breaking for equal-specificity matches and duplicate handling.

Consumers that need parallel discovery own the coordination step before registration. They can still publish compiled indexes safely to concurrent readers once the immutable snapshot is built.

## Alternatives considered

- Make builders internally thread-safe: rejected because locking and snapshot semantics would complicate registration order, diagnostics, and performance for a construction-only object.
- Allow concurrent builder mutation with unspecified ordering: rejected because deterministic registration-order tie-breaking is part of Pattrn's matching model.
- Require callers to build indexes per thread and merge compiled indexes: rejected because compiled index internals are private and not a serialization or merge format.
