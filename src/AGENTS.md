# src/AGENTS.md

Use this file for product code, public API, package implementation, and samples that depend on product behavior.

## Context to read

Read the root `AGENTS.md` first. Then read the smallest relevant set:

- `docs/reference/project-profile.md`
- `docs/reference/repository-layout.md`
- `docs/reference/validation.md`
- `docs/reference/coding/csharp.md`
- relevant accepted ADRs only when the task touches their decisions

For public API, core matching behavior, package boundaries, or dependencies, also read:

- `docs/reference/architecture/architecture-principles.md`
- `docs/reference/architecture/boundaries.md`
- `docs/reference/architecture/dependency-policy.md`
- `docs/reference/architecture/testing-strategy.md`

## Package boundaries

- `src/Pattrn` owns the generic segmented-pattern core, immutable indexes, captures, duplicate behavior, specificity metadata, and optional generic diagnostics.
- `src/Pattrn.Strings` owns string splitting, normalization, separators, and string-path ergonomics.
- `src/Pattrn.DependencyInjection` owns thin Microsoft dependency-injection integration.
- `src/Pattrn.Routing` owns framework-neutral route-template parsing and route-layer validation while it remains preview.
- Do not push HTTP, ASP.NET Core, filesystem, authorization, tenant, OpenAPI, source-generation, analyzer, or serialization semantics into the core.

## C# defaults

- Target .NET 10.
- Keep nullable enabled.
- Treat nullable warnings as errors.
- Prefer immutable public models where practical.
- Keep hot-path allocations visible and intentional.
- Use spans and caller-provided buffers in performance-sensitive matching paths.
- Keep route, string, DI, and diagnostics behavior outside the core unless an ADR changes the boundary.

## Public API changes

Public API changes require tests, public API snapshot updates when applicable, documentation updates, and changelog or roadmap notes when behavior changes.

## Validation

For product code changes, use the CI restore/build/test path from `docs/reference/validation.md` unless a maintainer explicitly scopes validation smaller. Report any skipped validation as `Not run` with the reason.
