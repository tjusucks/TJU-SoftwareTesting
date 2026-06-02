# Assignment 02 Black-Box Conventions (Member4)

This document defines the baseline contract for the Assignment 02 black-box
design pipeline owned by member4.

The goal is simple: member4 can run the pipeline independently in standalone mode,
and align to integrated mode when member2/member3 formal outputs are available.

## 1. Ownership and Adaptation Rule

- Member4 owns the black-box contract and maintains this file.
- Member2/member3 outputs must follow the schema files in `Assignment 02/schemas/`.
- Member1 orchestrator should call this contract as-is.
- If upstream outputs are missing, member4 uses `inputs/reference/*` without blocking.

## 2. Directory Layout

```text
Assignment 02/
├── CONVENTIONS.md
├── schemas/
│   ├── structured-requirements.schema.json
│   ├── risk-analysis.schema.json
│   ├── black-box-input.schema.json
│   ├── black-box-output.schema.json
│   └── designer-revisions.schema.json
├── skills/black-box-design/
│   ├── SKILL.md
│   └── template.md
├── inputs/
│   ├── reference/
│   │   ├── structured-requirements/
│   │   └── risk-analysis/
│   └── review/
├── outputs/
│   ├── structured-requirements/
│   ├── risk-analysis/
│   └── black-box-design/
└── docs/member4/
```

## 3. ID and Naming Convention

- `feature_id`: kebab-case, e.g. `article-lifecycle`
- `requirement_id`: `R{n}` (R1, R2, ...)
- `coverage_item_id`: `CI-{n}`
- `equivalence_partition_id`: `EP-{n}`
- `boundary_value_id`: `BVA-{n}`
- `decision_table_id`: `DT-{n}`
- `scenario_id`: `SC-{nn}` (SC-01, SC-02, ...)
- `test_case_id`: `TC-{nnn}` (TC-001, TC-002, ...)
- `run_id`: `A2-BB-{FEATURE}-R{rev}`, e.g. `A2-BB-ARTICLE-LIFECYCLE-R1`

## 4. Input Modes

The skill supports three modes:

1. `standalone` (default)
   - Uses feature spec + member4 reference structured requirements + reference risk.
2. `integrated`
   - Uses member2/member3 formal outputs in `outputs/structured-requirements` and `outputs/risk-analysis`.
3. `orchestrated`
   - Same payload shape, invoked by member1 orchestrator.

If `integrated` files are missing, fallback to `standalone` inputs.

Integrated article-lifecycle formal paths:

- `Assignment 02/outputs/structured-requirements/article-lifecycle.json`
- `Assignment 02/outputs/risk-analysis/article-lifecycle/risk-analysis.json`

Final frozen package status:

- Current authoritative final package is integrated and based on `R1`-`R12`.
- Standalone `inputs/reference/*` remains available only as fallback.

## 5. Priority Defaults (Risk Fallback)

When risk input is missing, use:

- High: auth/unauthenticated, validation, ownership, core CRUD write actions
- Medium: persistence checks, duplicate-title behavior, list consistency
- Low: non-critical readability/usability checks

## 6. Review and Revision Contract

Designer revisions are provided in `inputs/review/*.json` using the schema
`designer-revisions.schema.json`.

Supported actions:

- `add`
- `modify`
- `remove`

Supported target types:

- `coverage_item`
- `test_case`
- `decision_table_row`
- `traceability`

Each revision must include:

- `action`
- `target_type`
- `target_id` (for modify/remove)
- `reason`

## 7. Required Deliverables (Member4 Scope)

For `article-lifecycle`, member4 must produce:

- `report.md`
- `artifacts.json`
- `test-cases.csv`
- `traceability.csv`
- `revision_log.json` (after review rounds)

## 8. Handoff Output (to Member5)

Final handoff directory:

`Assignment 02/outputs/black-box-design/final/article-lifecycle/`

Minimum files:

- `report.md`
- `test-cases.csv`
- `traceability.csv`
- `HANDOFF.md`
