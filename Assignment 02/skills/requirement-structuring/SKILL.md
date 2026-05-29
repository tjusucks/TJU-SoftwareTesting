---
name: requirement-structuring
description: Use when extracting, analyzing, and structuring requirements from local project files, existing requirement documents, CSV-like inputs, README/API docs, issue text, or agent CLI/user input into a schema-compatible structured requirement specification. Produces requirement input inventory, R1/R2-style requirement IDs, structured requirement items, constraints, conditions, expected actions, acceptance criteria, open questions, and source evidence for downstream risk analysis and black-box test design.
user-invocable: true
---

# Requirement Structuring

## Overview

This skill helps the tool read and structure requirements before risk analysis,
black-box design, or white-box work begins.

It converts requirement sources into a stable intermediate specification that
downstream workflows can consume.

**The requirements structuring workflow handles**:

- Organize requirement sources for the target app.
- Read local project files, requirement text, CSV-like data, or agent CLI/user input.
- Extract requirement items, constraints, conditions, and expected behavior.
- Assign and maintain stable `R1`, `R2`, `R3` requirement IDs.
- Produce structured requirement results with source evidence and open questions.
- Keep the output schema stable for orchestration and downstream consumers.

**Primary outputs**:

- Requirement input inventory.
- Structured requirement specification.
- Requirement IDs using the toolchain-compatible `R1`, `R2`, `R3` convention.
- Requirement-to-source mapping.
- Acceptance criteria candidates.
- Ambiguities, assumptions, and questions for human review.

---

## When to Use This Skill

Use this skill when:

- The user asks for **requirement structuring**, **requirements parsing**, or
  **requirements analysis/structuring**.
- The input is a local project, README, API documentation, product description,
  issue list, CSV/table, requirement document, or free-form agent CLI/user input.
- Downstream skills need normalized requirement items before risk analysis or test
  design.
- A stable JSON/CSV/Markdown requirement specification is needed.
- Requirement IDs must be generated or normalized to `R1`, `R2`, `R3` style.

Do not use this skill for:

- Full project orchestration or final report integration.
- Risk scoring or test-plan ownership.
- Black-box test case design beyond acceptance-criteria candidates.
- White-box modeling, executable test generation, or test execution.
- Requirements change-control workflow after a baseline has already been approved.
- CMMI maturity-level assessment unless it directly affects the output format.

---

## Scope Boundary

This skill focuses on **Requirement Structuring**:

1. Read requirement text / CSV / user input.
2. Extract requirement items, constraints, conditions, and expected behavior.
3. Output structured requirement results.
4. Maintain sequential `R1`, `R2`, `R3` requirement IDs.
5. Keep the output format stable for orchestration and downstream tools.

The skill should stop at structured requirements. It may generate acceptance criteria
because they are part of requirement specification, but it should not generate risk
scores, coverage items, test cases, traceability matrices to tests, or execution scripts.

---

## High-Level Workflow

Follow this workflow for requirement structuring tasks:

1. **Collect inputs**
   - Identify source type: local files, requirement document, CSV/table, README/API
     docs, issue text, or agent CLI/user input.
   - Record each source in the requirement input inventory.
   - Preserve file paths, headings, row numbers, or line references when available.

2. **Extract candidates**
   - Identify explicit requirements first.
   - Mark inferred requirements separately.
   - Extract actors, actions, objects, constraints, conditions, expected behavior,
     non-functional requirements, and business rules.

3. **Normalize**
   - Merge duplicates.
   - Split overloaded requirements.
   - Keep ambiguous items but mark them as needing review.
   - Assign stable IDs using `R1`, `R2`, `R3`, incrementing sequentially within
     the feature requirement set.

4. **Specify**
   - Convert each item to the required output schema.
   - Add acceptance criteria only to the human-readable summary or `notes` when
     supported by the source or a clear inference.
   - Keep priority out of the machine schema unless the user explicitly requests it;
     record candidate priority in `notes` only.

5. **Validate**
   - Check every requirement has `id`, `description`, `conditions`, and
     `expected_actions`.
   - Check source, type, confidence, assumptions, constraints, and review status are
     recorded in `notes` when useful for handoff.
   - Check constraints, conditions, and expected behavior are not hidden in free text.
   - List open questions instead of inventing missing details.

---

## Input Modes

The skill supports two modes:

1. `standalone` (default)
   - User invokes directly via `/test-design:requirement-structuring`.
   - User provides source paths or text as input.
   - Output written to `outputs/structured-requirements/{feature_id}/`.

