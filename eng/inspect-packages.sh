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
  nuspec="$(unzip -p "$package" "*.nuspec")"
  if ! grep -q '<license type="expression">MIT</license>' <<<"$nuspec"; then
    echo "Package $(basename "$package") does not contain MIT license metadata." >&2
    exit 1
  fi
  if ! grep -q '<repository type="git" url="https://github.com/SimonGelbart/pattrn"' <<<"$nuspec"; then
    echo "Package $(basename "$package") should publish the Git repository metadata." >&2
    exit 1
  fi
  unzip -l "$package" | grep -q 'README.md'
  unzip -l "$package" | grep -q 'icon.png'
  unzip -l "$package" | grep -q 'LICENSE.txt'
done
echo "Package inspection passed."
