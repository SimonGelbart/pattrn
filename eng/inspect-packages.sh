#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PACKAGE_DIR="${PACKAGE_OUTPUT:-$ROOT/artifacts/packages}"
shopt -s nullglob
packages=("$PACKAGE_DIR"/*.nupkg)
if [[ ${#packages[@]} -eq 0 ]]; then
  echo "No .nupkg files found in $PACKAGE_DIR. Run eng/pack.sh first." >&2
  exit 1
fi
for package in "${packages[@]}"; do
  echo "Inspecting $(basename "$package")"
  if ! unzip -p "$package" "*.nuspec" | grep -q '<license type="expression">MIT</license>'; then
    echo "Package $(basename "$package") does not contain MIT license metadata." >&2
    exit 1
  fi
  if unzip -p "$package" "*.nuspec" | grep -q '<repository type="git"'; then
    echo "Package $(basename "$package") advertises git repository metadata even though no repository URL exists yet." >&2
    exit 1
  fi
  if ! unzip -p "$package" "*.nuspec" | grep -q '<repository type="none"'; then
    echo "Package $(basename "$package") should use neutral repository metadata until a real repository URL exists." >&2
    exit 1
  fi
  unzip -l "$package" | grep -q 'README.md'
  unzip -l "$package" | grep -q 'icon.png'
  unzip -l "$package" | grep -q 'LICENSE.txt'
done
echo "Package inspection passed."
