# Building offline

The repository is designed to restore from the provided offline NuGet bundle.

Set `NUGET_BUNDLE_PACKAGES` when the bundle is not located in one of the default paths:

```bash
export NUGET_BUNDLE_PACKAGES=/absolute/path/to/tracepack-nuget-bundle/packages
./eng/restore.sh
./eng/build.sh
./eng/test.sh
./eng/pack.sh
./eng/inspect-packages.sh
```

The solution targets .NET 10 only.

There is no Git repository today, so package projects intentionally omit `RepositoryUrl`, use neutral `RepositoryType=none`, and omit SourceLink metadata. Add real repository metadata once a repository exists.


## MSBuild node count

The offline scripts use `-m:1` for solution restore and build entry points. This avoids container-specific MSBuild/NuGet hangs observed when the environment also sets platform-like variables such as `PLATFORM=linux/amd64`. The common script already unsets `PLATFORM`.
