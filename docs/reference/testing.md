# Testing

The test projects use TUnit on top of Microsoft.Testing.Platform. CI is the authoritative verification path; local commands in this page are optional maintainer preflight helpers. See [validation](../reference/validation.md).

.NET 10 SDK selection and the Microsoft.Testing.Platform runner are configured in `global.json`. This repository currently opts into:

```json
{
  "sdk": {
    "version": "10.0.301",
    "rollForward": "latestFeature"
  },
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

The `sdk` section selects the compatible .NET 10 SDK feature band for local and CI restore/build/test commands. The `test` section selects Microsoft.Testing.Platform as the .NET test runner.

## Local preflight

Use ordinary restore, build, and test commands for supported local validation:

```bash
dotnet restore Pattrn.sln
dotnet build Pattrn.sln --configuration Release --no-restore
dotnet test Pattrn.sln --configuration Release --no-build
```

The explicit Release configuration matches CI build output. Use `dotnet --version` or `dotnet --info` if you need to verify which SDK `global.json` selected.

Legacy offline NuGet bundle paths and special restore flags are not the supported default workflow. If offline restore is required for maintainer recovery, configure an appropriate local NuGet package source and run the same direct `dotnet` commands rather than relying on repository-specific bundle paths.

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

## Benchmark tool tests

The Python benchmark helper tests are part of the normal validation set because the CI workflow runs them before .NET test assemblies:

```bash
python -m unittest discover tools/benchmarks/tests
```

## Benchmarks

The benchmark project is intentionally separate from the test suite. Local benchmark commands are optional maintainer preflight helpers; current benchmark proof comes from workflow artifacts and summaries. If you still need a local inspection run, use:

```bash
dotnet run -c Release --project benchmarks/Pattrn.Benchmarks -- --filter '*' --job short
```

See [benchmarks](benchmarks.md).
