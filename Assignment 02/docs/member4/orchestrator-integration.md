# Orchestrator Integration Notes (Optional)

This document explains how member1 can mount the member4 black-box pipeline.

## 1. Skill Path

- `Assignment 02/skills/black-box-design/SKILL.md`

## 2. Input Schema

- `Assignment 02/schemas/black-box-input.schema.json`

Required fields:

- `schema_version`
- `feature_id`
- `input_mode`
- `feature_spec_path`
- `structured_requirements_path`
- `risk_analysis_path`
- `review_context`

## 3. Runtime Modes

- `standalone`: use reference paths maintained by member4
- `integrated`: use member2/member3 formal outputs
- `orchestrated`: same payload shape, orchestrator-driven trigger

Integrated input paths:

- `Assignment 02/outputs/structured-requirements/article-lifecycle.json`
- `Assignment 02/outputs/risk-analysis/article-lifecycle/risk-analysis.json`

## 4. Recommended Orchestrator Flow

1. generate/collect structured requirements (member2)
2. generate/collect risk analysis (member3)
3. write black-box input JSON
4. call black-box-design
5. collect output files:
   - report.md
   - artifacts.json
   - test-cases.csv
   - traceability.csv
   - revision_log.json (if review rounds used)

## 5. Fallback Rule

If member2/member3 files are missing, orchestrator may switch to `standalone`
and reuse member4 reference inputs:

- `Assignment 02/inputs/reference/structured-requirements/article-lifecycle.json`
- `Assignment 02/inputs/reference/risk-analysis/article-lifecycle.json`
