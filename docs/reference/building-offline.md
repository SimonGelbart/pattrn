# Building offline

> Transitional note: CI is the authoritative verification path. This page documents local/offline restore guidance for maintainers who need to reproduce validation outside CI.

The legacy `eng/` scripts have been retired. Use direct `dotnet` commands with a maintainer-provided NuGet configuration or local package source when offline restore is required.

Set the package source through your local NuGet configuration or command-line restore options. Keep machine-specific paths out of committed documentation.

Example local flow:

```bash
dotnet restore Pattrn.sln
dotnet build Pattrn.sln --configuration Release --no-restore
dotnet test Pattrn.sln --configuration Release --no-build
dotnet pack Pattrn.sln --configuration Release --no-build
```

The solution targets .NET 10 only.

Package projects publish Git repository metadata through centralized build properties. Package and dependency versions are centralized in `Directory.Build.props` and `Directory.Packages.props`.
