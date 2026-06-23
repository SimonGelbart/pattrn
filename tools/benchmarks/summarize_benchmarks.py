#!/usr/bin/env python3
"""Generate grouped benchmark summaries from BenchmarkDotNet JSON reports."""

from __future__ import annotations

import argparse
import json
import os
from collections import Counter
from dataclasses import dataclass, asdict
from pathlib import Path
from typing import Any

GROUPS = [
    "Core hot path",
    "Detailed matching",
    "Routing preview",
    "Builder / validation",
    "String helpers",
    "Unclassified",
]


def text(value: Any) -> str:
    return "" if value is None else str(value)


def find_key(data: dict[str, Any], *keys: str) -> Any:
    for key in keys:
        if key in data:
            return data[key]
    lowered = {k.lower(): v for k, v in data.items()}
    for key in keys:
        if key.lower() in lowered:
            return lowered[key.lower()]
    return None


def format_number(value: Any) -> str | None:
    if value is None:
        return None
    if isinstance(value, (int, float)):
        return f"{value:,.3f}" if isinstance(value, float) else f"{value:,}"
    return text(value)


def flatten_parameters(value: Any) -> str | None:
    if value in (None, "", []):
        return None
    if isinstance(value, dict):
        items = []
        for key, val in value.items():
            if isinstance(val, dict) and "Value" in val:
                val = val["Value"]
            items.append(f"{key}={val}")
        return ", ".join(items) if items else None
    if isinstance(value, list):
        items = []
        for item in value:
            if isinstance(item, dict):
                name = find_key(item, "Name", "Parameter")
                val = find_key(item, "Value")
                items.append(f"{name}={val}" if name is not None else text(val))
            else:
                items.append(text(item))
        return ", ".join(x for x in items if x) or None
    return text(value)


@dataclass
class BenchmarkRow:
    benchmark_type: str
    method: str
    scenario: str | None
    display_info: str | None
    mean: str | None
    error: str | None
    standard_deviation: str | None
    allocated_bytes: int | None
    allocation_summary: str | None
    allocation_columns: dict[str, Any]
    job: str | None
    runtime: str | None
    source_report: str
    group: str


def extract_rows(report: Path, root: dict[str, Any]) -> list[BenchmarkRow]:
    benches = find_key(root, "Benchmarks") or []
    rows: list[BenchmarkRow] = []
    for bench in benches:
        if not isinstance(bench, dict):
            continue
        stats = find_key(bench, "Statistics") or {}
        memory = find_key(bench, "Memory") or {}
        benchmark_type = text(find_key(bench, "Type", "Class", "BenchmarkType"))
        method = text(find_key(bench, "Method", "MethodTitle"))
        display = find_key(bench, "DisplayInfo", "FullName")
        scenario = flatten_parameters(find_key(bench, "Parameters", "ParameterInstances"))
        job = find_key(bench, "Job", "JobDisplayInfo")
        runtime = find_key(bench, "Runtime", "RuntimeMoniker")
        if isinstance(job, dict):
            job = find_key(job, "DisplayInfo", "Id") or json.dumps(job, sort_keys=True)
        if isinstance(runtime, dict):
            runtime = find_key(runtime, "Name", "DisplayName") or json.dumps(runtime, sort_keys=True)
        allocated = find_key(memory, "BytesAllocatedPerOperation", "AllocatedBytes", "Allocated")
        try:
            allocated_int = int(float(allocated)) if allocated is not None else None
        except (TypeError, ValueError):
            allocated_int = None
        allocation_summary = find_key(memory, "Allocated", "AllocatedMemory")
        rows.append(BenchmarkRow(
            benchmark_type=benchmark_type,
            method=method,
            scenario=scenario,
            display_info=text(display) or None,
            mean=format_number(find_key(stats, "Mean")),
            error=format_number(find_key(stats, "StandardError", "Error")),
            standard_deviation=format_number(find_key(stats, "StandardDeviation", "StdDev")),
            allocated_bytes=allocated_int,
            allocation_summary=text(allocation_summary) if allocation_summary is not None else None,
            allocation_columns=memory if isinstance(memory, dict) else {},
            job=text(job) or None,
            runtime=text(runtime) or None,
            source_report=str(report),
            group="Unclassified",
        ))
    return rows


def classify(row: BenchmarkRow) -> str:
    haystack = " ".join([row.benchmark_type, row.method, row.scenario or "", row.display_info or ""]).lower()
    if "routingbenchmarks" in haystack or "route" in haystack:
        return "Routing preview"
    if "builderbenchmarks" in haystack or "diagnostic" in haystack or "validation" in haystack or "build" in haystack:
        return "Builder / validation"
    if "string" in haystack or "split" in haystack or "normaliz" in haystack:
        return "String helpers"
    if "detailed" in haystack or "capture" in haystack or "duplicate" in haystack or "matchtoarray" in haystack:
        return "Detailed matching"
    if "pattrnindexbenchmarks" in haystack and ("matchtospan" in haystack or "upperbound" in haystack):
        return "Core hot path"
    return "Unclassified"


