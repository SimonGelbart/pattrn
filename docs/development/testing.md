# Testing

The test project uses TUnit on top of Microsoft.Testing.Platform.

.NET 10 requires the test runner to be selected in `global.json`; this repository opts into:

```json
{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

## Offline restore

Unzip the provided NuGet bundle next to the repository as:

```text
../tracepack-nuget-bundle/packages
```

Then run:

```bash
export NUGET_CERT_REVOCATION_MODE=offline
dotnet restore Pattrn.sln /p:NuGetAudit=false /p:Platform="Any CPU"
dotnet test Pattrn.sln -c Release --no-restore /p:NuGetAudit=false /p:Platform="Any CPU"
```

The explicit platform property avoids environment-specific `Platform=linux/amd64` values being interpreted as a Visual Studio solution platform.

## Current coverage

The test suite covers:

- exact matching;
- no-match behavior;
- empty path and empty pattern behavior;
- wildcard at first, middle, and last segments;
- multiple wildcards;
- overlapping exact and wildcard patterns;
- explicit prefix matching compatible with the starter-kit reference behavior;
- reference-inspired market/instrument subscription examples;
- single-segment wildcard boundaries, including zero-segment and prefix-mode cases;
- default deduplication and disabled deduplication;
- duplicate-heavy overlapping exact/wildcard branches;
- repeated values on the same pattern;
- caller-provided destination spans;
- fluent builder registration chains;
- `AddRange`, `Contains`, `RemoveAll`, `Clear`, and builder remove overloads;
- branch pruning after removing exact and wildcard branches;
- non-throwing `TryMatch` overloads;
- distinct pattern/registration count semantics;
- immutable build snapshots;
- case-insensitive segment comparers;
- custom value comparers;
- dotted and separated string helpers;
- concurrent reads;
- randomized equivalence against a deliberately naive matcher;
- randomized equivalence with custom segment and value comparers.

## Benchmarks

The benchmark project is intentionally separate from the test suite. Use it to inspect performance and allocation behavior after functional tests pass:

```bash
dotnet run -c Release --project benchmarks/Pattrn.Benchmarks /p:Platform="Any CPU" -- --filter '*' --job short
```

See [benchmarks](benchmarks.md).
