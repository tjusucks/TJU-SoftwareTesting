# Orchestrator Integration Notes (Optional)

This document explains how member1 can mount the member2 requirement
structuring pipeline.

## 1. Skill Path

- `Assignment 02/skills/requirement-structuring/SKILL.md`

## 2. Output Schema

- `Assignment 02/schemas/structured-requirements.schema.json`

Required top-level fields:

- `schema_version`
- `project_name`
- `feature_id`
- `feature_name`
- `requirement_items`

Required fields in each `requirement_items[]` entry:

- `id`
- `description`
- `input_fields`
- `data_ranges`
- `conditions`
- `expected_actions`

Optional but recommended fields:

- `source_path`
- `actors`
- `preconditions`
- `business_rules`
- `input_constraints`
- `error_conditions`
- `notes`

## 3. Runtime Modes

- `standalone`: use local requirement sources or user-provided text directly
- `integrated`: write member2 formal output into `data/structured-requirements`
- `orchestrated`: same output shape, orchestrator-driven trigger

## 4. Recommended Orchestrator Flow

1. collect requirement source paths or user-provided requirement text
2. call `requirement-structuring`
3. generate structured requirements with `R1`, `R2`, `R3` style IDs
4. write JSON to `Assignment 02/data/structured-requirements/{feature_id}.json`
5. validate JSON against `structured-requirements.schema.json`
6. pass the output path to member3 risk analysis
7. pass the same output path into member4 black-box input as
   `structured_requirements_path`

## 5. Compatibility Rules

Downstream modules rely on these rules:

- Keep requirement IDs in `R{n}` format.
- Keep `feature_id` in kebab-case.
- Keep `requirement_items` as a non-empty array.
- Put extra metadata in `notes` instead of adding schema-unknown fields.
- Keep constraints, conditions, and expected actions visible.
- Do not output risk scores, coverage items, test case IDs, or execution results.

## 6. Expected Output Files

Formal integrated output:

- `Assignment 02/data/structured-requirements/{feature_id}.json`

For article lifecycle:

- `Assignment 02/data/structured-requirements/article-lifecycle.json`

Optional human-readable review artifacts:

- requirement source inventory
- Markdown requirement summary
- open-question list
- validation log

## 7. Fallback Rule

If member2 formal output is missing, orchestrator may switch downstream modules to
reference input:

- `Assignment 02/data/reference/structured-requirements/article-lifecycle.json`

This fallback should be recorded as a standalone/reference run, not as member2's
formal integrated output.

## 8. Downstream Handoff

Member3 risk analysis should consume:

- `Assignment 02/data/structured-requirements/{feature_id}.json`

Member4 black-box design should receive the same path through:

- `structured_requirements_path`

Member4 can then combine member2 and member3 outputs through:

- `Assignment 02/schemas/black-box-input.schema.json`