2. `orchestrated`
   - Invoked by the orchestrator skill with a standardized payload.
   - Orchestrator provides `feature_id`, `feature_spec_path`, and source list.
   - Same output contract; orchestrator reads the output path.

If `input_mode` is not specified, default to `standalone`.

---

## Required Output Contract

Produce both a human-readable summary and a machine-readable structure when possible.
For downstream `black-box-design` handoff, the machine-readable output must match
the structured requirements schema used by this toolchain
(`schemas/structured-requirements.schema.json`).

### Output Directory

```
outputs/structured-requirements/{feature_id}/
├── structured-requirements.json   # machine-readable, schema-compliant
├── report.md                      # human-readable summary
└── revision_log.json              # present only if revisions were applied
```

### Minimum Fields per Requirement

```json
{
  "id": "R1",
  "description": "A registered user can log in with email and password.",
  "input_fields": ["email", "password"],
  "data_ranges": [],
  "conditions": ["The account exists", "The credentials are valid"],
  "expected_actions": [
    "The system creates an authenticated session and redirects the user."
  ],
  "notes": "Source: README.md, Authentication section. Type: functional. Confidence: medium. Constraint: password must not be exposed in logs."
}
```

Also provide in the human-readable summary when useful:

- `input_inventory`: all files/text/table sources considered.
- `requirement_id_registry`: assigned `R*` IDs and the next available number.
- `assumptions`: assumptions made during extraction.
- `open_questions`: questions requiring human review.
- `excluded_sources`: files or content intentionally skipped, with reason.

---

## Reference Sheets (On-Demand)

### 1. Requirement Input Intake (`requirements-elicitation.md`)

**When to load**: Reading local project files, requirement docs, CSV-like tables,
or agent CLI/user input.

Use this reference to:

- Decide which files and text sources to inspect.
- Capture source evidence without over-reading irrelevant code.
- Convert user/agent CLI input into requirement source records.
- Build the requirement input inventory.

### 2. Requirement Structuring Analysis (`requirements-analysis.md`)

**When to load**: Extracting requirement items, constraints, conditions, expected
behavior, ambiguity, duplicates, and conflicts.

Use this reference to:

- Split and normalize requirement candidates.
- Classify functional, non-functional, constraint, and business-rule items.
- Identify missing information and open questions.
- Avoid hallucinating priorities or behavior not supported by sources.

### 3. Requirement Specification Output (`requirements-specification.md`)

**When to load**: Producing final structured requirement results, `R*` IDs,
acceptance criteria, JSON/CSV/Markdown output, or handoff to orchestration.

Use this reference to:

- Apply the output schema.
- Assign stable `R1`, `R2`, `R3` requirement IDs.
- Generate acceptance criteria candidates.
- Run the requirement structuring quality checklist.

---

## Review / Revision Behavior

The designer may review the structured requirements and request changes.
Revision input reuses the shared revision schema:

- `Assignment 02/schemas/designer-revisions.schema.json`

Supported actions:

- `add` — add a new requirement item
- `modify` — change description, conditions, expected_actions, or notes
- `remove` — remove a requirement item

Supported target types:

- `requirement`

If revision input is present, you must:

- Apply each revision deterministically to the current requirement set.
- Record before/after summary in `revision_log.json`.
- Preserve ID stability: removed IDs are not reused; new items get the next
  sequential ID.
- Re-run the Validate step after applying revisions.

---

## Outside This Skill's Scope

The following parts from the previous lifecycle-oriented skill are intentionally removed
from the active requirement structuring skill because they belong to downstream
workflow stages:

- Full requirements change management and volatility control.
- Full bidirectional traceability from requirements to design/code/tests.
- CMMI Level 2/3/4 maturity scaling.
- Risk analysis and test-plan ownership.
- Black-box test design, coverage items, and executable test cases.
- White-box modeling and test execution.

Keep only lightweight source mapping inside requirement records. Test traceability
belongs to downstream test-design or orchestration workflows.

---

## Loading Reference Sheets

Load only the reference needed for the current step:

- Need to read inputs: load `requirements-elicitation.md`.
- Need to extract or normalize requirement items: load `requirements-analysis.md`.
- Need to produce the final structured output: load `requirements-specification.md`.

If the source is a local project, first inspect project-level files such as README,
docs, API specs, issue exports, user stories, or existing requirement files. Only read
source code when documentation is absent or when the user explicitly asks for inferred
requirements from implementation.
