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

Astro uses:

- `src/pages/[...doc].astro` to render selected Markdown entries from the manifest as styled `/pattrn/.../` routes;
- `src/layouts/DocLayout.astro` and docs components for rendered doc pages;
- `tools/remark-doc-links.mjs` to rewrite relative Markdown links to internal rendered routes when available, with source-link fallback for non-rendered docs.

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
