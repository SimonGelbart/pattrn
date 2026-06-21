#!/usr/bin/env bash
set -euo pipefail

# Some containers set PLATFORM=linux/amd64, which MSBuild/BenchmarkDotNet misread as a solution platform.
unset PLATFORM || true

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

resolve_dotnet_cmd() {
  if [[ -n "${DOTNET:-}" ]]; then
    echo "$DOTNET"
    return
  fi

  local candidates=(
    "$ROOT/../dotnet10/dotnet"
    "$ROOT/../dotnet/dotnet"
    "dotnet"
  )

  for candidate in "${candidates[@]}"; do
    if [[ "$candidate" == "dotnet" ]]; then
      if command -v dotnet >/dev/null 2>&1; then
        command -v dotnet
        return
      fi
    elif [[ -x "$candidate" ]]; then
      echo "$candidate"
      return
    fi
  done

  echo "Could not find dotnet. Set DOTNET=/absolute/path/to/dotnet and retry." >&2
  return 1
}

DOTNET_CMD="$(resolve_dotnet_cmd)"
DOTNET_ROOT="$(cd "$(dirname "$DOTNET_CMD")" && pwd)"
export DOTNET_ROOT
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_NOLOGO=1
export DOTNET_NUGET_SIGNATURE_VERIFICATION=false
export NUGET_CERT_REVOCATION_MODE=offline

resolve_nuget_packages() {
  if [[ -n "${NUGET_BUNDLE_PACKAGES:-}" ]]; then
    echo "$NUGET_BUNDLE_PACKAGES"
    return
  fi

  local candidates=(
    "$ROOT/packages"
    "$ROOT/../nuget-bundle/packages"
  )

  for candidate in "${candidates[@]}"; do
    if [[ -d "$candidate" ]]; then
      echo "$candidate"
      return
    fi
  done

  echo "" >&2
  echo "Could not find the offline NuGet bundle packages directory." >&2
  echo "Set NUGET_BUNDLE_PACKAGES=/absolute/path/to/offline-nuget-bundle/packages and retry." >&2
  return 1
}

write_local_nuget_config() {
  local packages
  packages="$(resolve_nuget_packages)"
  local config="$ROOT/eng/NuGet.local.config"
  cat > "$config" <<XML
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="offline-bundle" value="$packages" />
  </packageSources>
</configuration>
XML
  echo "$config"
}
