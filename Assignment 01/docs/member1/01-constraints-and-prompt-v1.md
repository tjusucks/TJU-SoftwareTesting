# Member1 Task 1: Immutable Constraints + Prompt v1

## 1) Immutable constraints from assignment

Source of truth:
- course assignment requirements (shared in class materials)
- `Assignment 01/blackbox-testing/skills/SKILL.md`

Hard requirements that must not be violated:

1. The tool must support one valid input type:
   - system requirements, or
   - a testing codebase/module.
2. The black-box branch must generate test cases as output.
3. Submission must include:
   - prompts used,
   - model used,
   - model-generated code,
   - experimental analysis (accuracy/coverage/generalizability),
   - report with traditional method comparison and AI limitations.
4. Prompt and generated tests must stay implementation-independent:
   - no assumptions about internal code, DB schema, or hidden architecture.
5. Do not invent requirements not grounded in user-provided spec.
6. If requirement text is incomplete, list assumptions and missing details.
7. Output must keep the 7-section structure in `SKILL.md`:
   - Feature Summary
   - Requirements Extracted
   - Test Design Strategy
   - Test Scenarios
   - Detailed Test Cases
   - Coverage Summary
   - Ambiguities / Missing Information / Assumptions
8. Test design should naturally apply black-box techniques where relevant:
   - EP, BVA, decision table, state transition, scenario/error-focused cases.

## 2) Member1 baseline scope

Role scope for Member1 (team-agreed):
- design workflow,
- write prompts,
- tune prompts,
- define input and output formats.

Deliverables in this folder:
- `prompts/prompt-v1.txt`
- `prompts/prompt-v2.txt`
- `prompts/prompt-v3.txt`
- `templates/*`
- `experiments/*`
- `report-materials/*`

## 3) Prompt v1 baseline design goals

v1 objective:
- stable structure first, coverage second.

v1 acceptance criteria:
- same input run twice -> same section structure and same field names.
- each requirement must appear in `Requirements Extracted`.
- each test case must map to at least one requirement ID.

v1 limitations (expected):
- may miss deep boundary combinations.
- may under-cover cross-requirement interactions.
- may include duplicates when requirements overlap.
