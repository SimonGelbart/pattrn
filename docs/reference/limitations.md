# Known limitations

This document records intentional pre-beta product boundaries. These are product choices, not missing framework features.

## .NET target

`Pattrn` targets .NET 10 only. Multi-targeting can be revisited later if there is clear demand.

## Core stays segmented and generic

The core package works with already-segmented paths and patterns. String parsing and normalization belong in `Pattrn.Strings`; route-template parsing, URL normalization, OpenAPI semantics, and ASP.NET Core integration belong in companion packages or applications.

## Routing package is preview

`Pattrn.Routing` currently supports literal route segments, named parameters, terminal catch-alls, preserved constraint/default/optional metadata, structured parse diagnostics, and optional/defaulted suffix expansion. Constraint validation, URL decoding, ASP.NET Core integration, OpenAPI behavior, and broader optional route semantics are intentionally deferred.

## Constraints are not enforced by the core

The core preserves generic pattern metadata and captures, but it does not validate values as `int`, `guid`, `slug`, regex, tenant policy, or any other domain-specific rule.

## Catch-all semantics are segmented

Core catch-all captures are returned as one `PatternCapture<TSegment>` per captured input segment. String joining such as `"a/b/c"` is a routing-layer convenience, not a core behavior.

## Synchronous matching

Matching is synchronous by design. The index performs CPU-local traversal over already available memory. There are no `Task`, `ValueTask`, or async matching APIs.

Async may be appropriate for loading registrations from an asynchronous source, but not for the core in-memory match operation.

## String helper allocations

String and routing helpers split strings before matching. They are convenience APIs. Hot callers should prefer already-segmented paths, caller-provided route split buffers, and span-based core matching APIs.

## No framework behavior

The library returns matching values and metadata. It does not provide endpoint execution, authorization decisions, handler invocation, ASP.NET Core routing, OpenAPI compatibility, or filesystem globbing. Those can be built as companion packages or application layers.
