# ADR 0015: Remove public compiled-index interface

## Status

Accepted

## Context

`IPattrnIndex<TSegment, TValue>` exposed a broad compiled-index contract before beta. The interface covered the complete read surface for matching, detailed matching, upper-bound calculation, and diagnostic explanation.

That abstraction was premature for the current product stage. Pattrn has no beta users, no proven alternate compiled-index implementations, and no evidence yet that interface-based substitution is needed for testing, mocking, or integration. Freezing the interface before beta would expand the public compatibility surface at the same time that the project is deliberately hardening the API shape.

Accepted ADRs still apply: compiled indexes remain immutable snapshots safe for concurrent readers, compiled-index internals remain private, and dependency-injection integration remains a thin companion package. No accepted ADR requires a public compiled-index interface.

## Decision

Remove the public `IPattrnIndex<TSegment, TValue>` interface.

Use the concrete immutable `PattrnIndex<TSegment, TValue>` type as the public compiled-index contract for the core package and for companion-package APIs that operate on compiled indexes.

`Pattrn.DependencyInjection` registers concrete `PattrnIndex<TSegment, TValue>` instances. Default registrations register concrete indexes as singletons, and named registrations use keyed concrete indexes.

## Rationale

- There are no beta users yet, so the breaking change can be made before the API is treated as stable.
- There are no proven alternate compiled-index implementations to justify a public abstraction.
- A smaller public surface reduces compatibility obligations before beta.
- The concrete immutable type communicates the actual lifecycle and concurrency model more clearly.
- Removing the interface avoids promising alternate implementations or substitutability before there is evidence that the project needs them.

## Consequences

- Consumers inject, resolve, store, and use `PattrnIndex<TSegment, TValue>` directly.
- `Pattrn.DependencyInjection` registers concrete `PattrnIndex<TSegment, TValue>` instances only.
- Named-index provider APIs return concrete `PattrnIndex<TSegment, TValue>` instances.
- Public API snapshots, docs, samples, and package READMEs use the concrete compiled-index type.
- Future interface work can be reconsidered if real alternate implementations, mocking/testing needs, or integration requirements emerge.
- If an interface is reconsidered later, it should be designed from observed needs rather than reintroducing the removed pre-beta surface by default.

## Non-goals

- This does not redesign matching APIs.
- This does not redesign match result or capture types.
- This does not introduce a replacement abstraction.
- This does not change core pattern semantics.
- This does not change routing behavior.
