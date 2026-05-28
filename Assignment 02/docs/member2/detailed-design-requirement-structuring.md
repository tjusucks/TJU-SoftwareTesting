# Detailed Design - Requirement Structuring Section (Member2)

## 1. Module Scope and Requirement Concept

### 1.1 Target Module

- Target application: RealWorld (Conduit)
- Focus module: Article Lifecycle
- Covered requirement sources:
  - local requirement documents
  - README or API documentation
  - CSV-like requirement tables
  - free-form user or agent input
  - feature specification text when available

### 1.2 Requirement Concept

The member2 module converts raw requirement sources into a stable structured
requirement specification for downstream risk analysis and black-box design.

The design follows this flow:

1. Requirement source intake
2. Requirement candidate extraction
3. Requirement normalization and ID assignment
4. Structured output generation
5. Quality review and open-question recording
6. Handoff to downstream modules

## 2. Requirement Source Intake

Input handling is defined by:

- `Assignment 02/skills/requirement-structuring/SKILL.md`
- `Assignment 02/skills/requirement-structuring/references/requirements-elicitation.md`

Primary source categories:

- Project-level documentation such as README, docs, and API specs
- Existing requirement documents or feature descriptions
- CSV/table rows containing requirement fields
- User-provided requirement text
- Implementation files only when documentation is absent or inferred
  requirements are explicitly requested

Each source should be recorded with enough evidence for review:

- source path or source label
- heading, row, section, or line reference when available
- reason for using or skipping the source
- confidence when the source is incomplete or inferred

## 3. Structuring Strategy and Method

### 3.1 Applied Requirement Structuring Techniques

1. Source inventory and evidence capture
2. Atomic requirement extraction
3. Duplicate merging
4. Overloaded requirement splitting
5. Constraint, condition, and expected-action separation
6. Ambiguity and open-question recording

### 3.2 Technique Selection Rationale

- Source inventory prevents hidden or undocumented input assumptions.
- Atomic extraction keeps each requirement testable.
- Duplicate merging avoids repeated downstream coverage work.
- Requirement splitting prevents one broad sentence from becoming one
  untestable requirement.
- Separating conditions, constraints, and expected actions makes the output
  easier for risk analysis and black-box design to consume.
- Open questions keep uncertain behavior visible instead of silently inventing
  product decisions.

## 4. Structured Requirement Output Design

### 4.1 Output Schema

The machine-readable output must follow:

- `Assignment 02/schemas/structured-requirements.schema.json`

Required top-level fields:

- `schema_version`
- `project_name`
- `feature_id`
- `feature_name`
- `requirement_items`

Recommended top-level fields:

- `source_path`
- `actors`
- `preconditions`
- `business_rules`
- `input_constraints`
- `error_conditions`

Each `requirement_items[]` entry must include:

- `id`
- `description`
- `input_fields`
- `data_ranges`
- `conditions`
- `expected_actions`
- `notes` when review metadata is useful

### 4.2 Requirement ID Convention

Requirement IDs use the stable sequential format:

```text
R{n}
```

Examples:

- `R1`
- `R2`
- `R3`
- `R10`

Rules:

- Start at `R1` for a new feature requirement set.
- Continue the previous sequence when revising an approved baseline.
- Do not encode module names or requirement types in the ID.
- Do not reuse deleted IDs in the same baseline.

### 4.3 Expected Output Location

Formal integrated outputs should be placed in:

- `Assignment 02/data/structured-requirements/{feature_id}.json`

For the article lifecycle feature:

- `Assignment 02/data/structured-requirements/article-lifecycle.json`

If the formal output is not available, downstream modules may use the reference
fixture:

- `Assignment 02/data/reference/structured-requirements/article-lifecycle.json`

## 5. Traceability and Handoff Data

Member2 output provides the requirement side of the traceability chain:

```text
requirement_id -> risk_id -> coverage_item_id -> test_case_id
```

Member2 owns:

- stable `R*` requirement IDs
- requirement descriptions
- requirement-level source evidence
- actors, preconditions, business rules, and input constraints
- open questions and assumptions

Member2 does not own:

- risk score or likelihood/impact evaluation
- coverage item design
- black-box test cases
- white-box paths
- executable test scripts

## 6. Prompt Design

Skill definition:

- `Assignment 02/skills/requirement-structuring/SKILL.md`

Reference sheets:

- `Assignment 02/skills/requirement-structuring/references/requirements-elicitation.md`
- `Assignment 02/skills/requirement-structuring/references/requirements-analysis.md`
- `Assignment 02/skills/requirement-structuring/references/requirements-specification.md`

Prompt intent:

- Ask the skill to read requirement sources or user input.
- Require `R1`, `R2`, `R3` style IDs.
- Require schema-compatible JSON output.
- Require open questions instead of unsupported assumptions.
- Require conditions and expected actions to be separated from prose.

## 7. Quality Review

Before handoff, check that:

- Every requirement has a stable `R*` ID.
- Every requirement is atomic and testable.
- Conditions and expected actions are populated.
- Input fields and data ranges are not hidden only in description text.
- Ambiguous or inferred behavior is marked in `notes`.
- Source evidence is available in `notes` or the review summary.
- The JSON passes `structured-requirements.schema.json`.
- No risk scores, coverage items, test cases, or execution scripts are included.

## 8. Integration with Downstream Members

### 8.1 Handoff to Risk Analysis (Member3)

Member3 consumes the structured requirements and adds risk information.

Expected input from member2:

- `Assignment 02/data/structured-requirements/{feature_id}.json`

Member3 should preserve `R*` IDs when attaching risk records.

### 8.2 Handoff to Black-Box Design (Member4)

Member4 consumes structured requirements through:

- `Assignment 02/schemas/black-box-input.schema.json`

The black-box input points to:

- `structured_requirements_path`
- `risk_analysis_path`

Member4 depends on member2 preserving:

- `feature_id`
- `requirement_items[]`
- `id`
- `conditions`
- `expected_actions`

## 9. Skill Execution Record

Recommended execution evidence:

- input source inventory
- generated structured requirement JSON
- human-readable requirement summary
- open-question list
- validation result against `structured-requirements.schema.json`

Suggested run record fields:

- runtime
- skill path
- feature ID
- source paths
- output path
- last assigned requirement ID
- validation status

## 10. Sections Owned by Downstream Members

### 10.1 Risk Analysis

To be completed by member3.

### 10.2 Black-Box Test Design

To be completed by member4.

### 10.3 Test Tool Implementation and Execution

To be completed by member5.
