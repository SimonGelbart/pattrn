# ADR 0008: ASP.NET Core behavior stays outside the core

## Status

Accepted

## Context

ASP.NET Core endpoint routing has framework-specific semantics, metadata, constraints, precedence, and integration behavior. Pattrn can be useful under routing-like systems without becoming an ASP.NET Core router.

## Historical context

The current routing companion package is framework-neutral. Historical docs repeatedly describe ASP.NET Core compatibility as optional and future-facing, not part of the core.

## Decision

Do not add ASP.NET Core behavior to `Pattrn`.

If demand appears, create a separate `Pattrn.AspNetCore` companion package that adapts ASP.NET Core metadata to Pattrn's model without moving framework behavior into the core.

## Consequences

The core remains reusable outside web apps. ASP.NET Core users may need an adapter package later.

The routing companion package must avoid claiming full compatibility with ASP.NET Core endpoint routing.

## Alternatives considered

- Make `Pattrn.Routing` mimic ASP.NET Core: rejected because it would blur product positioning and create a large compatibility burden.
- Add endpoint metadata to core matches: rejected because consumer values and companion packages can carry metadata externally.

## Follow-up work

Keep docs explicit that Pattrn is not a router and does not own HTTP method, endpoint, or middleware semantics.
