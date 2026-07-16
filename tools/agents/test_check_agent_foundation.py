#!/usr/bin/env python3
"""Negative mutation tests for check-agent-foundation.py."""
from __future__ import annotations

import shutil
import subprocess
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
CHECKER = ROOT / "tools/agents/check-agent-foundation.py"


class FoundationCheckerNegativeTests(unittest.TestCase):
    def setUp(self) -> None:
        self.temporary = tempfile.TemporaryDirectory()
        self.root = Path(self.temporary.name) / "repository"
        shutil.copytree(
            ROOT,
            self.root,
            ignore=shutil.ignore_patterns(
                ".git", ".agent-work", "__pycache__", "node_modules", "bin", "obj", "artifacts"
            ),
        )

    def tearDown(self) -> None:
        self.temporary.cleanup()

    def assert_rejected(self, expected: str) -> None:
        result = subprocess.run(
            ["python3", str(CHECKER), "--root", str(self.root)],
            capture_output=True, text=True, check=False,
        )
        self.assertNotEqual(0, result.returncode)
        self.assertIn(expected, result.stderr)

    def replace(self, relative: str, old: str, new: str) -> None:
        path = self.root / relative
        path.write_text(path.read_text(encoding="utf-8").replace(old, new), encoding="utf-8")

    def test_invalid_toml(self) -> None:
        (self.root / ".codex/config.toml").write_text("invalid = [", encoding="utf-8")
        self.assert_rejected("invalid TOML")

    def test_invalid_yaml(self) -> None:
        (self.root / ".github/ISSUE_TEMPLATE/implementation.yml").write_text("body: [", encoding="utf-8")
        self.assert_rejected("invalid issue-template YAML")

    def test_missing_skill_metadata(self) -> None:
        self.replace(".agents/skills/repository-context/SKILL.md", "---\nname:", "name:")
        self.assert_rejected("missing metadata front matter")

    def test_forbidden_profile(self) -> None:
        with (self.root / ".codex/config.toml").open("a", encoding="utf-8") as file:
            file.write("\nprofile = \"local\"\n")
        self.assert_rejected("forbidden project-local profile")

    def test_malformed_agent(self) -> None:
        self.replace(".codex/agents/local-implementation.toml", "description =", "summary =")
        self.assert_rejected("missing non-empty field: description")

    def test_validation_vocabulary_drift(self) -> None:
        self.replace("docs/reference/validation.md", "**Passed:**", "**Successful:**")
        self.assert_rejected("canonical validation vocabulary has drifted")

    def test_authority_conflation(self) -> None:
        path = self.root / ".github/ISSUE_TEMPLATE/implementation.yml"
        with path.open("a", encoding="utf-8") as file:
            file.write("\n# branch creation, commits, pushes are remote actions\n")
        self.assert_rejected("conflates local Git mechanics")

    def test_missing_agent_work_ignore(self) -> None:
        self.replace(".gitignore", ".agent-work/", ".temporary-agent-work/")
        self.assert_rejected("ignore rule is missing")


if __name__ == "__main__":
    unittest.main()
