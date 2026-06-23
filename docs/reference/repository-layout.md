# Repository Layout

## Root files

| Path | Purpose |
|---|---|
| `AGENTS.md` | Rules for assisted and automated repository changes. |
| `Directory.Build.props` | Shared build, package, and repository metadata. |
| `Directory.Packages.props` | Central package version management. |
| `Pattrn.sln` | Solution entry point for restore, build, test, and package commands. |
| `README.md` | Product overview and first-use examples. |
| `CHANGELOG.md` | Release and migration notes. |

## Product code

| Path | Purpose |
|---|---|
| `src/Pattrn` | Core generic segmented-pattern index. |
| `src/Pattrn.Strings` | String-path companion package. |
| `src/Pattrn.DependencyInjection` | DI companion package. |
| `src/Pattrn.Routing` | Preview route-template companion package. |
| `samples/` | Compileable examples. |

## Quality and automation

| Path | Purpose |
|---|---|
| `tests/` | Unit, compatibility, package metadata, and public API snapshot tests. |
| `benchmarks/` | BenchmarkDotNet benchmark project. |
| `.github/workflows/` | GitHub Actions workflows. |

Legacy local `eng/` scripts have been retired. CI workflows are the durable automation entry points for restore, build, test, package, documentation, and benchmark verification.

## Documentation

Pattrn is moving toward the Diataxis documentation structure:

| Path | Purpose |
|---|---|
| `docs/tutorials/` | Learning-oriented walkthroughs. |
| `docs/how-to/` | Task-oriented procedures. |
| `docs/reference/` | Stable technical reference. |
| `docs/explanation/` | Rationale and conceptual background. |
| `docs/adr/` | Architecture Decision Records. |

Historical pre-beta folders such as `docs/getting-started/`, `docs/design/`, and `docs/release/` have been moved under `docs/archive/pre-beta/`. Current maintainer guidance lives under `docs/reference/`.
