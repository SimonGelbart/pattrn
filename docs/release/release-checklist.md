# Release checklist

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
- verify neutral `RepositoryType=none` metadata and no `RepositoryUrl` until a real Git repository exists.

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

For beta-specific requirements, see `docs/release/beta-checklist.md`.


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
- [ ] Run `./eng/restore.sh`.
- [ ] Run `./eng/build.sh`.
- [ ] Run `./eng/test.sh`.
- [ ] Run `./eng/pack.sh`.
- [ ] Run `./eng/inspect-packages.sh`.
- [ ] Confirm package contents include the intended README, license, icon, XML docs, symbols, and no generated benchmark artifacts.
- [ ] Publish release notes that clearly state routing and diagnostics remain preview.
