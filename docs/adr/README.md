# Architecture Decision Records

ADRs record decisions that constrain the architecture or public contracts. Current behavior belongs in the user and maintainer references; historical experiments remain in Git history.

## Accepted decisions

| ADR | Decision |
|---|---|
| [0001](0001-core-remains-segmented-and-domain-neutral.md) | Keep the core segmented and domain-neutral. |
| [0002](0002-target-dotnet-10-only.md) | Target .NET 10 during the current product cycle. |
| [0003](0003-explicit-pattern-segments-are-primary.md) | Make explicit pattern segments the primary core model. |
| [0004](0004-companion-packages.md) | Keep domain helpers in companion packages and framework boundaries outside the core. |
| [0005](0005-compiled-index-internals-are-private.md) | Keep compiled-index internals private and non-serializable. |
| [0006](0006-hot-path-matching-policy.md) | Keep normal matching synchronous and diagnostics off the hot path. |
| [0007](0007-use-fixed-ranking-with-consumer-side-sorting.md) | Use fixed generic ranking and consumer-side sorting. |
| [0008](0008-builders-single-writer-compiled-indexes-concurrent-reader-safe.md) | Keep builders single-writer and compiled indexes safe for concurrent readers. |
| [0009](0009-use-concrete-compiled-index-contract.md) | Use the concrete compiled-index contract. |

## Writing a new ADR

Add an ADR only when a choice has meaningful alternatives, affects future implementation or public compatibility, and cannot be explained adequately in a current reference page. Use the sections `Status`, `Context`, `Decision`, `Consequences`, and `Alternatives considered`. Do not use ADRs for backlog items, release notes, routine process, or obvious API facts.

## History-reset boundary

This cleanup is the final normalization of the project's previously murky ADR history. Until it is accepted, legacy records may be merged, removed, or renamed to establish a coherent baseline.

After this baseline is accepted, ADR numbers, filenames, titles, context, decisions, consequences, and alternatives are immutable. If a decision changes, create a new ADR that links to the old one and change the old record only through a validated status transition such as `Superseded` or `Deprecated`.

The accepted baseline uses 0001–0009. The 0000 file is the template; 0010 is the next available ADR number.
