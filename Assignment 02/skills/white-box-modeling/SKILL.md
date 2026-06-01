---
name: white-box-modeling
description: Use when modeling internal behavior for a target feature or module using state transitions, control-flow paths, logic paths, or branch/decision coverage. Produces white-box test paths, coverage rationale, traceability, and reviewable artifacts.
license: Apache-2.0
user-invocable: true
---

# White-Box Modeling

## Purpose

You are an expert QA engineer focusing on white-box modeling. Your job is to
analyze a **specific module** and generate:

- state transition models or
- logic/control-flow paths

These artifacts are then used to derive **white-box test design** for the selected feature or module.

## Scope

This skill focuses on **white-box modeling for one feature module**. It should:

- Identify key states, transitions, and guards **or** key nodes, branches, and decisions.
- Provide a minimal set of white-box test paths that cover the model.
- Map paths to requirement IDs when available.
- Produce human-readable modeling and test design output.

It should **not**:

- Generate executable tests unless explicitly requested.
- Overwrite black-box artifacts.
- Invent behaviors not supported by provided sources.

## Input Contract

The skill accepts a single input payload or user request with the following
fields (if provided):

- `feature_id`
- `feature_name`
- `input_mode` (`standalone`, `integrated`, `orchestrated`)
- `feature_spec_path`
- `structured_requirements_path`
- `source_code_paths` (source files or directories relevant to the target feature)
- `target_module_path` (primary source code or design doc path)
- `entry_points` (functions, endpoints, routes, handlers, or UI actions to model)
- `black_box_artifacts_path` (optional handoff from black-box design)
- `generate_executable_tests` (optional boolean; default `false`)
- `test_framework` (optional; e.g., Playwright, PyTest, JUnit, Jest)
- `test_output_path` (optional; where generated test code should be written)
- `review_context.revision` (optional)
- `review_context.designer_revisions_path` (optional)

### Input Source Roles

- Requirements define the intended behavior and provide requirement IDs for traceability.
- Source code and design docs reveal internal states, branches, guards, and paths.
- Black-box artifacts help avoid duplicate coverage and identify internal paths that complement existing test cases.
- If source code is not available, derive only a logical model from requirements and clearly mark the model elements as assumptions.

### Input Mode Rules

1. `standalone`:
   - Use feature spec and any provided module design/code notes.
2. `integrated`:
   - Use upstream structured requirements if available.
3. `orchestrated`:
   - Same data contract, invoked by orchestrator.

If `input_mode` is not specified, default to `standalone`.

## Required Workflow

Follow these steps strictly:

1. **Scope and Model Selection**
   - Identify the target module and its boundaries.
   - Choose **one** model type: state transition (`state`) or logic/control-flow paths (`path`).
   - Justify the choice based on the module behavior.

2. **Extract Internal Logic**
   - Use provided design docs or source code excerpts if available.
   - If no code or implementation design is provided, derive a minimal internal model from requirements.
   - **Constraint**: In the absence of source code, all derived structures must be explicitly marked as "Logical Assumptions" in the report and JSON.

3. **Build the Model**
   - **For State Model**: Define states, transitions, guards, and actions.
   - **For Path Model**: Define nodes (decisions, statements), edges, conditions, and branches.

4. **Coverage Strategy and Path Optimization**
   - Define target coverage criteria (e.g., state coverage, transition coverage, branch/decision coverage).
   - Generate candidate paths or sequences for the chosen model.
   - Select the smallest practical set of paths that satisfies the chosen coverage criterion.
   - If multiple equivalent path sets exist, prefer paths that cover high-risk requirements or complement black-box coverage.

5. **Derive White-Box Test Paths**
   - Create path IDs (`WB-PATH-*`) and link to requirements where possible.
   - For each path, describe the test intent, sequence of steps (nodes or states), and expected internal effects (e.g., variable updates, event emissions).

6. **Review Loop**
   - If `designer_revisions_path` exists, apply revisions and log changes.
   - After revisions, re-check the model, coverage criteria, selected paths, and traceability.

7. **Optional Executable Test Generation**
   - Only generate executable test code when `generate_executable_tests` is true or the designer explicitly requests it.
   - Generate tests from the selected white-box paths, not from unrelated assumptions.
   - Each generated test should reference the corresponding `WB-PATH-*` ID and requirement ID where available.
   - Use the requested `test_framework` if provided; otherwise recommend a suitable framework and ask for confirmation before writing code.
   - Treat generated tests as optional implementation artifacts; the required output remains the white-box model and test path design.

8. **Export**
   - Generate `report.md` and `artifacts.json`.
   - If executable tests are generated, write them to `test_output_path` or `outputs/white-box-modeling/{feature_id}/tests/`.

## Review / Revision Behavior

Revision input follows the shared revision contract:

- `schemas/designer-revisions.schema.json`

Supported actions:

- `add`
- `modify`
- `remove`

Supported target types:

- `state`
- `transition`
- `node`
- `edge`
- `white_box_path`
- `traceability`

If revision input is present, you must:

- Apply each revision deterministically.
- Record before/after summary in `revision_log.json`.
- Preserve stable IDs where possible.
- Recalculate coverage metrics and update traceability.
- Re-present the revised model and path set for designer confirmation.

## Output Contract

### Output Directory

```
outputs/white-box-modeling/{feature_id}/
├── report.md
├── artifacts.json
├── tests/              # present only if executable tests were requested
└── revision_log.json   # present only if revisions were applied
```

### Schema

The machine-readable output must validate against:

- `schemas/white-box-output.schema.json`

`report.md` should follow:

- `skills/white-box-modeling/template.md`
