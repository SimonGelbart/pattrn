# ADR 0005: Keep compiled-index internals private

## Status

Accepted

## Date

2026-06-21

## Context

Pattrn compiles registrations into internal structures optimized for matching. Those structures may change as performance, diagnostics, and implementation needs evolve.

## Decision

Compiled-index internals are private and are not a serialization or merge format. Applications that need persistence serialize registrations or domain metadata and rebuild an index.

## Consequences

Implementation changes do not become binary-format compatibility breaks. Persistence remains explicit and application-owned.

## Alternatives considered

- Serialize compiled indexes: rejected because it freezes private representation and creates versioning obligations.
- Expose internal nodes or merge compiled indexes: rejected because those structures are not a stable public contract.
