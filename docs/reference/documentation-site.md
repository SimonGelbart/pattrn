# Documentation Site

Pattrn publishes documentation from the `main` branch through GitHub Pages using Astro.

## Canonical source

Markdown files in the repository remain the canonical documentation source:

```text
README.md
docs/**/*.md
```

Generated site output is build artifact content. Do not edit generated HTML by hand.

## Rendering model

The site keeps a route manifest in:

```text
docs.site.json
```

This manifest records:

- the repository/source root metadata used for source links;
- the list of published docs routes and their source Markdown files;
- section grouping metadata for rendered docs navigation;
- paths intentionally excluded from rendered docs routing (for example archive and historical benchmark output folders).

Entries with `render:false` are represented by curated Astro pages instead of the generic Markdown renderer. They still stay in `docs.site.json` so source links, route ownership, and docs navigation metadata remain explicit.

Astro uses:

- `src/pages/[...doc].astro` to render selected Markdown entries from the manifest as styled `/pattrn/.../` routes;
- `src/site/layouts/DocLayout.astro` and docs components under `src/site/components/` for rendered doc pages;
- `tools/remark-doc-links.mjs` to rewrite relative Markdown links to internal rendered routes when available, with source-link fallback for non-rendered docs.

## Source layout

The source tree separates documentation content from the Pages implementation:

- Markdown files under `docs/**` remain the canonical documentation source. Update these files for documentation content changes.
- `docs.site.json` curates which canonical Markdown files render as public documentation routes and records route ownership metadata.
- Top-level curated Astro routes live in `src/pages/**`, including routes for entries marked `render:false` in the manifest. These pages stand in for the generic Markdown renderer without changing the canonical Markdown source.
- Shared site implementation lives in `src/site/**`: layouts, components, helper code, and CSS.
- Generated HTML and other build output are artifacts only. Do not edit generated output to change the published site.

## Curated index routes

Top-level routes such as `/docs/`, `/reference/`, `/packages/`, `/adr/`, `/roadmap/`, and `/benchmarks/` remain curated Astro pages.

These pages should link to internal rendered docs routes when available, and only use source links when a document is intentionally not rendered as a dedicated route.

## URL model

```text
https://simongelbart.github.io/pattrn/
https://simongelbart.github.io/pattrn/docs/
https://simongelbart.github.io/pattrn/reference/api/
https://simongelbart.github.io/pattrn/adr/0001-core-remains-segmented-and-domain-neutral/
```
