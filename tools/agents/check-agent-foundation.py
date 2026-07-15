#!/usr/bin/env python3
"""Static consistency checks for agent-facing repository foundation."""
from __future__ import annotations

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[2]

REQUIRED_FILES = [
    "AGENTS.md",
    ".agents/AGENTS.md",
    ".agents/skills/repository-context/SKILL.md",
    ".agents/skills/validation-reporting/SKILL.md",
    ".codex/AGENTS.md",
    ".codex/config.toml",
    ".github/AGENTS.md",
    ".github/ISSUE_TEMPLATE/implementation.yml",
    ".agent-work.README.md",
]

REQUIRED_PHRASES = {
    "AGENTS.md": [
        "Proportionate repository discovery",
        "Do not treat issue readiness",
        ".agent-work/ is ignored",
    ],
    ".agents/skills/repository-context/SKILL.md": ["non-authoritative", "Do not edit files"],
    ".agents/skills/validation-reporting/SKILL.md": ["Passed", "Skipped", "Local validation is not CI evidence"],
    ".codex/config.toml": ["profile = \"pattrn-local\""],
    ".github/ISSUE_TEMPLATE/implementation.yml": [
        "does not grant authority",
        "Do not push, publish, tag, release, or open a pull request",
    ],
    ".gitignore": [".agent-work/"],
}

FORBIDDEN_ISSUE_PHRASES = [
    "open a focused PR linked to this issue",
]


def fail(message: str) -> None:
    print(f"Failed: {message}", file=sys.stderr)
    raise SystemExit(1)


def main() -> int:
    for relative in REQUIRED_FILES:
        if not (ROOT / relative).is_file():
            fail(f"missing required file: {relative}")

    for relative, phrases in REQUIRED_PHRASES.items():
        text = (ROOT / relative).read_text(encoding="utf-8")
        for phrase in phrases:
            if phrase not in text:
                fail(f"{relative} is missing required phrase: {phrase}")

    issue_text = (ROOT / ".github/ISSUE_TEMPLATE/implementation.yml").read_text(encoding="utf-8")
    for phrase in FORBIDDEN_ISSUE_PHRASES:
        if phrase in issue_text:
            fail(f"issue template still contains forbidden authority wording: {phrase}")

    print("Passed: agent foundation consistency checks")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
