# Packaging

The solution produces four packages:

- `Pattrn`
- `Pattrn.Strings`
- `Pattrn.DependencyInjection`
- `Pattrn.Routing`

All packages use the MIT license and include a package-scoped README, icon, XML docs, repository metadata, and symbol packages.

`Pattrn.Routing` is intentionally packaged separately from the core so route-template syntax can evolve without adding route-specific policy to the generic matcher.

Package versions are centralized in `Directory.Build.props`. External package dependency versions are centralized in `Directory.Packages.props`.

Before publishing or handing off packages, run:

```bash
bash eng/restore.sh
bash eng/build.sh
bash eng/test.sh
bash eng/pack.sh
bash eng/inspect-packages.sh
```
