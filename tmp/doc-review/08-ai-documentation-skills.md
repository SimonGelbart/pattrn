# Phase 8 — AI Documentation Skills Design (Design-Only)

## Scope and constraints

- This phase designs reusable documentation-management skills only.
- No product documentation was edited in this phase.
- No executable skill files were added in this phase.
- No files were moved, deleted, or archived in this phase.

## Skill 1 — Documentation inventory skill

### Purpose
Build a complete, repeatable inventory of documentation-like artifacts and ownership metadata before any audits or edits.

### Inputs
- Repository root path
- Canonical documentation path map (`docs/`, `README.md`, `CHANGELOG.md`, `docs.site.json`, workflows, package README sources)
- Prior `tmp/doc-review` phase artifacts

### Required reading
- `AGENTS.md`
- `docs/reference/repository-layout.md`
- `docs/reference/documentation-standards.md`
- `tmp/doc-review/INDEX.md`
- `tmp/doc-review/consolidated-findings.md`

### Steps
1. Enumerate documentation-like files and routing/configuration files.
2. Classify each artifact by purpose, audience, and likely canonical owner.
3. Record duplicates/near-duplicates, stale-risk candidates, and uncertain ownership.
4. Mark phase output scope as read-only inventory.

### Outputs
- Inventory report file for the phase
- Updated inventory summary in consolidated findings
- Any discovered questions for open-questions tracking

### Stop conditions
- All targeted documentation paths are inventoried.
- Each item has at least one ownership/category tag.
- No unresolved path-discovery gaps remain.

### Mode
Read-only.

### tmp/doc-review updates
- Add/update phase report (for example `01-inventory.md` style artifact)
- Update `consolidated-findings.md`
- Update `open-questions.md` when ambiguities appear
- Update `INDEX.md`
- Update `validation-log.md` when validation is intentionally skipped

## Skill 2 — Documentation consistency audit skill

### Purpose
Detect contradictions, redundancy, staleness, and ownership drift across current documentation.

### Inputs
- Inventory output
- Current docs corpus and governance references
- Prior contradictions/redundancy findings

### Required reading
- `docs/reference/project-profile.md`
- `docs/reference/documentation-standards.md`
- `docs/adr/README.md`
- Latest inventory phase artifact
- `tmp/doc-review/consolidated-findings.md`

### Steps
1. Compare equivalent claims across docs that describe the same behavior/policy.
2. Identify direct conflicts, soft conflicts, and duplicated ownership.
3. Classify issues by severity and remediation type (clarify, consolidate, defer).
4. Record decision-dependent items as explicit questions.

### Outputs
- Consistency audit report with contradiction/redundancy matrix
- Prioritized remediation candidates

### Stop conditions
- All major claim domains were cross-checked.
- Every flagged inconsistency has a proposed disposition.

### Mode
Read-only.

### tmp/doc-review updates
- Add/update consistency-audit phase artifact
- Update `consolidated-findings.md`
- Add decision-dependent items to `open-questions.md`
- Update `INDEX.md`
- Update `validation-log.md` if validation is skipped

## Skill 3 — Docs-vs-code gap analysis skill

### Purpose
Verify that current documentation claims match implemented and tested behavior.

### Inputs
- Consistency-audit output
- Source/tests/samples API and behavior evidence
- Accepted ADR decisions relevant to audited areas

### Required reading
- `docs/reference/validation.md`
- `docs/reference/architecture/testing-strategy.md`
- Relevant package/reference docs under audit
- Relevant source/tests/samples files
- Current consolidated findings

### Steps
1. Extract behavior/API claims from docs in scope.
2. Check each claim against source, tests, and samples.
3. Mark claims as aligned, underdocumented, or contradictory.
4. Capture concrete docs-vs-code remediation targets.

### Outputs
- Gap-analysis report with verified/unverified claim table
- Candidate implementation tasks tied to evidence

### Stop conditions
- All in-scope claims have evidence status.
- Remaining unknowns are logged as explicit questions or follow-ups.

### Mode
Read-only.

### tmp/doc-review updates
- Add/update docs-vs-code phase artifact
- Update `consolidated-findings.md` docs-vs-code section
- Update `open-questions.md` for unresolved evidence gaps
- Update `INDEX.md`
- Update `validation-log.md` if validation is skipped

## Skill 4 — Maintainer decision capture skill

### Purpose
Convert open questions into durable, traceable maintainer decisions that gate implementation work.

### Inputs
- Open questions list
- Maintainer responses
- Existing decision ledger

