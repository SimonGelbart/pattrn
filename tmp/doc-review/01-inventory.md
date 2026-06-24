# Phase 1 Inventory — Documentation-Like Material

## Inventory table

| Path | Current role | Audience | Diataxis category | Status | Recommended action | Reason | Related files |
|---|---|---|---|---|---|---|---|
| `README.md` | Root product overview and first-use entry point | user | tutorial | current | keep | Primary external entry point and listed in docs index/site manifest. | `docs/README.md`, `docs.site.json`, `docs/roadmap.md` |
| `docs/README.md` | Documentation hub and navigation for current vs historical material | user | reference | current | keep | Canonical docs index; explicitly separates current paths from archive/transitional areas. | `README.md`, `docs/roadmap.md`, `docs/archive/README.md` |
| `docs/roadmap.md` | Product direction and milestone sequencing | user | explanation | current | keep | Central direction page and published in docs site manifest. | `README.md`, `docs/reference/project-profile.md`, `docs/adr/*.md` |
| `CHANGELOG.md` | Historical release train notes (alpha-era chronology) | release/history | archive | transitional | consolidate | Long alpha history may duplicate/archive release material and can obscure current pre-beta reset posture. | `docs/archive/pre-beta/release/*.md`, `docs/roadmap.md` |
| `AGENTS.md` | Maintainer/agent workflow and policy rules | maintainer | reference | current | keep | Governs change workflow and validation honesty. | `docs/reference/git-workflow.md`, `docs/reference/validation.md` |
| `docs.site.json` | Route manifest for published docs pages | maintainer | reference | current | keep | Defines published routes, source file mapping, and exclusions (`docs/archive/**`, `docs/benchmark-results/**`). | `docs/reference/documentation-site.md`, `.github/workflows/docs.yml` |
| `docs/tutorials/*.md` | Learning walkthroughs for current product | user | tutorial | current | keep | Matches Diataxis tutorial role; listed from docs index. | `docs/tutorials/README.md`, `docs.site.json` |
| `docs/how-to/*.md` | Task-oriented procedures | user | how-to | current | keep | Matches Diataxis how-to role and active docs path. | `docs/how-to/README.md`, `docs.site.json` |
| `docs/explanation/package-boundaries.md` | Conceptual package boundary rationale | user | explanation | current | keep | Aligns with ADR package-boundary decisions and current conceptual docs path. | `docs/explanation/README.md`, `docs/adr/0004-companion-packages.md` |
| `docs/explanation/README.md` | Explanation section index | user | explanation | transitional | keep | Useful local navigation, but not currently in docs site manifest. | `docs/README.md`, `docs.site.json` |
| `docs/reference/*.md` (core reference pages) | Stable technical and maintainer reference | user | reference | current | keep | Reference docs are active source-of-truth targets linked from docs index and many are in site manifest. | `docs.site.json`, `docs/README.md` |
| `docs/reference/documentation-site.md` | Documentation rendering/source-of-truth policy | maintainer | reference | current | keep | Defines manifest model and generated-output boundary for docs site. | `docs.site.json`, `.github/workflows/docs.yml` |
| `docs/reference/architecture/*.md` | Architecture rules and boundaries | maintainer | explanation | current | keep | Active maintainer guidance aligned with ADR governance. | `docs/adr/*.md`, `AGENTS.md` |
| `docs/reference/coding/*.md` | Coding standards for maintainers | maintainer | reference | current | keep | Stable maintainer standards; not user onboarding docs. | `AGENTS.md`, `docs/reference/repository-layout.md` |
| `docs/packages/*.md` | NuGet package README sources | package docs | reference | current | keep | Packed into `.nupkg` README via project files; user-facing package docs. | `src/*/*.csproj`, `docs/reference/package-readmes.md`, `docs.site.json` |
| `docs/adr/README.md` | ADR index and change rules | ADR | adr | current | keep | Canonical ADR entry point and governance. | `docs/adr/*.md`, `docs.site.json` |
| `docs/adr/0001,0003,0004,0012,0013` | Accepted architectural/product decisions | ADR | adr | current | keep | Explicitly accepted and used for precedence in current changes. | `docs/adr/README.md`, `docs/reference/*` |
| `docs/adr/0000-adr-template.md` | ADR authoring template | maintainer | reference | current | keep | Needed for future ADR creation; not product docs. | `docs/adr/README.md` |
| `docs/adr/0002,0005-0011` | Additional accepted ADRs not currently rendered in docs site | ADR | adr | current | edit | Likely should be discoverable in published ADR navigation for consistency with accepted ADR scope. | `docs.site.json`, `docs/adr/README.md` |
| `docs/archive/**/*.md` | Historical migration/design/release documentation | archive | archive | historical | keep | Explicitly marked archive/historical and referenced as such from docs index. | `docs/archive/README.md`, `docs/README.md` |
| `docs/benchmark-results/latest.md`, `routing-latest.md`, `performance-guardrails.md` | Historical benchmark summary notes | benchmark evidence | archive | historical | keep | Marked as historical evidence; useful context but not source-of-truth for current performance claims. | `docs/reference/benchmarks.md`, `.github/workflows/benchmarks.yml` |
| `docs/benchmark-results/**/raw/*.md` | Raw committed benchmark markdown outputs | generated/transient | archive | generated | needs-maintainer-decision | Large generated machine output retained for history; deletion/retention policy should be explicit before cleanup. | `docs/benchmark-results/**/README.md`, `.github/workflows/benchmarks.yml` |
| `samples/Pattrn.Samples/Program.cs` | Executable usage examples (documentation-like code) | user | tutorial | current | keep | Demonstrates core usage and prefix behavior as runnable sample docs. | `README.md`, `docs/tutorials/examples.md` |
| `samples/Pattrn.DiSample/Program.cs` | Executable DI usage example | user | tutorial | current | keep | Demonstrates companion DI package usage as runnable example docs. | `docs/packages/pattrn-dependency-injection.md`, `README.md` |
| `benchmarks/Pattrn.Benchmarks/*.cs` | Executable benchmark specification and evidence generator | benchmark evidence | reference | current | keep | Benchmark intent/source lives in code and workflow, not committed raw output. | `.github/workflows/benchmarks.yml`, `docs/reference/benchmarks.md` |
| `.github/workflows/docs.yml` | Documentation publish pipeline | maintainer | reference | current | keep | Governs docs build/publish (`npm ci`, `npm run build`). | `package.json`, `package-lock.json`, `docs.site.json` |
| `.github/workflows/ci.yml` | CI restore/build/test workflow | maintainer | reference | current | keep | Canonical validation path for code and tests. | `docs/reference/validation.md`, `global.json`, `Pattrn.sln` |
| `.github/workflows/benchmarks.yml` | Benchmark source-of-truth workflow | maintainer | reference | current | keep | Defines artifact-based benchmark evidence process. | `docs/reference/benchmarks.md`, `benchmarks/Pattrn.Benchmarks` |
| `.github/workflows/mutation-testing.yml` | Mutation testing workflow | maintainer | reference | current | keep | Maintainer-only quality workflow; documentation-adjacent validation material. | `docs/reference/testing.md` |
| `Directory.Build.props` | Central package version/public metadata used in docs claims | maintainer | reference | current | keep | Docs reference centralized pre-beta package versioning from this file. | `docs/roadmap.md`, `docs/reference/packaging.md` |
| `Directory.Packages.props` | Central dependency version metadata | maintainer | reference | current | keep | Referenced by docs as canonical dependency-version source. | `docs/roadmap.md`, `docs/reference/packaging.md` |
| `src/*/*.csproj` | Package metadata and README packing map | package docs | reference | current | keep | Contains `PackageReadmeFile` and `docs/packages/*.md` pack mapping. | `docs/reference/package-readmes.md`, `docs/packages/*.md` |
| `package.json` + `package-lock.json` | Docs-site tooling and locked dependency graph | maintainer | reference | current | keep | Required for deterministic docs site build; workflow uses `npm ci`. | `.github/workflows/docs.yml`, `docs/reference/documentation-site.md` |
| `global.json` | .NET SDK + test runner selection referenced by validation/testing docs | maintainer | reference | current | keep | Supports docs claims about CI SDK/test runner behavior. | `.github/workflows/ci.yml`, `docs/reference/testing.md`, `docs/reference/validation.md` |