def zero_alloc(rows: list[BenchmarkRow]) -> str:
    if not rows:
        return "Unknown"
    known = [r for r in rows if r.allocated_bytes is not None]
    if len(known) != len(rows):
        return "Unknown"
    return "Pass" if all(r.allocated_bytes == 0 for r in known) else "Review"


def guardrails(rows: list[BenchmarkRow]) -> list[dict[str, str]]:
    def matches(*needles: str) -> list[BenchmarkRow]:
        return [r for r in rows if all(n.lower() in " ".join([r.benchmark_type, r.method, r.scenario or "", r.display_info or ""]).lower() for n in needles)]
    core = [r for r in rows if r.group == "Core hot path" and ("MatchToSpan" in r.method or "UpperBound" in r.method)]
    presplit = matches("RouteIndex_MatchPreSplit", "Span")
    non_hot = [r for r in rows if r.group in {"Detailed matching", "Routing preview", "String helpers"}]
    return [
        {"guardrail": "Core span hot paths allocate 0 B", "status": zero_alloc(core), "evidence": f"rows matched: {len(core)}"},
        {"guardrail": "Pre-split route matching allocates 0 B", "status": zero_alloc(presplit), "evidence": f"rows matched: {len(presplit)}"},
        {"guardrail": "Non-hot-path allocation is isolated", "status": "Review" if non_hot else "Unknown", "evidence": f"detailed/string/parsing rows: {len(non_hot)}"},
    ]


def markdown(rows: list[BenchmarkRow], guards: list[dict[str, str]], metadata: dict[str, str]) -> str:
    counts = Counter(r.group for r in rows)
    lines = ["# Benchmark Summary", "", "Benchmark artifacts from this workflow run are the benchmark source of truth for this commit.", "", "## Run metadata", ""]
    for label, key in [("Repository", "repository"), ("Ref", "ref"), ("SHA", "sha"), ("Run ID", "run_id"), ("Run attempt", "run_attempt"), ("Filter", "benchmark_filter"), ("Strategy", "benchmark_strategy")]:
        if metadata.get(key):
            lines.append(f"- {label}: `{metadata[key]}`")
    lines += ["", "## Row counts by group", "", "| Group | Rows |", "|---|---:|"]
    for group in GROUPS:
        lines.append(f"| {group} | {counts[group]} |")
    lines += ["", "## Guardrail check", "", "| Guardrail | Status | Evidence |", "|---|---|---|"]
    for g in guards:
        lines.append(f"| {g['guardrail']} | {g['status']} | {g['evidence']} |")
    lines += ["", f"Unclassified rows: **{counts['Unclassified']}**.", ""]
    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--artifacts", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    reports = sorted((args.artifacts / "results").glob("*-report-full-compressed.json"))
    if not reports:
        raise SystemExit(f"No BenchmarkDotNet full compressed JSON report found under {args.artifacts / 'results'}")
    rows: list[BenchmarkRow] = []
    for report in reports:
        rows.extend(extract_rows(report, json.loads(report.read_text(encoding="utf-8"))))
    if not rows:
        raise SystemExit("BenchmarkDotNet JSON reports were found, but no benchmark rows could be parsed")
    for row in rows:
        row.group = classify(row)
    metadata = {
        "repository": os.environ.get("GITHUB_REPOSITORY", ""),
        "ref": os.environ.get("GITHUB_REF", ""),
        "sha": os.environ.get("GITHUB_SHA", ""),
        "run_id": os.environ.get("GITHUB_RUN_ID", ""),
        "run_attempt": os.environ.get("GITHUB_RUN_ATTEMPT", ""),
        "benchmark_filter": os.environ.get("BENCHMARK_FILTER", ""),
        "benchmark_strategy": os.environ.get("BENCHMARK_STRATEGY", ""),
    }
    summary_dir = args.output / "summaries"
    summary_dir.mkdir(parents=True, exist_ok=True)
    guards = guardrails(rows)
    result = {"metadata": metadata, "reports": [str(p) for p in reports], "groups": {group: Counter(r.group for r in rows)[group] for group in GROUPS}, "guardrails": guards, "benchmarks": [asdict(r) for r in rows]}
    (summary_dir / "grouped-results.json").write_text(json.dumps(result, indent=2, sort_keys=True) + "\n", encoding="utf-8")
    (summary_dir / "grouped-summary.md").write_text(markdown(rows, guards, metadata), encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
