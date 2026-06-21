# ADR 0001: Core remains segmented and domain-neutral

## Status

Accepted

## Context

Pattrn can support routing-like, glob-like, policy, command, topic, and resource lookup use cases. If the core owns any one of those domains, the generic engine will inherit domain-specific rules, edge cases, and performance costs.

## Historical context

Earlier alpha documentation and implementation explored routing, strings, diagnostics, package metadata, and performance work while the product boundary was still being shaped. The historical architecture review and roadmap consistently point toward a reusable segmented-pattern engine rather than a web framework, router, filesystem abstraction, authorization engine, or business-rule engine.

## Decision

`Pattrn` remains a domain-neutral segmented-pattern matching core.

The core owns segments, explicit pattern segment kinds, literals, parameters, wildcards, terminal catch-alls, captures, duplicate behavior, deterministic matching metadata, immutable compiled indexes, and optional generic diagnostics.

The core does not own HTTP, ASP.NET Core routing, filesystem globbing, authorization, tenancy, OpenAPI, endpoint metadata, business rules, or framework-specific precedence.

## Consequences

The core stays smaller, faster, and easier to reason about. Companion packages and consuming applications are responsible for translating their own syntax and interpreting matches.

Some users must write a thin adapter for their domain instead of expecting the core to understand that domain directly.

## Alternatives considered

- Make the core a router: rejected because it would import HTTP and framework precedence into the generic engine.
- Make the core a globber: rejected because glob syntax and filesystem behavior are not general segmented matching semantics.
- Add domain policy hooks to the core: deferred because they risk turning matching into interpretation.

## Follow-up work

Keep docs and tests explicit about product boundaries. Add new ADRs before moving any domain semantics into the core.
