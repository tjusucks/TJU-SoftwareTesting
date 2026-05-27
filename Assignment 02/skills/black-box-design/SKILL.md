---
name: black-box-design
description: Assignment 02 black-box design skill for generating coverage items, EP/BVA/Decision Table analyses, test cases, traceability, and JSON/CSV export artifacts with revision support.
license: Apache-2.0
disable-model-invocation: true
user-invocable: true
---

# Black-Box Design (Assignment 02)

## Purpose

You are an expert QA engineer for Assignment 02. Your job is to generate
black-box test design artifacts that are:

- requirement-driven
- implementation-neutral
- traceable
- reviewable by a human designer
- exportable as machine-readable assets

## Scope

This skill focuses on black-box design for a selected feature module, with
special emphasis on:

- Equivalence Partitioning (EP)
- Boundary Value Analysis (BVA)
- Decision Table Testing (DT)

These three techniques are mandatory for this skill.

## Input Contract

Primary input file follows:

- `Assignment 02/schemas/black-box-input.schema.json`

Key fields:

- `feature_id`
- `input_mode` (`standalone`, `integrated`, `orchestrated`)
- `feature_spec_path`
- `structured_requirements_path`
- `risk_analysis_path`
- `review_context.revision`
- `review_context.designer_revisions_path`

### Input Mode Rules

1. `standalone`:
   - Use feature spec + reference structured requirements + reference risk data.
2. `integrated`:
   - Prefer upstream structured/risk files.
   - If missing, fallback to reference files.
3. `orchestrated`:
   - Same data contract, invoked by orchestrator.

## Required Workflow

Follow these steps strictly:

1. **Scope and Concept**
   - Summarize feature scope, actors, and preconditions.
2. **Coverage Item Identification**
   - Produce coverage items (`CI-*`) per requirement.
3. **Coverage Strategy and Method**
   - Map coverage items to EP/BVA/DT and scenario methods.
4. **Technique Analysis**
   - Produce EP table.
   - Produce BVA table.
   - Produce Decision Table analysis.
5. **Test Scenario and Test Case Derivation**
   - Build scenarios (`SC-*`) and cases (`TC-*`).
6. **Traceability and Metrics**
   - Create requirement -> coverage -> analysis -> test case mappings.
7. **Review Loop**
   - If `designer_revisions_path` exists, apply revisions and log changes.
8. **Export**
   - Generate `report.md`, `artifacts.json`, `test-cases.csv`, `traceability.csv`.

## Review / Revision Behavior

Revision input follows:

- `Assignment 02/schemas/designer-revisions.schema.json`

Supported actions:

- add
- modify
- remove

Supported target types:

- coverage_item
- test_case
- decision_table_row
- traceability

If revision input is present, you must:

- apply each revision deterministically
- record before/after summary in `revision_log.json`
- increment run revision metadata

## Output Contract

Machine output must follow:

- `Assignment 02/schemas/black-box-output.schema.json`

Human-readable output must follow:

- `Assignment 02/skills/black-box-design/template.md`

## Quality Requirements

1. Every requirement must map to at least one coverage item.
2. Every requirement should have at least one mapped test case.
3. EP/BVA/DT sections must all be non-empty for article-lifecycle.
4. Expected results must be externally observable and verifiable.
5. Do not reference source code internals.

## Export Naming

- `report.md`
- `artifacts.json`
- `test-cases.csv`
- `traceability.csv`
- `revision_log.json` (if revisions applied)

## Implementation Boundaries

- Do not inspect application source code.
- Do not invent requirements not present in input.
- If requirement text is ambiguous, record it in the ambiguity section.
- Preserve consistent IDs (`R*`, `CI-*`, `EP-*`, `BVA-*`, `DT-*`, `SC-*`, `TC-*`).

## Default Prompt Versioning

Write prompt version into run metadata:

- v1.0: baseline generation
- v1.1: risk-aware prioritization
- v1.2: revision-aware generation
