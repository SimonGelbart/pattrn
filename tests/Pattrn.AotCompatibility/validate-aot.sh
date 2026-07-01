#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
PROJECT="$SCRIPT_DIR/Pattrn.AotCompatibility.csproj"
ARTIFACTS="$REPO_ROOT/artifacts/Pattrn.AotCompatibility"
HOST_OS="$(uname -s | tr '[:upper:]' '[:lower:]')"
HOST_ARCH="$(uname -m)"
DOTNET_CMD="${DOTNET_CMD:-dotnet}"

attempted=()
passed=()
failed=()
skipped=()

record_attempt() { attempted+=("$1"); }
record_pass() { passed+=("$1"); }
record_fail() { failed+=("$1: $2"); }
record_skip() { skipped+=("$1: $2"); }

can_execute_rid() {
  local rid="$1"
  case "$HOST_OS:$HOST_ARCH:$rid" in
    linux*:x86_64:linux-x64|linux*:amd64:linux-x64) return 0 ;;
    mingw*:*:win-x64|msys*:*:win-x64|cygwin*:*:win-x64) return 0 ;;
    *) return 1 ;;
  esac
}

exe_path() {
  local rid="$1"
  local mode="$2"
  local extension=""
  [[ "$rid" == win-* ]] && extension=".exe"
  printf '%s/%s/%s/Pattrn.AotCompatibility%s' "$ARTIFACTS" "$rid" "$mode" "$extension"
}

run_publish() {
  local rid="$1"
  local mode="$2"
  shift 2
  local label="$mode publish $rid"
  local output="$ARTIFACTS/$rid/$mode"
  local log="$output/publish.txt"
  record_attempt "$label"
  rm -rf "$output"
  mkdir -p "$output"

  set +e
  "$DOTNET_CMD" publish "$PROJECT" -c Release -r "$rid" -o "$output" "$@" >"$log" 2>&1
  local code=$?
  set -e

  if [[ $code -ne 0 ]]; then
    if grep -Eiq 'NU1301|NU1101|NU1801|proxy|Unable to load the service index|Failed to retrieve information' "$log"; then
      record_skip "$label" "package source or restore prerequisite unavailable; see $log"
      return 0
    fi
    if [[ "$mode" == "aot" ]] && grep -Eiq 'native aot|clang|linker|was not found|is required|Platform linker|Exit code 1' "$log"; then
      record_skip "$label" "Native AOT toolchain or RID prerequisite unavailable; see $log"
      return 0
    fi
    record_fail "$label" "dotnet publish failed; see $log"
    return 0
  fi

  if grep -E '(^|[^A-Z0-9])(IL[0-9]{4}|ILC[0-9]{4}|Trim analysis warning|AOT analysis warning)' "$log"; then
    record_fail "$label" "trim/AOT warnings were emitted; see $log"
    return 0
  fi

  record_pass "$label"
}

run_smoke() {
  local rid="$1"
  local mode="$2"
  local label="$mode smoke $rid"
  local exe
  exe="$(exe_path "$rid" "$mode")"
  record_attempt "$label"

  if [[ ! -f "$exe" ]]; then
    record_skip "$label" "published executable not present"
    return 0
  fi

  if ! can_execute_rid "$rid"; then
    record_skip "$label" "host cannot execute $rid binaries"
    return 0
  fi

  set +e
  "$exe"
  local code=$?
  set -e
  if [[ $code -eq 0 ]]; then
    record_pass "$label"
  else
    record_fail "$label" "smoke executable returned $code"
  fi
}

main() {
  rm -rf "$ARTIFACTS"
  mkdir -p "$ARTIFACTS"
  cd "$REPO_ROOT"

  for rid in linux-x64 win-x64; do
    run_publish "$rid" trimmed -p:PublishTrimmed=true -p:SelfContained=true
    run_smoke "$rid" trimmed
    run_publish "$rid" aot -p:PublishAot=true
    run_smoke "$rid" aot
  done

  echo "AOT compatibility validation summary"
  printf 'Attempted:\n'; printf '  - %s\n' "${attempted[@]}"
  printf 'Passed:\n'; ((${#passed[@]})) && printf '  - %s\n' "${passed[@]}" || printf '  - none\n'
  printf 'Skipped:\n'; ((${#skipped[@]})) && printf '  - %s\n' "${skipped[@]}" || printf '  - none\n'
  printf 'Failed:\n'; ((${#failed[@]})) && printf '  - %s\n' "${failed[@]}" || printf '  - none\n'

  ((${#failed[@]} == 0))
}

main "$@"
