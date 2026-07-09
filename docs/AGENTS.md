# docs/AGENTS.md

Use this file for documentation-only changes under `docs/` and durable maintainer guidance.

## Context to read

Read the root `AGENTS.md` first. Then read the smallest relevant set:

- `docs/reference/documentation-standards.md`
- affected product documentation
- relevant ADRs only when the documentation references governed architecture or product decisions

Read `docs/reference/documentation-site.md` only when the change affects site routing, rendering, navigation, or `docs.site.json`.

## Documentation rules

- Use the Diataxis structure for new durable docs: tutorials, how-to, reference, explanation, and ADRs.
- Prefer one canonical source and link to it instead of duplicating long-form content.
- Keep package versions, detailed release history, local validation transcripts, generated benchmark output, environment-specific paths, and private implementation details out of durable docs unless explicitly appropriate.
- Current user-facing docs should prioritize current reference, roadmap, and project-profile sources.
- Keep `CHANGELOG.md` as public release history and clearly frame old alpha-train entries as historical pre-beta context.
- When documentation disagrees with code, tests, accepted ADRs, or the current roadmap, update it or mark it historical.

## Validation

For documentation-only wording changes with no code, samples, package metadata, workflow, or site-rendering impact, full .NET validation is usually not required. Report skipped build/test commands as `Not run` with the reason.

Run docs-rendering checks such as `npm run build` when the change affects site rendering, `docs.site.json`, Astro pages, examples that are rendered, or docs navigation.
