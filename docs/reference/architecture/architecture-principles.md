# Architecture Principles

## Small domain-neutral core

The core package models segmented pattern matching. It should remain independent of HTTP, ASP.NET Core, filesystems, authorization, tenants, OpenAPI, and business-rule semantics.

## Explicit segments first

The primary core model is `PatternSegment<TSegment>` with explicit literal, parameter, wildcard, and terminal catch-all segment kinds.

String syntax and route templates are convenience layers that compile into the core model.

## Matching is not interpretation

Pattrn can say which registrations matched, what was captured, and how matches rank. Consumers decide what matched values mean and whether domain-specific policies accept or reject a result.

## Fast paths stay fast

The immutable compiled index is optimized for repeated reads. Diagnostics and rejected-candidate explanations are optional because they are more expensive.

## Deterministic behavior

Match ordering, duplicate behavior, capture behavior, terminal catch-all behavior, and ranking metadata must be deterministic, documented, and tested before beta.
