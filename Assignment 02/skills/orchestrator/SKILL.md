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

- For stages with a dedicated sub-skill, follow that skill's workflow.
- For stages without a dedicated skill, use the inline fallback instructions in this file.
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

**Sub-skill**: not yet available — use inline fallback.

**Fallback behavior**:

1. Check if `inputs/reference/risk-analysis/{feature_id}.json` exists.
   - If yes: load it and present to the designer as the baseline risk assessment.
   - If no: generate a lightweight risk assessment using the rules below.
2. Write output to `outputs/risk-analysis/{feature_id}/`.
3. **Pause for review.**

**Inline risk assessment rules** (used only when no reference data and no dedicated skill):

For each requirement from Stage 1, assign:

- **Risk level** (High / Medium / Low) based on:
  - High: authentication, authorization, data integrity, core write operations
  - Medium: validation, persistence, list/query consistency
  - Low: display, formatting, non-critical UX
- **Test priority** matching risk level.
- **Rationale**: one sentence explaining the risk assignment.

Output format:

```json
{
  "feature_id": "...",
  "requirements_risk": [
    {
      "requirement_id": "R1",
      "risk_level": "High",
      "test_priority": "High",
      "rationale": "..."
    }
  ]
}
```

When a dedicated `risk-analysis` skill becomes available, this fallback is
replaced by that skill's workflow.

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

**Sub-skill**: not yet available — use inline fallback.

**Fallback behavior**:

1. Based on the structured requirements and black-box results from previous stages, model system behavior using state transition diagrams or control flow graphs.
2. Identify states, transitions, and coverage criteria (e.g., all-states, all-transitions).
3. Generate white-box test sequences that complement the black-box test cases.
4. Write output to `outputs/white-box-modeling/{feature_id}/`.
5. **Pause for review.**

Note: generating executable test code is optional. The focus is on modeling and test sequence design.

When a dedicated `white-box-modeling` skill becomes available, this fallback is replaced by that skill's workflow.

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
