# Packaging

The solution produces four packages:

- `Pattrn`
- `Pattrn.Strings`
- `Pattrn.DependencyInjection`
- `Pattrn.Routing`

All packages use the MIT license and include a package-scoped README, icon, XML docs, and symbol packages.

`Pattrn.Routing` is intentionally packaged separately from the core so route-template syntax can evolve without adding route-specific policy to the generic matcher.

There is no Git repository today. Repository metadata uses neutral `RepositoryType=none`; `RepositoryUrl` and SourceLink are intentionally omitted until a real repository URL exists.