## Suspected current documentation entry points
- `README.md`
- `docs/README.md`
- `docs/roadmap.md`
- `docs/tutorials/first-index.md`
- `docs/how-to/select-best-match.md`
- `docs/reference/api.md`
- `docs/packages/pattrn.md`
- `docs/adr/README.md`

## Suspected historical/archive material
- `docs/archive/**`
- `docs/benchmark-results/**`
- `CHANGELOG.md` alpha-era sections (`3.0.0-alpha.*` chronology)

## Suspected duplicates
- Release/process history appears split between `CHANGELOG.md` and `docs/archive/pre-beta/release/*.md`.
- Benchmark historical narrative appears in both `docs/benchmark-results/*` summaries and archive/release references.
- Multiple section `README.md` files (`docs/tutorials/README.md`, `docs/how-to/README.md`, `docs/explanation/README.md`) overlap with `docs/README.md` navigation responsibility.

## Suspected stale content
- `CHANGELOG.md` may be stale against centralized pre-beta version reset (`0.1.0-alpha.1`).
- `docs/reference/testing.md` contains legacy local offline restore guidance that may be transitional versus CI-first posture.

## Suspected generated/transient content
- `docs/benchmark-results/**/raw/*-report-github.md` appears generated from BenchmarkDotNet output.
- Committed benchmark comparison/report outputs under `docs/benchmark-results/**` are historical evidence, not current source-of-truth.

## docs.site.json-linked files
- 31 source entries are currently linked.
- Includes root docs, selected tutorials/how-to/explanation/reference pages, package docs, and five accepted ADRs.
- Excludes `docs/archive/**` and `docs/benchmark-results/**` by manifest rule.
- Not all accepted ADR files are currently rendered as dedicated docs-site routes.

## Files not safe to delete without maintainer decision
- `CHANGELOG.md` (historical policy/consumer expectations risk)
- `docs/archive/**` (explicit historical retention)
- `docs/benchmark-results/**` (historical benchmark evidence policy)
- `docs/benchmark-results/**/raw/*.md` (generated but currently committed historical artifacts)
- `docs/reference/testing.md` legacy sections (may still support offline maintainer workflows)

## Blocking questions
- None for Phase 2 inventory follow-up work.

## Nonblocking questions
- Should all accepted ADRs (`0002`, `0005`-`0011`) be added to `docs.site.json` published routes, or intentionally remain index-only/source-only?
- Should raw committed benchmark markdown under `docs/benchmark-results/**/raw/` remain in-repo long-term, or move to archive artifacts policy with summary-only retention?
- Should `CHANGELOG.md` be reduced to post-reset milestones and move older alpha chronology fully into archive docs?

## Recommended next phase
Phase 2 should perform a contradiction-and-duplication pass that cross-checks current docs against accepted ADRs, docs-site manifest coverage, and package metadata mappings, then propose precise consolidation actions requiring maintainer decisions.
