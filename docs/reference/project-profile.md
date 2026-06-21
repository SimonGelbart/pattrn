# Project Profile

This file records durable public facts about Pattrn.

Do not put private identities, credentials, local paths, validation transcripts, generated artifacts, or short-lived task notes here.

## Project identity

| Field | Value |
|---|---|
| Project name | Pattrn |
| Repository slug | `SimonGelbart/pattrn` |
| License | MIT |
| Status | Pre-beta / alpha-line product shaping |
| Primary platform | .NET 10 |
| Primary language | C# |

## Product statement

Pattrn is a fast, immutable, segmented-pattern index for .NET backend applications and libraries.

It provides deterministic matching, captures, specificity metadata, and optional diagnostics for hierarchical keys while keeping domain and framework semantics outside the core.

## Package family

| Package | Role | Stability posture |
|---|---|---|
| `Pattrn` | Generic segmented-pattern matching core. | Stable candidate before beta. |
| `Pattrn.Strings` | String splitting, normalization, separators, and string-path ergonomics. | Stable candidate before beta. |
| `Pattrn.DependencyInjection` | Thin dependency-injection registration helpers. | Stable candidate before beta. |
| `Pattrn.Routing` | Framework-neutral route-template parsing and structural compilation. | Preview until explicitly stabilized. |

## Public interfaces

The public surface consists of:

- NuGet packages;
- public C# APIs under `src/`;
- package README files;
- documented matching, diagnostics, capture, duplicate, and ranking semantics;
- tests and public API snapshots used to protect the intended surface.

Compiled index internals are not a public format and must not become a serialization contract.

## Current direction

Pattrn is still pre-beta. Breaking alpha-era APIs are acceptable when doing so makes the long-term product smaller, clearer, faster, or more predictable.

Roadmap milestones are product-readiness stages, not package-version numbers. Package versions are centralized and intentionally simple until beta/1.0 version policy is finalized.

## Canonical documentation

- Documentation index: `docs/README.md`
- Roadmap: `docs/roadmap.md`
- Repository layout: `docs/reference/repository-layout.md`
- Validation reference: `docs/reference/validation.md`
- Architecture boundaries: `docs/reference/architecture/boundaries.md`
- ADR index: `docs/adr/README.md`
