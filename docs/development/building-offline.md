# Building offline

The repository can restore from a local offline NuGet package bundle.

Set `NUGET_BUNDLE_PACKAGES` when the bundle is not located in one of the default relative paths:

```bash
export NUGET_BUNDLE_PACKAGES=/absolute/path/to/offline-nuget-bundle/packages
bash eng/restore.sh
bash eng/build.sh
bash eng/test.sh
bash eng/pack.sh
bash eng/inspect-packages.sh
```

Set `DOTNET` when the required .NET 10 SDK is not discoverable on `PATH`:

```bash
export DOTNET=/absolute/path/to/dotnet
```

The solution targets .NET 10 only.

Package projects publish Git repository metadata through centralized build properties. Package and dependency versions are centralized in `Directory.Build.props` and `Directory.Packages.props`.

## MSBuild node count

The offline scripts use `-m:1` for solution restore and build entry points. This avoids container-specific MSBuild/NuGet hangs observed when the environment also sets platform-like variables such as `PLATFORM=linux/amd64`. The common script already unsets `PLATFORM`.
