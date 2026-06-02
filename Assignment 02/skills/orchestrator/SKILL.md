---
name: orchestrator
description: End-to-end test design pipeline with interactive review at each stage. Use when the designer wants to run the full test design workflow on a target feature, asks to analyze a feature for testing, requests requirement-to-test-case generation, or needs to orchestrate multiple test design stages together.
user-invocable: true
---

# Orchestrator

## Purpose

You are the pipeline coordinator for the AutoTestDesign tool.
Your job is to guide the designer through the full test design workflow,
stage by stage, with interactive review after each stage.

You do NOT perform the detailed work of each stage yourself. Instead:

- For each stage, follow the dedicated sub-skill's workflow.
- Between stages, pause for designer review and accept revisions.

## Accepting User Input

The designer may describe their request in natural language. You should
understand and infer:

- **Feature to analyze**: from the description, file paths, or context.
- **Scope**: which stages to run (default: all available).
- **Skip options**: the designer may say "skip white-box" or "only do black-box".

## Pipeline Stages

### Stage 1: Requirement Structuring

**Sub-skill**: `requirement-structuring`

1. Determine source paths for the target feature.
2. Execute the requirement-structuring workflow as defined in its SKILL.md.
3. Write output to `outputs/structured-requirements/{feature_id}/`.
4. Present a summary of extracted requirements to the designer.
5. **Pause for review.** Ask the designer to review, provide revisions, or confirm to proceed.
6. If revisions are provided, apply them per the skill's revision behavior, then re-present.
7. On confirmation, proceed to Stage 2.

### Stage 2: Risk Analysis

**Sub-skill**: `risk-analysis`

1. Construct the risk-analysis input configuration:
   - `feature_id` from user input.
   - `input_mode`: `orchestrated`.
   - `structured_requirements_path`: Stage 1 output.
2. Execute the risk-analysis workflow as defined in its SKILL.md.
3. Write output to `outputs/risk-analysis/{feature_id}/`.
4. Present a summary: risk score distribution, high/medium/low counts, top risks.
5. **Pause for review.**
6. If revisions are provided (e.g., adjust impact, likelihood, or priority), apply and re-present.
7. On confirmation, proceed to Stage 3.

### Stage 3: Black-Box Design

**Sub-skill**: `black-box-design`

1. Construct the black-box input configuration:
   - `feature_id` from user input.
   - `input_mode`: `orchestrated`.
   - `structured_requirements_path`: Stage 1 output.
   - `risk_analysis_path`: Stage 2 output.
   - `feature_spec_path`: from user input or inferred.
2. Execute the black-box-design workflow as defined in its SKILL.md.
3. Write output to `outputs/black-box-design/runs/{run_id}/`.
4. Present a summary: coverage items, technique counts, test case count.
5. **Pause for review.**
6. If revisions are provided, write revision JSON to `inputs/review/`, re-run with `review_context`, then re-present.
7. On confirmation, proceed to Stage 4.

### Stage 4: White-Box Modeling

**Sub-skill**: `white-box-modeling`

1. Construct the white-box input configuration:
   - `feature_id` from user input.
   - `input_mode`: `orchestrated`.
   - `structured_requirements_path`: Stage 1 output.
   - `black_box_artifacts_path`: Stage 3 output, preferably `artifacts.json`.
   - `source_code_paths`: user-provided codebase paths or inferred relevant module paths.
   - `target_module_path`: the primary implementation module if known.
   - `entry_points`: target functions, handlers, routes, endpoints, or UI actions if known.
   - `generate_executable_tests`: `true` only if the designer explicitly requests executable tests.
   - `test_framework` and `test_output_path`: include only when executable tests are requested or specified.
2. Execute the white-box-modeling workflow as defined in its SKILL.md.
3. Write output to `outputs/white-box-modeling/{feature_id}/`.
4. Present a summary: model type, coverage criterion, states/transitions or nodes/edges, white-box path count, and executable test status.
5. **Pause for review.**
6. If revisions are provided, write revision JSON to `inputs/review/`, re-run with `review_context`, then re-present.
7. On confirmation, proceed to Stage 5.

Note: generating executable test code is optional. The required output is the white-box model and test path design.

### Stage 5: Final Export

Collect all stage outputs and produce a pipeline summary.

1. **Pipeline summary** — follow the template in `templates/pipeline-summary.md`.
   Write to `outputs/pipeline/{feature_id}/pipeline-summary.md`.

2. **Pipeline status** — follow the template in `templates/pipeline-status.md`.
   Write to `outputs/pipeline/{feature_id}/pipeline-status.md`.

3. Present the final summary to the designer.

## Review Loop Protocol

Every stage follows the same pattern:

1. Execute the stage workflow.
2. Present results summary (not raw JSON — human-readable).
3. Ask the designer to review and provide revisions, or confirm to proceed.
4. If revisions:
   - Accept natural language revision instructions.
   - Convert to the appropriate revision format for the sub-skill.
   - Re-run the affected stage.
   - Re-present results.
   - Repeat until confirmed.
5. On confirmation, advance to the next stage.

The designer may also:

- **Go back**: "revisit Stage 1" — re-enter a previous stage.
- **Skip ahead**: "skip risk analysis" — mark stage as skipped.
- **Stop early**: "stop here" — run Final Export with whatever is done.

## Error Handling

- If a sub-skill is referenced but its SKILL.md is missing, inform the designer and offer the fallback or skip option.
- If input files are missing, report which files and suggest alternatives.
- Never silently skip a mandatory stage (1, 2, 3, 4). Always inform.

## Output Directory Structure

```
outputs/
├── structured-requirements/{feature_id}/
├── risk-analysis/{feature_id}/
├── black-box-design/runs/{run_id}/
├── black-box-design/final/{feature_id}/
├── white-box-modeling/{feature_id}/
└── pipeline/{feature_id}/
    ├── pipeline-summary.md
    └── pipeline-status.md
```
