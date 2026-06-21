# ADR 0009: Compiled index internals are private and not serialized

## Status

Accepted

## Context

Pattrn compiles registrations into internal structures optimized for matching. Those structures may need to change for performance, maintainability, and diagnostics.

## Historical context

The implementation has evolved through packed data structures, exact-only fast paths, writer paths, diagnostics, and route expansion metadata. Treating compiled internals as a serialized public format would freeze implementation details too early.

## Decision

Compiled index internals are private and must not be serialized as a stable format.

Applications that need persistence should serialize registration DTOs or their own domain metadata and rebuild an index at startup.

## Consequences

Implementation can continue to improve without breaking a binary serialized format. Persistence remains explicit and application-owned.

A future registration DTO model is still useful, but it must describe input registrations, not internal trie/index structures.

## Alternatives considered

- Serialize compiled indexes: rejected because it freezes private implementation details and complicates version compatibility.
- Expose internal nodes for advanced users: rejected for the same reason.

## Follow-up work

Add serialization-friendly registration DTOs in a later milestone without exposing compiled internals.
