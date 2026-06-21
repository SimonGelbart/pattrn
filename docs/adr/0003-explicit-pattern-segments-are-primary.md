# ADR 0003: Explicit pattern segments are the primary core model

## Status

Accepted

## Context

A segmented matcher needs to distinguish literals from parameters, wildcards, and catch-alls without reserving magic values in the user's segment type.

## Historical context

Earlier alpha APIs supported tokenized wildcard registration as a compact convenience. The alpha design later moved toward tokenless builders and explicit `PatternSegment<TSegment>` registrations. Existing docs and tests already describe explicit segment registration as the documentation-first model.

## Decision

The primary core model is explicit `PatternSegment<TSegment>` registration.

Tokenized APIs may remain as convenience wrappers, but they must compile into explicit pattern segments and must not define the core mental model.

## Consequences

The core can support arbitrary segment types without reserving a wildcard token. APIs are clearer for parameters, catch-alls, captures, and future diagnostics.

Tokenized patterns remain useful for short examples but must be documented as convenience APIs.

## Alternatives considered

- Keep wildcard-token registration as the primary model: rejected because it makes one segment value special and is ambiguous for generic segment types.
- Make strings the primary core input: rejected because strings and separators belong in `Pattrn.Strings`.

## Follow-up work

Keep README examples and package docs centered on explicit segments. Use string and route examples only after the core model is clear.
