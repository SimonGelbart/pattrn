# tests/Pattrn.AotCompatibility/AGENTS.md

Use this file for trimming and Native AOT compatibility work.

## Context to read

Read the root `AGENTS.md` first. Then read:

- `docs/reference/aot-trimming.md`
- `docs/reference/validation.md`
- affected files under `tests/Pattrn.AotCompatibility/`
- relevant product package code when compatibility behavior changes

## Compatibility scope

- Stable candidate package validation covers `Pattrn`, `Pattrn.Strings`, and `Pattrn.DependencyInjection`.
- `Pattrn.Routing` is preview and intentionally excluded from the stable-package AOT/trimming validation harness.
- The harness references project outputs directly and does not validate packed NuGet artifacts.
- Generated publish outputs, local artifacts, and validation logs must not be committed.

## Validation

Run `tests/Pattrn.AotCompatibility/validate-aot.sh` from the repository root when the change affects trimming or Native AOT posture and the required toolchain is available.

Report attempted, passed, failed, and skipped checks honestly. Skips must be tied to explicit missing prerequisites or unsupported host/target execution, not vague publish failures.
