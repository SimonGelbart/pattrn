#!/usr/bin/env bash
set -euo pipefail
source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/common.sh"

PROJECT="$ROOT/benchmarks/Pattrn.Benchmarks/Pattrn.Benchmarks.csproj"
RESULTS_DIR="$ROOT/BenchmarkDotNet.Artifacts/results"
DOCS_RESULTS_DIR="$ROOT/docs/benchmark-results"
LATEST_REPORT="$DOCS_RESULTS_DIR/latest.md"
MODE="${BENCHMARK_MODE:-smoke}"

usage() {
  cat <<USAGE
Usage:
  ./eng/benchmark.sh [BenchmarkDotNet args]

Defaults to a real out-of-process BenchmarkDotNet smoke run that completes in constrained sandboxes.
Set BENCHMARK_MODE=dry, BENCHMARK_MODE=short, or BENCHMARK_MODE=full to run broader suites, or pass BenchmarkDotNet arguments directly.

Examples:
  ./eng/benchmark.sh
  BENCHMARK_MODE=full ./eng/benchmark.sh
  ./eng/benchmark.sh --filter '*WideFanOut*' --job short
USAGE
}

if [[ "${1:-}" == "-h" || "${1:-}" == "--help" ]]; then
  usage
  exit 0
fi

ensure_root_nuget_bundle_link || true
CONFIG="$(write_local_nuget_config)"
mkdir -p "$DOCS_RESULTS_DIR"

# Keep NuGet package cache local to the repo for reproducible offline runs unless the caller overrides it.
export NUGET_PACKAGES="${NUGET_PACKAGES:-$ROOT/.nuget/packages}"
mkdir -p "$NUGET_PACKAGES"

RESTORE_ARGS=(--configfile "$CONFIG" /p:NuGetAudit=false)
BUILD_ARGS=(--configuration Release --no-restore -m:1)

if [[ $# -gt 0 ]]; then
  BDN_ARGS=("$@")
  MODE_DESCRIPTION="custom"
elif [[ "$MODE" == "full" ]]; then
  BDN_ARGS=(--filter "*")
  MODE_DESCRIPTION="full/default"
elif [[ "$MODE" == "dry" ]]; then
  BDN_ARGS=(--filter "*" --job dry)
  MODE_DESCRIPTION="dry/all"
elif [[ "$MODE" == "short" ]]; then
  BDN_ARGS=(--filter "*" --job short --warmupCount "${BENCHMARK_WARMUP_COUNT:-3}" --iterationCount "${BENCHMARK_ITERATION_COUNT:-3}")
  MODE_DESCRIPTION="short/all"
else
  BDN_ARGS=(--filter "*PattrnIndexBenchmarks.Trie_MatchToSpan*" --job dry)
  MODE_DESCRIPTION="smoke"
fi

printf 'Using dotnet: %s\n' "$DOTNET_CMD"
"$DOTNET_CMD" --info
printf '\nRestoring benchmark project from offline NuGet bundle...\n'
"$DOTNET_CMD" restore "$PROJECT" "${RESTORE_ARGS[@]}"
printf '\nBuilding benchmark project in Release...\n'
"$DOTNET_CMD" build "$PROJECT" "${BUILD_ARGS[@]}"
printf '\nRunning BenchmarkDotNet (%s mode)...\n' "$MODE_DESCRIPTION"
"$DOTNET_CMD" run --configuration Release --no-build --project "$PROJECT" -- "${BDN_ARGS[@]}"

latest_source=""
if compgen -G "$RESULTS_DIR/*-report-github.md" > /dev/null; then
  latest_source="$(ls -t "$RESULTS_DIR"/*-report-github.md | head -n 1)"
elif compgen -G "$RESULTS_DIR/*-report.md" > /dev/null; then
  latest_source="$(ls -t "$RESULTS_DIR"/*-report.md | head -n 1)"
fi

if [[ -z "$latest_source" ]]; then
  echo "BenchmarkDotNet completed, but no markdown report was found under $RESULTS_DIR" >&2
  exit 1
fi

{
  echo "# Latest Benchmark Results"
  echo
  echo "- Generated UTC: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
  echo "- Mode: $MODE_DESCRIPTION"
  echo "- Dotnet: $DOTNET_CMD"
  echo "- BenchmarkDotNet arguments: ${BDN_ARGS[*]:-(default)}"
  echo
  cat "$latest_source"
} > "$LATEST_REPORT"

printf '\nLatest benchmark report written to %s\n' "$LATEST_REPORT"
printf 'Raw BenchmarkDotNet artifacts are under %s\n' "$ROOT/BenchmarkDotNet.Artifacts"
