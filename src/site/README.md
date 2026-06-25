# Pattrn Pages implementation

This directory contains the Astro implementation for the Pattrn documentation site.

It renders selected canonical Markdown files from `docs/**` through routes declared in `docs.site.json`.

- `kit/` contains internal reusable Lab Pages primitives for project documentation sites.
- `project/` contains Pattrn-specific site composition, copy, navigation, and visual styling.

Keep public route entry points in `src/pages/**`. Do not treat this directory as canonical documentation content. Update `docs/**` for documentation changes.
