import json
import tempfile
import unittest
from pathlib import Path

from tools.benchmarks.compare_benchmarks import StructuralFailure, compare, main


def row(group="Core hot path", benchmark_type="PattrnIndexBenchmarks", method="Trie_MatchToSpan", scenario="case=small", mean="100.0", allocated_bytes=0, error=None, standard_deviation=None):
    return {
        "group": group,
        "benchmark_type": benchmark_type,
        "method": method,
        "scenario": scenario,
        "mean": mean,
        "error": error,
        "standard_deviation": standard_deviation,
        "allocated_bytes": allocated_bytes,
        "job": "job-a",
        "runtime": "net10.0",
    }


class CompareBenchmarksTests(unittest.TestCase):
    def write(self, directory: Path, name: str, rows):
        path = directory / name
        path.write_text(json.dumps({"benchmarks": rows}), encoding="utf-8")
        return path

    def test_matched_rows_latency_uncertainty_and_allocation_deltas(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            baseline = self.write(root, "baseline.json", [row(group="Detailed matching", mean="1,000.0", allocated_bytes=100, error="2.5", standard_deviation="5.5")])
            candidate = self.write(root, "candidate.json", [row(group="Detailed matching", mean="1100.0", allocated_bytes=150, error="3.5", standard_deviation="6.5")])
            result = compare(baseline, candidate)
            self.assertEqual("Pass", result["status"])
            compared = result["rows"][0]
            self.assertEqual(100.0, compared["latency"]["absolute_delta"])
            self.assertEqual(10.0, compared["latency"]["percentage_delta"])
            self.assertEqual(2.5, compared["latency"]["baseline_error"])
            self.assertEqual(6.5, compared["latency"]["candidate_standard_deviation"])
            self.assertEqual(50, compared["allocations"]["absolute_delta_bytes"])
            self.assertEqual(50.0, compared["allocations"]["percentage_delta"])

    def test_missing_baseline_and_candidate_rows_are_review(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            old = row(method="Old")
            shared = row(method="Shared")
            new = row(method="New")
            result = compare(self.write(root, "baseline.json", [old, shared]), self.write(root, "candidate.json", [shared, new]))
            self.assertEqual("Review", result["status"])
            changes = {r["identity"]["method"]: r["change"] for r in result["rows"]}
            self.assertEqual("baseline_only", changes["Old"])
            self.assertEqual("candidate_only", changes["New"])

    def test_duplicate_identity_is_structural_failure(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            baseline = self.write(root, "baseline.json", [row(), row()])
            candidate = self.write(root, "candidate.json", [row()])
            with self.assertRaises(StructuralFailure):
                compare(baseline, candidate)

    def test_zero_allocation_transition_notes(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            baseline = self.write(root, "baseline.json", [row(method="A", allocated_bytes=0), row(method="B", allocated_bytes=12), row(method="C", allocated_bytes=0)])
            candidate = self.write(root, "candidate.json", [row(method="A", allocated_bytes=8), row(method="B", allocated_bytes=0), row(method="C", allocated_bytes=0)])
            notes = {r["identity"]["method"]: r["allocations"]["note"] for r in compare(baseline, candidate)["rows"]}
            self.assertEqual("0B -> nonzero", notes["A"])
            self.assertEqual("nonzero -> 0B", notes["B"])
            self.assertEqual("unchanged zero-allocation row", notes["C"])

    def test_protected_hot_path_allocation_review_callout(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            result = compare(
                self.write(root, "baseline.json", [row(group="Core hot path", allocated_bytes=0)]),
                self.write(root, "candidate.json", [row(group="Core hot path", allocated_bytes=1)]),
            )
            self.assertEqual("Review", result["status"])
            self.assertEqual(1, result["summary"]["protected_hot_path_allocation_changes"])
            self.assertTrue(result["rows"][0]["protected_hot_path_allocation_change"])

    def test_malformed_json_and_unrecognized_structure_fail(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            bad = root / "bad.json"
            bad.write_text("{not json", encoding="utf-8")
            good = self.write(root, "good.json", [row()])
            with self.assertRaises(StructuralFailure):
                compare(bad, good)
            wrong = root / "wrong.json"
            wrong.write_text(json.dumps({"rows": []}), encoding="utf-8")
            with self.assertRaises(StructuralFailure):
                compare(wrong, good)

    def test_cli_writes_outputs_and_returns_zero_for_review(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            baseline = self.write(root, "baseline.json", [row(method="Old")])
            candidate = self.write(root, "candidate.json", [row(method="New")])
            output = root / "out"
            code = main(["--baseline", str(baseline), "--candidate", str(candidate), "--output", str(output)])
            self.assertEqual(0, code)
            self.assertTrue((output / "benchmark-comparison.md").exists())
            self.assertTrue((output / "benchmark-comparison.json").exists())
            data = json.loads((output / "benchmark-comparison.json").read_text(encoding="utf-8"))
            self.assertEqual("Review", data["status"])

    def test_cli_returns_nonzero_for_structural_failure(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            code = main(["--baseline", str(root / "missing.json"), "--candidate", str(root / "missing2.json"), "--output", str(root / "out")])
            self.assertNotEqual(0, code)


if __name__ == "__main__":
    unittest.main()
