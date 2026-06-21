# ADR 0006: Globbing is deferred to a post-1.0 companion package

## Status

Accepted

## Context

Filesystem and policy-style globbing is a plausible use case for a segmented-pattern engine, but glob syntax introduces domain-specific behavior such as `*`, `**`, `?`, extensions, separators, and filesystem expectations.

## Historical context

The product roadmap identifies `Pattrn.Globbing` as a future candidate and explicitly keeps glob semantics out of the core.

## Decision

Do not add globbing before beta or 1.0. Treat globbing as a post-1.0 companion package candidate.

## Consequences

The first stable product remains focused. The core can still be designed so a future globbing package can compile glob syntax into generic patterns where possible.

Users who need globbing immediately must build their own adapter or wait for a companion package.

## Alternatives considered

- Add glob syntax to the core: rejected because it would merge filesystem-like semantics into a domain-neutral package.
- Build `Pattrn.Globbing` before beta: rejected because stabilization work has higher priority.

## Follow-up work

After 1.0, evaluate real demand and design `Pattrn.Globbing` as a thin translator over the core.
