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
- kit helpers under `src/site/kit/**` for manifest reading, source-link generation, docs navigation, internal/external route handling, rendered Markdown document layout, and reusable docs CSS;
- Pattrn-specific composition under `src/site/project/**` for the project shell, project navigation, project metadata, and project visual styling;
- `tools/remark-doc-links.mjs` to rewrite relative Markdown links to internal rendered routes when available, with source-link fallback for non-rendered docs.

## Source layout

The source tree separates documentation content from the Pages implementation:

- Markdown files under `docs/**` remain the canonical documentation source. Update these files for documentation content changes.
- `docs.site.json` curates which canonical Markdown files render as public documentation routes and records route ownership metadata.
- Top-level curated Astro routes live in `src/pages/**`, including routes for entries marked `render:false` in the manifest. These pages stand in for the generic Markdown renderer without changing the canonical Markdown source.
- Reusable site primitives live in `src/site/kit/**`. The kit is internal for now; it contains stable docs-rendering helpers and components that may become shared only after another project site adopts the model.
- Pattrn-specific site implementation lives in `src/site/project/**`. Project-specific landing copy, package descriptions, roadmap wording, benchmark wording, navigation labels, and visual identity stay outside the kit.
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


## Internal Lab Pages kit

The reusable boundary is intentionally conservative. Rendered docs use the kit for manifest lookup, route/source URL helpers, link classification, source links, docs navigation, the generic rendered Markdown layout, and docs-oriented CSS primitives. Curated Pattrn pages compose those helpers where useful, but keep Pattrn product copy and project-specific status language in `src/pages/**` or `src/site/project/**`.

The kit is not a package, workspace, or separate repository. Extraction is deferred until another project site, such as LORQ or dotnet-learning-lab, uses the same model and proves which pieces are truly shared. The main site should only adopt the docs renderer if it needs rendered project documentation, not merely for visual consistency.
