# Open Questions

| ID | Phase | Question | Blocking? | Context | Needed before | Status |
|---|---|---|---|---|---|---|
| Q1 | Phase 1 | Should accepted ADR files `0002`, `0005`-`0011` be added to `docs.site.json` routes or intentionally remain non-rendered? | No | Docs site currently renders only a subset of accepted ADRs. | Any manifest expansion/removal phase. | Open |
| Q2 | Phase 1 | Should raw benchmark markdown under `docs/benchmark-results/**/raw/` remain committed or transition to summary-only retention? | No | Raw files look generated but are currently retained as historical evidence. | Any deletion/archival action touching benchmark-results raw output. | Open |
| Q3 | Phase 1 | Should `CHANGELOG.md` retain full alpha chronology or be reduced with older history moved to archive references? | No | Possible duplication with archived pre-beta release docs and version-reset messaging. | Any consolidation/rewrite of release/history docs. | Open |
