# AI Documentation Management

This reference defines a reusable workflow for AI-assisted documentation review and maintenance in this repository.

Use it to run repeatable phases, keep decisions traceable, and preserve honest validation reporting.

## Workflow goals

- Keep documentation durable, canonical, and aligned with code and accepted ADRs.
- Separate read-only analysis phases from approved implementation phases.
- Record maintainer decisions explicitly before destructive actions.
- Preserve transparent validation records.

## Skill templates

### 1) Documentation inventory skill

**Purpose**  
Build a complete inventory of documentation-like artifacts and ownership metadata before audits or edits.

**Inputs**
- Repository root path
- Canonical documentation path map (`docs/`, `README.md`, `CHANGELOG.md`, `docs.site.json`, workflows, package README sources)
- Prior review artifacts (if present)

**Required reading**
- `AGENTS.md`
- `docs/reference/repository-layout.md`
- `docs/reference/documentation-standards.md`
- Current review index/findings artifacts when a temporary review workspace is used

**Steps**
1. Enumerate documentation-like files and routing/configuration files.
2. Classify each artifact by purpose, audience, and canonical owner.
3. Record duplicates, stale-risk candidates, and uncertain ownership.
4. Mark this phase output as read-only inventory.

**Outputs**
- Inventory artifact
- Updated consolidated findings
- Open questions for unclear ownership

**Stop conditions**
- All target paths inventoried
- Each item tagged with category/owner
- No unresolved path-discovery gaps

**Mode**  
Read-only.

### 2) Documentation consistency audit skill

**Purpose**  
Detect contradictions, redundancy, staleness, and ownership drift across current documentation.

**Inputs**
- Inventory output
- Current docs corpus and governance references
- Existing contradiction/redundancy findings

**Required reading**
- `docs/reference/project-profile.md`
- `docs/reference/documentation-standards.md`
- `docs/adr/README.md`
- Latest inventory artifact
- Consolidated findings artifact

**Steps**
1. Compare equivalent claims across docs.
2. Identify direct conflicts, soft conflicts, and duplicated ownership.
3. Classify issues by severity and remediation type.
4. Record decision-dependent items as explicit questions.

**Outputs**
- Consistency audit artifact with contradiction/redundancy matrix
- Prioritized remediation candidates

**Stop conditions**
- Major claim domains cross-checked
- Every inconsistency has a proposed disposition

**Mode**  
Read-only.

### 3) Docs-vs-code gap analysis skill

**Purpose**  
Verify that documentation claims match implemented and tested behavior.

**Inputs**
- Consistency-audit output
- Source/tests/samples evidence
- Accepted ADR decisions relevant to audited areas

**Required reading**
- `docs/reference/validation.md`
- `docs/reference/architecture/testing-strategy.md`
- Relevant package/reference docs in scope
- Relevant source/tests/samples files
- Consolidated findings artifact

**Steps**
1. Extract behavior/API claims from docs in scope.
2. Check each claim against source, tests, and samples.
3. Mark claims as aligned, underdocumented, or contradictory.
4. Capture concrete remediation targets tied to evidence.

**Outputs**
- Gap-analysis artifact with verified/unverified claim table
- Candidate implementation tasks linked to evidence

**Stop conditions**
- All in-scope claims have evidence status
- Unknowns are logged as follow-ups/questions

**Mode**  
Read-only.

### 4) Maintainer decision capture skill

**Purpose**  
Convert open questions into durable, traceable maintainer decisions that gate implementation.

**Inputs**
- Open questions list
- Maintainer responses
- Existing decision ledger

**Required reading**
- Open questions artifact
- Decision ledger artifact
- Consolidated findings artifact
- `docs/reference/git-workflow.md`

**Steps**
1. Map each maintainer response to one or more question IDs.
2. Assign decision IDs, scope, applicability, and supersedence links.
3. Mark corresponding questions resolved/deferred.
4. Summarize implementation implications in consolidated findings.

**Outputs**
- Updated decision table
- Updated question statuses
- Implementation-ready decision summary

**Stop conditions**
- Blocking questions resolved or explicitly deferred
- Decision records traceable to question IDs and affected files

**Mode**  
Read-only record-keeping (no product-doc edits).

### 5) Documentation implementation skill

**Purpose**  
Apply approved documentation changes safely with explicit non-destructive/destructive gates.

**Inputs**
- Approved decisions
- Consolidated remediation list
- Target docs files and ownership map

**Required reading**
- `AGENTS.md`
- `docs/reference/documentation-standards.md`
- `docs/reference/validation.md`
- Decision ledger artifact
- Latest implementation artifact

**Steps**
1. Filter tasks to approved scope.
2. Apply non-destructive edits first; defer gated destructive actions.
3. Keep canonical ownership boundaries intact.
4. Record exact files changed/moved/deleted.

**Outputs**
- Implementation artifact
- Updated summaries reflecting decision outcomes
- Updated validation records

**Stop conditions**
- All approved in-scope tasks complete
- Deferred/gated tasks explicitly listed
- No unapproved destructive action performed

**Mode**  
May edit after approval.

### 6) Validation/reporting skill

**Purpose**  
Produce honest, policy-aligned validation records and completion reporting for each phase.

**Inputs**
- Change scope classification
- Executed command/workflow results
- Validation policy references

**Required reading**
- `docs/reference/validation.md`
- `AGENTS.md` honesty standard
- Current phase artifact
- Validation log artifact

**Steps**
1. Determine minimum expected validation for the change type.
2. Run required checks when applicable, or mark them as not run.
3. Record command/workflow, directory, summary, and skip reason.
4. Ensure phase report and validation log agree.

**Outputs**
- Updated validation log artifact
- Validation section in the phase artifact

**Stop conditions**
- Every relevant check recorded as Passed/Failed/Not run/Not completed
- No implied success without execution evidence

**Mode**  
Read-only unless part of an approved implementation phase.

### 7) ADR/docs-site synchronization skill

**Purpose**  
Keep ADR index, ADR statuses, and docs-site routing synchronized with discoverability policy.

**Inputs**
- ADR index and ADR files
- `docs.site.json` routes/excludes
- Discoverability policy decisions

**Required reading**
- `docs/adr/README.md`
- `docs.site.json`
- `docs/reference/documentation-standards.md`
- Relevant maintainer decisions

**Steps**
1. Audit accepted/superseded/deprecated ADR set and intended public relevance.
2. Compare ADR set with docs-site routes.
3. Flag missing/extra routes relative to policy.
4. Propose or apply approved synchronization updates.

**Outputs**
- Synchronization audit and/or implementation summary
- Explicit drift risks and follow-up actions

**Stop conditions**
- ADR index and docs-site alignment explicitly confirmed
- Intentional exceptions documented

**Mode**  
Read-only for audit; may edit after approval when synchronization changes are authorized.

## Temporary review workspace usage

Future documentation review cycles may stage intermediate artifacts in a temporary workspace (for example under `tmp/`) for inventory, findings, questions, decisions, and validation logs.

Temporary workflow artifacts should be consolidated into durable documentation when the review closes.
