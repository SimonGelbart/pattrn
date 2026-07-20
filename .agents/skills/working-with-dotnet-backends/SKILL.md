---
name: working-with-dotnet-backends
description: Explore, modify, build, and test .NET backend solutions with minimal context. Use for C# backend work involving .sln or .slnx files, csproj dependencies, NuGet packages, ASP.NET Core services, shared contracts, targeted dotnet build commands, or focused dotnet test verification.
---

# Work With .NET Backends

Use the smallest .NET workflow that identifies the affected boundary and verifies the change.

For a shared contract or multi-project change, run a command-compatibility
preflight before spending time on the full gate: inspect `global.json`, check
the solution/test-platform shape, and prefer the repository's documented test
module command over guessing CLI syntax. Preserve both the exact requested
command result and the canonical equivalent in the final report when they
differ.

## Discover The Solution

```bash
fd -t f '^(global\.json|Directory\.Build\.(props|targets)|Directory\.Packages\.props|.*\.(sln|slnx|csproj))$'
dotnet sln SOLUTION list
dotnet --info
```

Inspect the SDK pin and shared props only when they affect the task.

## Edit Gate

Do not edit C#/.NET files until these are known or explicitly irrelevant:

1. Affected solution or project
2. SDK pin, if present
3. Shared build files, if present
4. Affected test project or reason no test project is relevant
5. Whether the change touches shared contracts, public APIs, packages, or cross-cutting behavior

If any item is unknown, perform bounded discovery before editing.

## Map Dependencies

Before changing a shared type, package, or service contract:

```bash
dotnet list PROJECT.csproj reference
dotnet list PROJECT.csproj package --format json
rg -n -C 2 -F 'TypeOrContractName' .
```

Use `jq -c` to project only the required fields from JSON output. For .NET 10 and later, noun-first forms such as `dotnet package list --project PROJECT.csproj` are also available.

## Inspect Selectively

Search for the relevant type, route, handler, or registration before reading implementations:

```bash
rg -n -C 2 -g '*.cs' -F 'TargetSymbol' src tests
bat --line-range START:END --style=numbers path/to/File.cs
```

For multi-tool exploration, use `composing-cli-workflows`.

## Verify Progressively

Start with the affected project and test scope:

```bash
dotnet build path/to/Project.csproj --nologo -v:minimal
dotnet test path/to/Project.Tests.csproj --nologo -v:minimal --no-build
dotnet test path/to/Project.Tests.csproj --nologo -v:minimal --no-build --filter 'FullyQualifiedName~TargetNamespace'
```

Use `--no-restore` only when restore assets are current. Broaden to the solution when shared contracts, packages, or cross-cutting behavior changed. Increase verbosity only when compact failure output is insufficient.

Do not run build, test, and pack concurrently when they share `bin/` or
`obj/`. Independent Python, documentation, API-snapshot, and artifact checks
may run in parallel when their output paths are isolated.

## Verification Failure Policy

When build or test verification fails:

1. Capture only the smallest relevant failure excerpt.
2. Classify the failure as caused by the change, pre-existing, environment/tooling, or unknown.
3. If caused by the change, fix and rerun the same narrow command first.
4. If unknown, run one narrower diagnostic command before escalating.
5. Do not escalate to full solution verification until the affected-project result is understood.

## Escalation Matrix

Run project-level build first for changes limited to one project.
Run affected test project first when tests exist.
Run solution-wide build/test only when one of these is true:

1. Shared contract or public API changed
2. Package version or central props changed
3. Project reference changed
4. Generated code or shared build target changed
5. Narrow verification passed but cross-project risk remains

State the escalation reason before running the wider command.

When tests are removed or renamed during an API migration, perform a focused
test-inventory review before declaring the change complete: each removed test
must have a replacement contract test or an explicit obsolete-behavior reason.

Read [the .NET workflow recipes](./reference/dotnet-backend-recipes.md) for dependency audits, ASP.NET Core searches, and escalation guidance.
