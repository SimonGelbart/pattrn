# ADR 0004: Keep domain helpers in companion packages

## Status

Superseded

## Date

2026-06-21

## Context

Pattrn needs useful string, dependency-injection, and route-template ergonomics without making the generic matcher depend on those domains or on ASP.NET Core.

## Decision

Keep domain helpers in thin companion packages:

- `Pattrn.Strings` owns string splitting and normalization.
- `Pattrn.DependencyInjection` owns thin service-registration helpers.
- Route-template parsing is retained only as historical experimental content and
  is not part of the supported package pipeline.
- ASP.NET Core endpoint behavior is not part of the core or routing companion.

Companion packages translate their syntax into the generic segmented core and must not redefine core matching or framework precedence. The supported package story is limited to the core, string convenience, and DI convenience packages.

## Consequences

The core remains reusable outside web applications and filesystem or policy tools. Companion-package APIs can evolve independently, but consumers need the appropriate package or adapter for domain syntax.

## Alternatives considered

- Put all helpers in the core: rejected because it couples the matcher to unrelated domains.
- Make the routing package an ASP.NET Core router: rejected because framework metadata and precedence are not generic segmented matching.
