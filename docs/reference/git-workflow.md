# Git Workflow

Repository history should be reviewable and easy to audit.

## Branches

Create branches from the intended base branch, usually `main`.

Use descriptive slash-separated names:

```text
docs/project-foundation
feat/<short-feature>
fix/<short-bug>
refactor/<short-area>
ci/<short-ci-topic>
test/<short-test-topic>
```

## Commits

Prefer focused commits and Conventional Commit messages:

```text
build: centralize pre-beta package metadata
docs: add project workflow foundation
docs: record product boundary decisions
docs: simplify roadmap milestones
fix: align package inspection with repository metadata
```

Each commit should represent one meaningful review unit.

Avoid mixing unrelated code, documentation, formatting, generated artifacts, and validation logs in one commit.

## Remote operations

Do not push branches, tags, or commits to a remote unless the maintainer explicitly asks.

Do not open pull requests unless the maintainer explicitly asks.

When a package is delivered as a zip, the zip may include local Git history for review, but that does not imply remote repository state changed.
