# Validation Reporting Skill Examples

## Honest skipped check

`Skipped` — `dotnet test Pattrn.sln --configuration Release --no-build` because the .NET SDK required by `global.json` is not installed locally.

## Incomplete check

`Not completed` — `dotnet build Pattrn.sln --configuration Release --no-restore` because restore assets were missing and network/package access was not permitted.
