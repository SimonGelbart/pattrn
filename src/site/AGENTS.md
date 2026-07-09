# src/site/AGENTS.md

Use this file for the Astro documentation site under `src/pages/`, `src/site/kit/`, and `src/site/project/`.

## Context to read

Read the root `AGENTS.md` first. Then read:

- `docs/reference/documentation-site.md`
- `docs/reference/repository-layout.md`
- `docs/reference/documentation-standards.md` when the change affects documentation content or navigation
- `docs.site.json`
- `package.json`
- affected Astro, TypeScript, CSS, or Markdown files

## Site boundaries

- Markdown files under `docs/**` remain the canonical documentation source.
- `docs.site.json` curates which Markdown files render as public documentation routes and records route ownership metadata.
- `src/pages/**` contains Astro route entry points and curated pages.
- `src/site/kit/**` contains internal reusable docs-rendering helpers, source-link and navigation helpers, layout pieces, and docs CSS primitives.
- `src/site/project/**` contains Pattrn-specific composition, visual styling, navigation labels, and project copy.
- Generated HTML and build output are artifacts only. Do not edit generated output by hand.

## Validation

For site changes, run `npm run build` when feasible. If skipped, report it as `Not run` with the reason.
