# ADR 0009: Use the concrete compiled-index contract

## Status

Accepted

## Date

2026-07-09

## Context

The public `IPattrnIndex<TSegment, TValue>` interface would freeze an abstraction without a proven alternate implementation or consumer need. The concrete compiled index is already immutable and safe for concurrent readers.

## Decision

Use concrete immutable `PattrnIndex<TSegment, TValue>` as the public compiled-index contract across the core, companion packages, dependency-injection integration, samples, documentation, and public API snapshots.

## Consequences

The pre-beta surface is smaller and clearer, with fewer compatibility obligations. A future interface can be reconsidered if real alternate implementations or integration requirements emerge.

## Alternatives considered

- Keep the broad interface for flexibility: rejected because it would prematurely freeze an abstraction.
- Mark it obsolete or replace it with a smaller interface: rejected because both preserve the same unproven seam.
