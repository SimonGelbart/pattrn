# Documentation

Pattrn is an immutable segmented-pattern index for read-heavy .NET
applications. These Markdown files describe the current implementation and
contracts; historical experiments remain on their research branches.

## Start here

1. [Product scope](product.md)
2. [Build your first index](tutorials/first-index.md)
3. [Core package](packages/pattrn.md)
4. [Semantic contract](semantics.md)
5. [Known limitations](limitations.md)

## Current product docs

- [Performance](performance.md)
- [Core API](reference/api.md)
- [Duplicate behavior](reference/duplicate-behavior.md)
- [Diagnostics](reference/diagnostics.md)
- [Trimming and Native AOT](reference/aot-trimming.md)
- [Validation](reference/validation.md)
- [Benchmarks](reference/benchmarks.md)
- [Package boundaries](explanation/package-boundaries.md)
- [Architecture decisions](adr/README.md)

## Packages

- [Pattrn](packages/pattrn.md)
- [Pattrn.Strings](packages/pattrn-strings.md)
- [Pattrn.DependencyInjection](packages/pattrn-dependency-injection.md)

## Tutorials and how-to guides

- [First index](tutorials/first-index.md)
- [Examples](tutorials/examples.md)
- [Select a best match](how-to/select-best-match.md)

## Maintainer guidance

- [Validation reference](reference/validation.md)
- [Git workflow](reference/git-workflow.md)
- [Documentation site](reference/documentation-site.md)

The repository is maintained as a focused library. New capabilities require
evidence from a concrete application and must not broaden the generic core.
