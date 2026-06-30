#!/usr/bin/env python3
"""Compare two grouped benchmark summary artifacts."""

from __future__ import annotations

import argparse
import json
import math
import sys
from pathlib import Path
from typing import Any

IDENTITY_FIELDS = ("group", "benchmark_type", "method", "scenario")
REVIEW = "Review"
PASS = "Pass"
FAILED = "Failed"


class StructuralFailure(Exception):
    """Raised when comparison inputs cannot be interpreted safely."""


def _read_grouped_results(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise StructuralFailure(f"Input file does not exist: {path}")
    if not path.is_file():
        raise StructuralFailure(f"Input path is not a file: {path}")
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except OSError as exc:
        raise StructuralFailure(f"Could not read input file {path}: {exc}") from exc
    except json.JSONDecodeError as exc:
        raise StructuralFailure(f"Malformed JSON in {path}: {exc}") from exc


def _require_benchmarks(data: dict[str, Any], label: str) -> list[dict[str, Any]]:
    if not isinstance(data, dict) or not isinstance(data.get("benchmarks"), list):
        raise StructuralFailure(f"{label} is not a recognized grouped-results structure")
    rows = data["benchmarks"]
    if not all(isinstance(row, dict) for row in rows):
        raise StructuralFailure(f"{label} contains non-object benchmark rows")
    return rows


def _identity(row: dict[str, Any]) -> tuple[str, str, str, str]:
    values: list[str] = []
    for field in IDENTITY_FIELDS:
        value = row.get(field)
        if value is None:
            value = ""
        if not isinstance(value, str):
            value = str(value)
        values.append(value)
    return tuple(values)  # type: ignore[return-value]


def _index_rows(rows: list[dict[str, Any]], label: str) -> dict[tuple[str, str, str, str], dict[str, Any]]:
    indexed: dict[tuple[str, str, str, str], dict[str, Any]] = {}
    for row in rows:
        key = _identity(row)
        if key in indexed:
            raise StructuralFailure(
                f"{label} contains duplicate/ambiguous row identity: "
                f"group={key[0]!r}, benchmark_type={key[1]!r}, method={key[2]!r}, scenario={key[3]!r}"
            )
        indexed[key] = row
    return indexed


def _number(value: Any, field: str, key: tuple[str, str, str, str]) -> float | None:
    if value in (None, ""):
        return None
    if isinstance(value, bool):
        raise StructuralFailure(f"Required numeric value {field} for {key} is boolean")
    if isinstance(value, (int, float)):
        result = float(value)
    elif isinstance(value, str):
        text = value.strip().replace(",", "")
        try:
            result = float(text)
        except ValueError as exc:
            raise StructuralFailure(f"Required numeric value {field} for {key} cannot be interpreted: {value!r}") from exc
    else:
        raise StructuralFailure(f"Required numeric value {field} for {key} cannot be interpreted: {value!r}")
    if not math.isfinite(result):
        raise StructuralFailure(f"Required numeric value {field} for {key} is not finite: {value!r}")
    return result


def _bytes(value: Any, field: str, key: tuple[str, str, str, str]) -> int | None:
    parsed = _number(value, field, key)
    if parsed is None:
        return None
    return int(parsed)


def _pct(delta: float, baseline: float) -> float | None:
    if baseline == 0:
        return None
    return (delta / baseline) * 100.0


def _protected(row: dict[str, Any]) -> bool:
    haystack = " ".join(str(row.get(k) or "") for k in ("group", "benchmark_type", "method", "scenario", "display_info"))
    lower = haystack.lower()
    return (
        row.get("group") == "Core hot path"
        or ("routeindex_matchpresplit" in lower and "span" in lower)
    )


def _allocation_note(base: int | None, cand: int | None) -> str | None:
    if base == 0 and cand and cand != 0:
        return "0B -> nonzero"
    if base and base != 0 and cand == 0:
        return "nonzero -> 0B"
    if base == 0 and cand == 0:
        return "unchanged zero-allocation row"
    return None


def compare(baseline_path: Path, candidate_path: Path) -> dict[str, Any]:
    baseline = _index_rows(_require_benchmarks(_read_grouped_results(baseline_path), "baseline"), "baseline")
    candidate = _index_rows(_require_benchmarks(_read_grouped_results(candidate_path), "candidate"), "candidate")
    rows: list[dict[str, Any]] = []
    protected_callouts: list[dict[str, Any]] = []

    for key in sorted(set(baseline) | set(candidate)):
        base = baseline.get(key)
        cand = candidate.get(key)
        row: dict[str, Any] = {"identity": dict(zip(IDENTITY_FIELDS, key)), "status": PASS}
        if base is None:
            row.update({"status": REVIEW, "change": "candidate_only", "message": "Row is present only in candidate."})
            rows.append(row)
            continue
        if cand is None:
            row.update({"status": REVIEW, "change": "baseline_only", "message": "Row is present only in baseline."})
            rows.append(row)
            continue

        base_mean = _number(base.get("mean"), "mean", key)
        cand_mean = _number(cand.get("mean"), "mean", key)
        base_alloc = _bytes(base.get("allocated_bytes"), "allocated_bytes", key)
        cand_alloc = _bytes(cand.get("allocated_bytes"), "allocated_bytes", key)
        if base_mean is None or cand_mean is None:
            raise StructuralFailure(f"Required numeric mean for matched row cannot be missing: {key}")
        if base_alloc is None or cand_alloc is None:
            raise StructuralFailure(f"Required numeric allocated_bytes for matched row cannot be missing: {key}")

        latency_delta = cand_mean - base_mean
        allocation_delta = cand_alloc - base_alloc
        alloc_note = _allocation_note(base_alloc, cand_alloc)
        row.update({
            "change": "matched",
            "job": {"baseline": base.get("job"), "candidate": cand.get("job")},
            "runtime": {"baseline": base.get("runtime"), "candidate": cand.get("runtime")},
            "latency": {
                "baseline_mean": base_mean,
                "candidate_mean": cand_mean,
                "absolute_delta": latency_delta,
                "percentage_delta": _pct(latency_delta, base_mean),
                "baseline_error": _number(base.get("error"), "error", key),
                "candidate_error": _number(cand.get("error"), "error", key),
                "baseline_standard_deviation": _number(base.get("standard_deviation"), "standard_deviation", key),
                "candidate_standard_deviation": _number(cand.get("standard_deviation"), "standard_deviation", key),
            },
            "allocations": {
                "baseline_allocated_bytes": base_alloc,
                "candidate_allocated_bytes": cand_alloc,
                "absolute_delta_bytes": allocation_delta,
                "percentage_delta": _pct(float(allocation_delta), float(base_alloc)) if base_alloc != 0 else None,
                "note": alloc_note,
            },
        })
        if _protected(base) and allocation_delta != 0:
            callout = {"identity": row["identity"], "baseline_allocated_bytes": base_alloc, "candidate_allocated_bytes": cand_alloc, "absolute_delta_bytes": allocation_delta, "status": REVIEW}
            protected_callouts.append(callout)
            row["status"] = REVIEW
            row["protected_hot_path_allocation_change"] = True
        rows.append(row)

    status = REVIEW if any(r["status"] == REVIEW for r in rows) else PASS
    return {
        "status": status,
        "caveat": "Historical artifact comparison only; this does not replace BenchmarkDotNet same-run baselines and does not claim formal statistical significance.",
        "inputs": {"baseline": str(baseline_path), "candidate": str(candidate_path)},
        "summary": {"matched": sum(1 for r in rows if r.get("change") == "matched"), "baseline_only": sum(1 for r in rows if r.get("change") == "baseline_only"), "candidate_only": sum(1 for r in rows if r.get("change") == "candidate_only"), "protected_hot_path_allocation_changes": len(protected_callouts)},
        "rows": rows,
        "protected_hot_path_allocation_changes": protected_callouts,
    }


def _fmt(value: Any, suffix: str = "") -> str:
    if value is None:
        return "n/a"
    if isinstance(value, float):
        return f"{value:.3f}{suffix}"
    return f"{value}{suffix}"


def markdown(result: dict[str, Any]) -> str:
    lines = ["# Benchmark Comparison", "", result["caveat"], "", f"Status: **{result['status']}**", "", "## Summary", ""]
    for key, value in result["summary"].items():
        lines.append(f"- {key.replace('_', ' ').title()}: {value}")
    if result["protected_hot_path_allocation_changes"]:
        lines += ["", "## Protected hot-path allocation changes", "", "| Group | Benchmark type | Method | Scenario | Baseline | Candidate | Delta | Status |", "|---|---|---|---|---:|---:|---:|---|"]
        for item in result["protected_hot_path_allocation_changes"]:
            ident = item["identity"]
            lines.append(f"| {ident['group']} | {ident['benchmark_type']} | {ident['method']} | {ident['scenario']} | {item['baseline_allocated_bytes']} | {item['candidate_allocated_bytes']} | {item['absolute_delta_bytes']} | {item['status']} |")
    lines += ["", "## Rows", "", "| Status | Change | Group | Benchmark type | Method | Scenario | Mean delta | Mean delta % | Allocation delta | Allocation note |", "|---|---|---|---|---|---|---:|---:|---:|---|"]
    for row in result["rows"]:
        ident = row["identity"]
        lat = row.get("latency", {})
        alloc = row.get("allocations", {})
        lines.append(f"| {row['status']} | {row.get('change', '')} | {ident['group']} | {ident['benchmark_type']} | {ident['method']} | {ident['scenario']} | {_fmt(lat.get('absolute_delta'))} | {_fmt(lat.get('percentage_delta'), '%')} | {_fmt(alloc.get('absolute_delta_bytes'))} | {alloc.get('note') or ''} |")
    lines += ["", "Review means human review is recommended; it is not an automatic blocking failure.", ""]
    return "\n".join(lines)


def write_outputs(result: dict[str, Any], output: Path) -> None:
    output.mkdir(parents=True, exist_ok=True)
    (output / "benchmark-comparison.json").write_text(json.dumps(result, indent=2, sort_keys=True) + "\n", encoding="utf-8")
    (output / "benchmark-comparison.md").write_text(markdown(result), encoding="utf-8")


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Compare two grouped benchmark result artifacts.")
    parser.add_argument("--baseline", required=True, type=Path)
    parser.add_argument("--candidate", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args(argv)
    try:
        result = compare(args.baseline, args.candidate)
        write_outputs(result, args.output)
        return 0
    except StructuralFailure as exc:
        sys.stderr.write(f"{FAILED}: {exc}\n")
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
