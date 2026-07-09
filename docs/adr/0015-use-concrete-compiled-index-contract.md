# ADR 0015: Use concrete compiled index contract

## Status

Accepted

## Context

`IPattrnIndex<TSegment, TValue>` exposed a broad compiled-index contract before beta. The project has no known users and no proven need for alternate compiled-index implementations, so keeping a public interface would freeze too much API surface too early.

The compiled index itself is already an immutable snapshot and is safe for concurrent readers after construction. That concrete type is the behavior callers build, inject, and match against.

## Decision

Remove the public `IPattrnIndex<TSegment, TValue>` interface.

Use concrete immutable `PattrnIndex<TSegment, TValue>` as the public compiled-index contract across the core package, companion packages, dependency-injection registration, docs, samples, and public API snapshots.

## Consequences

`Pattrn.DependencyInjection` registers concrete `PattrnIndex<TSegment, TValue>` instances for default and named/keyed indexes. Consumers inject or resolve the concrete immutable type.

The beta candidate surface is smaller, clearer, and carries fewer compatibility obligations. Public docs no longer promise alternate compiled-index implementations or interface-based substitution before there is evidence that those seams are useful.

A future interface can be reconsidered if real alternate implementations, mocking or testing needs, or integration requirements emerge after beta feedback.

## Non-goals

This decision does not redesign matching APIs.

This decision does not redesign match result or capture types.

This decision does not introduce a replacement abstraction.

## Alternatives considered

- Keep `IPattrnIndex<TSegment, TValue>` for future flexibility: rejected because flexibility without proven implementations would freeze a broad contract prematurely.
- Mark the interface obsolete for a compatibility period: rejected because this is a pre-beta breaking change with no known users.
- Add a smaller replacement interface: rejected because it would preserve the same premature-abstraction problem under another name.
