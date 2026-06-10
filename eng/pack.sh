#!/usr/bin/env bash
source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/common.sh"
OUT_DIR="${PACKAGE_OUTPUT:-$ROOT/artifacts/packages}"
mkdir -p "$OUT_DIR"
NUGET_CERT_REVOCATION_MODE=offline "$DOTNET_CMD" pack "$ROOT/src/Pattrn/Pattrn.csproj" --configuration Release --no-build --output "$OUT_DIR" -m:1 "$@"
NUGET_CERT_REVOCATION_MODE=offline "$DOTNET_CMD" pack "$ROOT/src/Pattrn.Strings/Pattrn.Strings.csproj" --configuration Release --no-build --output "$OUT_DIR" -m:1 "$@"
NUGET_CERT_REVOCATION_MODE=offline "$DOTNET_CMD" pack "$ROOT/src/Pattrn.DependencyInjection/Pattrn.DependencyInjection.csproj" --configuration Release --no-build --output "$OUT_DIR" -m:1 "$@"
NUGET_CERT_REVOCATION_MODE=offline "$DOTNET_CMD" pack "$ROOT/src/Pattrn.Routing/Pattrn.Routing.csproj" --configuration Release --no-build --output "$OUT_DIR" -m:1 "$@"
echo "Packages written to $OUT_DIR"
