#!/usr/bin/env python3
"""Validate the repository-specific agent foundation without third-party packages."""
from __future__ import annotations

import argparse
import re
import subprocess
import sys
import tomllib
from pathlib import Path

SKILLS = (
    ".agents/skills/repository-context/SKILL.md",
    ".agents/skills/validation-reporting/SKILL.md",
)
AGENT_FIELDS = ("name", "description", "developer_instructions")
VALIDATION_STATES = {"Passed", "Failed", "Not run", "Not completed", "Skipped"}


class CheckError(Exception):
    """A deterministic foundation contract failure."""


def require(condition: bool, message: str) -> None:
    if not condition:
        raise CheckError(message)


def read(root: Path, relative: str) -> str:
    path = root / relative
    require(path.is_file(), f"missing required file: {relative}")
    return path.read_text(encoding="utf-8")


def parse_toml(path: Path) -> dict[str, object]:
    try:
        return tomllib.loads(path.read_text(encoding="utf-8"))
    except (OSError, tomllib.TOMLDecodeError) as error:
        raise CheckError(f"invalid TOML in {path.relative_to(path.parents[2])}: {error}") from error


def parse_skill_metadata(text: str, relative: str) -> dict[str, str]:
    match = re.match(r"\A---\n(.*?)\n---\n", text, re.DOTALL)
    require(match is not None, f"{relative} is missing metadata front matter")
    metadata: dict[str, str] = {}
    for line in match.group(1).splitlines():
        key, separator, value = line.partition(":")
        require(bool(separator and key.strip() and value.strip()), f"invalid metadata in {relative}: {line}")
        metadata[key.strip()] = value.strip()
    for field in ("name", "description"):
        require(bool(metadata.get(field)), f"{relative} is missing metadata field: {field}")
    return metadata


def parse_issue_yaml(path: Path) -> None:
    script = "require 'yaml'; value = YAML.safe_load_file(ARGV[0], aliases: false); abort unless value.is_a?(Hash) && value['body'].is_a?(Array)"
    result = subprocess.run(
        ["ruby", "-e", script, str(path)], capture_output=True, text=True, check=False
    )
    require(result.returncode == 0, f"invalid issue-template YAML: {(result.stderr or result.stdout).strip()}")


def check(root: Path) -> None:
    for relative in (
        "AGENTS.md", ".agents/AGENTS.md", ".codex/AGENTS.md", ".github/AGENTS.md",
        ".agent-work.README.md", ".gitignore", "docs/reference/validation.md",
    ):
        read(root, relative)

    config_path = root / ".codex/config.toml"
    config = parse_toml(config_path)
    require("profile" not in config, ".codex/config.toml contains forbidden project-local profile selection")
    require("profiles" not in config, ".codex/config.toml contains forbidden [profiles.*] configuration")

    agent_dir = root / ".codex/agents"
    markdown_agents = sorted(agent_dir.glob("*.md"))
    if markdown_agents:
        raise CheckError(f"Markdown custom-agent files are forbidden: {markdown_agents[0].name}")
    agents = sorted(agent_dir.glob("*.toml"))
    require(len(agents) == 3, "exactly three local custom-agent TOML files are required")
    for path in agents:
        definition = parse_toml(path)
        for field in AGENT_FIELDS:
            require(isinstance(definition.get(field), str) and bool(definition[field].strip()),
                    f"{path.name} is missing non-empty field: {field}")
        instructions = str(definition["developer_instructions"])
        require("AGENTS.md" in instructions, f"{path.name} must point to repository instructions")

    skill_names = set()
    for relative in SKILLS:
        metadata = parse_skill_metadata(read(root, relative), relative)
        skill_names.add(metadata["name"])
    require(skill_names == {"repository-context", "validation-reporting"}, "skill names are unstable or duplicated")

    issue_path = root / ".github/ISSUE_TEMPLATE/implementation.yml"
    parse_issue_yaml(issue_path)
    issue = read(root, ".github/ISSUE_TEMPLATE/implementation.yml").lower()
    for required in ("readiness", "implementation", "local git", "validation", "remote"):
        require(required in issue, f"issue template does not distinguish required concept: {required}")
    conflations = (
        "branch creation, commits, pushes are remote actions",
        "create or switch branches, commit, push, publish",
        "branch, commit, push, publication, pull-request",
    )
    require(not any(phrase in issue for phrase in conflations),
            "issue template conflates local Git mechanics with remote authority")

    ignore_lines = {line.strip() for line in read(root, ".gitignore").splitlines()}
    require(".agent-work/" in ignore_lines, ".agent-work/ ignore rule is missing")
    temporary = read(root, ".agent-work.README.md").lower()
    require("non-authoritative" in temporary and "do not commit" in temporary,
            ".agent-work/ must remain ignored, temporary, and non-authoritative")

    validation = read(root, "docs/reference/validation.md")
    states = set(re.findall(r"^- \*\*(Passed|Failed|Not run|Not completed|Skipped):\*\*", validation, re.MULTILINE))
    require(states == VALIDATION_STATES, "canonical validation vocabulary has drifted")
    for relative in ("AGENTS.md", ".agents/skills/validation-reporting/SKILL.md"):
        supporting = read(root, relative)
        require("docs/reference/validation.md" in supporting,
                f"{relative} must point to the canonical validation reference")
        repeated = sum(f"`{state}`" in supporting for state in VALIDATION_STATES)
        require(repeated < len(VALIDATION_STATES), f"{relative} duplicates the canonical validation vocabulary")

    codex_guidance = read(root, ".codex/AGENTS.md")
    require("local Codex clients" in codex_guidance and "Do not claim that Codex Cloud loads" in codex_guidance,
            ".codex guidance must accurately describe the runtime surface")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[2])
    args = parser.parse_args()
    try:
        check(args.root.resolve())
    except CheckError as error:
        print(f"Failed: {error}", file=sys.stderr)
        return 1
    print("Passed: agent foundation consistency checks")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
