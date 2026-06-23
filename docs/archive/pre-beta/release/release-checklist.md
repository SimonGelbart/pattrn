# Release checklist

> Historical note: this file preserves earlier alpha-line release notes. Current roadmap and versioning policy live in `docs/roadmap.md` and `docs/adr/0012-simplify-pre-beta-versioning-and-roadmap-milestones.md`.
> Transitional note: use the current roadmap and project profile for active product direction. This page should not be read as the current release train or versioning source of truth.


Before producing a release artifact:

- restore with the offline bundle;
- build Release with 0 warnings;
- run all tests;
- run core and DI samples;
- build benchmarks;
- pack all packages;
- inspect package metadata;
- verify MIT license metadata;
- verify packages include README, icon, XML docs, symbols, and license;
- verify centralized Git repository metadata, package version, package README, icon, license, XML docs, and symbol packages.

## Golden update rules

After every update, even a documentation-only update:

- produce a versioned source zip;
- update `CHANGELOG.md`;
- report roadmap status, including where the project is now and the next increment.

Current packages:

- `Pattrn`
- `Pattrn.Strings`
- `Pattrn.DependencyInjection`
- `Pattrn.Routing`

For beta-specific requirements, see `docs/archive/pre-beta/release/beta-checklist.md`.


## Beta.1 candidate checklist

Before cutting `3.0.0-beta.1` after the roadmap consolidation, core API reorientation, stable matching-contract line, fast/explainability split, generic/string normalization boundary, and string API ergonomics pass:

- [x] Complete tokenless builder creation, explicit-segment-first docs/samples, and public API snapshot review.
- [x] Complete pattern identity and stable match-result contract.
- [x] Complete the fast matching versus explainability separation increment.
- [x] Complete the generic normalization hooks increment.
- [x] Complete the string API ergonomics increment.
- [x] Complete optional/defaulted route-segment expansion hardening.
- [ ] Complete the next implementation increment: specificity and ranking customization.
- [ ] Bump package versions from the final alpha line to `3.0.0-beta.1`.
- [ ] Confirm CI restore/build/test/package verification passed.
- [ ] Confirm package contents include the intended README, license, icon, XML docs, symbols, and no generated benchmark artifacts.
- [ ] Publish release notes that clearly state routing and diagnostics remain preview.