### Required reading
- `tmp/doc-review/open-questions.md`
- `tmp/doc-review/maintainer-decisions.md`
- `tmp/doc-review/consolidated-findings.md`
- `docs/reference/git-workflow.md`

### Steps
1. Map each maintainer response to one or more question IDs.
2. Assign decision IDs, scope, applicability, and supersedence links.
3. Mark corresponding questions as resolved/deferred.
4. Summarize implementation implications in consolidated findings.

### Outputs
- Updated `maintainer-decisions.md` decision table
- Resolved question statuses in `open-questions.md`
- Implementation-ready decision summary

### Stop conditions
- All currently blocking questions are resolved or explicitly deferred.
- Decision records are traceable to question IDs and affected files.

### Mode
Read-only (record-keeping only; no product-doc edits).

### tmp/doc-review updates
- Update `maintainer-decisions.md`
- Update `open-questions.md`
- Update `consolidated-findings.md`
- Update `INDEX.md`
- Update `validation-log.md` if validation is skipped

## Skill 5 — Documentation implementation skill

### Purpose
Apply approved documentation changes safely, with explicit non-destructive/destructive gates.

### Inputs
- Approved decisions
- Consolidated remediation list
- Target docs files and ownership map

### Required reading
- `AGENTS.md`
- `docs/reference/documentation-standards.md`
- `docs/reference/validation.md`
- `tmp/doc-review/maintainer-decisions.md`
- Latest implementation phase artifact

### Steps
1. Filter tasks to approved scope only.
2. Apply non-destructive edits first; defer gated destructive actions.
3. Keep canonical ownership boundaries intact while editing.
4. Record exact files changed/moved/deleted (if any approved).

### Outputs
- Implementation phase report
- Updated doc-review summaries reflecting completed decisions
- Updated validation records

### Stop conditions
- All approved-in-scope tasks are complete.
- Deferred/gated tasks are explicitly listed.
- No unapproved destructive action was performed.

### Mode
May edit after approval.

### tmp/doc-review updates
- Add/update implementation report (for example `07-implementation-and-validation.md`)
- Update `consolidated-findings.md` with implementation outcomes
- Update `open-questions.md` if new blockers emerge
- Update `INDEX.md`
- Update `validation-log.md` with actual validation outcomes

## Skill 6 — Validation/reporting skill

### Purpose
Produce honest, policy-aligned validation records and completion reporting for each phase.

### Inputs
- Change scope classification (artifact-only vs product-doc vs code-impacting)
- Executed command/workflow results
- Validation policy references

### Required reading
- `docs/reference/validation.md`
- `AGENTS.md` honesty standard
- Current phase report
- `tmp/doc-review/validation-log.md`

### Steps
1. Determine minimum expected validation set for the change type.
2. Run required checks when applicable, or explicitly mark as not run.
3. Record results with command/workflow, directory, summary, and skip reason.
4. Ensure phase report and validation log agree.

### Outputs
- Updated `validation-log.md`
- Validation section in the phase report

### Stop conditions
- Every relevant check is recorded as Passed/Failed/Not run/Not completed.
- No implied success is reported without execution evidence.

### Mode
Read-only unless part of an approved implementation phase.

### tmp/doc-review updates
- Update `validation-log.md`
- Update current phase report validation section
- Update `INDEX.md` status/summary if validation changes phase state

## Skill 7 — ADR/docs-site synchronization skill

### Purpose
Keep ADR index, ADR statuses, and docs-site manifest routing synchronized with agreed public-surface policy.

### Inputs
- ADR index and ADR files
- `docs.site.json` routes/excludes
- Discoverability policy decisions

### Required reading
- `docs/adr/README.md`
- `docs.site.json`
- `docs/reference/documentation-standards.md`
- Relevant maintainer decisions in `tmp/doc-review/maintainer-decisions.md`

### Steps
1. Audit accepted/superseded/deprecated ADR set and intended public relevance.
2. Compare ADR set with docs-site routed pages.
3. Flag missing/extra routes relative to policy.
4. Propose or apply approved synchronization updates.

### Outputs
- Sync audit results and/or implementation update summary
- Explicit drift risks and follow-up actions

### Stop conditions
- ADR index and docs-site policy alignment is explicit.
- Any intentional exceptions are documented.

### Mode
Read-only for audit; may edit after approval when synchronization changes are authorized.

### tmp/doc-review updates
- Add/update phase artifact covering ADR/docs-site sync findings
- Update `consolidated-findings.md` risk/outcome sections
- Update `open-questions.md` for policy ambiguities
- Update `INDEX.md`
- Update `validation-log.md` if validation is skipped
