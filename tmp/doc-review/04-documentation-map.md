# Phase 4 — Documentation Map and Ownership

## Proposed documentation map

| Path | Purpose | Audience | Source of truth for | Allowed content | Disallowed content | Related canonical docs |
|---|---|---|---|---|---|---|
| `README.md` | Short product overview and first-use entry point. | New users, evaluators | Product summary, first example, package high-level positioning | Concise overview, first example, links to docs hub | Deep reference details, maintainer workflow policy, historical dumps | `docs/README.md`, `docs/reference/project-profile.md`, `docs/reference/api.md` |
| `docs/README.md` | Current documentation index and user path. | All docs readers | Canonical docs navigation and section routing | Current docs entry links by Diataxis + package docs + ADR index + historical-context pointers | Replacing section docs with duplicated deep guidance | `docs/reference/documentation-standards.md`, `docs/reference/repository-layout.md` |
| `docs/tutorials/` | Learning-oriented walkthroughs. | New adopters | Step-by-step onboarding flows | End-to-end guided learning content | Canonical API contracts or maintainer policy | `docs/tutorials/README.md`, `docs/tutorials/first-index.md` |
| `docs/how-to/` | Task-oriented procedures. | Practitioners | Practical “how do I do X” workflows | Focused procedures for concrete tasks | Conceptual essays, canonical architecture decisions | `docs/how-to/README.md`, `docs/reference/ranking-specificity.md` |
| `docs/reference/` | Stable facts and technical/maintainer reference. | Users and maintainers | API behavior, validation policy, repository layout, package boundaries, stable constraints | Durable factual docs: APIs, semantics, diagnostics, limitations, validation, workflow references | Transient notes, historical-only narrative, release diaries | `docs/reference/api.md`, `docs/reference/validation.md`, `docs/reference/repository-layout.md`, `docs/reference/documentation-standards.md` |
| `docs/explanation/` | Concepts and rationale. | Users evaluating design trade-offs | Why the design is structured this way | Conceptual and rationale-oriented docs | Procedural runbooks, full API tables | `docs/explanation/package-boundaries.md`, `docs/adr/README.md` |
| `docs/adr/` | Accepted architecture decisions only. | Maintainers, contributors | Decision records and decision history | ADR documents with status and decision consequences | General docs backlog, tutorials, arbitrary notes | `docs/adr/README.md`, accepted ADR files |
| `docs/packages/` | Package-scoped README/NuGet guidance sources. | Package consumers | Package-level usage and package README content | Package-specific guidance aligned with package boundaries | Repository-wide policy that belongs in reference | `docs/reference/package-readmes.md`, `README.md`, package `.csproj` README metadata |
| `docs/archive/pre-beta/` | Historical pre-beta notes. | Maintainers, historians | Historical context only | Archived design/release/getting-started notes with historical framing | Primary current user path, current source-of-truth claims | `docs/archive/README.md`, `docs/README.md` historical section |
| `docs/benchmark-results/` | Historical committed benchmark evidence only. | Maintainers, reviewers | Historical benchmark snapshots | Historical benchmark baselines and preserved evidence | Current performance proof claims | `docs/reference/benchmarks.md`, benchmark workflow artifacts |
| `docs.site.json` | Public docs route manifest for current docs surface. | Maintainers, docs-site pipeline | Rendered pages, route metadata, exclusions | Current public docs pages and explicit excludes | Archive trees, benchmark dumps, raw logs, transitional notes | `docs/reference/documentation-site.md`, `docs/README.md` |

## Topic ownership table

| Topic | Canonical location | Secondary links allowed? | Notes |
|---|---|---|---|
| Product overview | `README.md` | Yes | Keep short and redirect to docs hub for depth. |
| First example | `README.md` and `docs/tutorials/first-index.md` | Yes | README should keep one first-use example only. |
| Package selection | `docs/reference/project-profile.md` | Yes | Summary can appear in README/docs index. |
| Core API | `docs/reference/api.md` | Yes | Package docs and README may link but not duplicate full contract. |
| String helper API | `docs/packages/pattrn-strings.md` | Yes | Core API page can reference, not absorb package-specific details. |
| DI | `docs/packages/pattrn-dependency-injection.md` | Yes | Keep DI package specifics package-scoped. |
| Routing | `docs/packages/pattrn-routing.md` | Yes | Routing remains preview; cross-link from roadmap/reference. |
| Pattern segments | `docs/reference/pattern-segments.md` | Yes | Semantic authority belongs in reference. |
| Matching semantics | `docs/reference/matching-semantics.md` | Yes | How-to/tutorial pages may link to use-cases. |
| Ranking | `docs/reference/ranking-specificity.md` | Yes | ADR 0013 is decision history, reference is behavioral contract. |
| Duplicates | `docs/reference/duplicate-behavior.md` | Yes | Keep semantics centralized. |
| Diagnostics | `docs/reference/diagnostics.md` | Yes | README can mention optional diagnostics at high level only. |
| Limitations | `docs/reference/limitations.md` | Yes | Roadmap and package docs may link. |
| Benchmarks | `docs/reference/benchmarks.md` | Yes | Current proof must come from benchmark CI artifacts/summaries. |
| Validation | `docs/reference/validation.md` | Yes | CI-first authority; local preflight is secondary. |
| Repository layout | `docs/reference/repository-layout.md` | Yes | Docs index can link for maintainer orientation. |
| Git workflow | `docs/reference/git-workflow.md` | Yes | Keep contributor process centralized. |
| Release history | `CHANGELOG.md` | Yes | Older migration/release notes should remain in archive. |
| Roadmap | `docs/roadmap.md` | Yes | README/docs index should point here for current direction. |
| Migration notes | `docs/archive/` | Yes | Keep current docs links contextual and clearly historical. |
| ADRs | `docs/adr/README.md` + accepted ADR files | Yes | ADRs are decision records, not a general docs bucket. |

## Proposed edits to documentation standards, if any

1. Add explicit path-ownership bullets mirroring this map (README scope, docs index scope, Diataxis folders, package docs, archive, benchmark-results, ADR-only rule).
2. Add a benchmark-proof rule: current benchmark claims must reference CI workflow artifacts/summaries; committed benchmark-results are historical.
3. Add docs-site manifest rule: include current public pages only; exclude archive, benchmark dumps, raw logs, and transitional notes.
4. Add archive-linking rule: current docs may link to archive only when historical context adds value and the link is labeled historical.

## Proposed docs.site.json public-surface rules

1. Include only current public docs pages that belong to the active user/maintainer path.
2. Keep explicit excludes for `docs/archive/**` and `docs/benchmark-results/**`.
3. Do not add raw logs, generated benchmark dumps, or transitional review notes to the docs manifest.
4. Keep ADR routing policy explicit (rendered ADR subset vs all accepted ADRs) and consistent with `docs/adr/README.md` discoverability expectations.
5. Treat `docs.site.json` as a curation layer, not as a backlog for every markdown file.

## Blocking questions

- None.

## Nonblocking questions

1. Should `docs/reference/documentation-standards.md` be updated now to codify all 15 map rules explicitly, or staged with other docs-governance edits?
2. Should docs-site ADR coverage expand to all accepted ADR pages for discoverability consistency, or remain curated with index/source links?

## Recommended next phase

Phase 5 should apply the approved map to current docs: update `docs/reference/documentation-standards.md`, align `docs/README.md` historical-link framing, and align `docs.site.json` curation policy with ADR discoverability and exclusion rules.
