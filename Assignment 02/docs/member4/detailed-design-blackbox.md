# Detailed Test Design - Black-Box Section (Member4)

## 1. Module Scope and Test Concept

### 1.1 Target Module

- Target application: RealWorld (Conduit)
- Focus module: Article Lifecycle
- Covered behaviors:
  - article create
  - article update
  - article delete
  - post-update persistence
  - post-delete observability

### 1.2 Test Concept

The design follows Assignment 02 "Mainly" flow:

1. Concept definition
2. Coverage item identification
3. Coverage strategy and methods
4. Test case design and traceability
5. Prompt design
6. Result analysis
7. Improvement with evidence

## 2. Coverage Item Identification

Coverage items are defined in:

- `Assignment 02/outputs/black-box-design/final/article-lifecycle/artifacts.json`
- `coverage_items[]`

Primary coverage items:

- Auth gates (create/update/delete)
- Field validation (empty title/description/body)
- Duplicate-title uniqueness semantics
- tagList decision combinations (omit/empty/null/new values)
- Persistence checks (update and delete)

ID alignment reference:

- `Assignment 02/docs/member4/requirement-id-mapping.md`

## 3. Coverage Strategy and Method (ISO 29119-4 aligned)

### 3.1 Applied Black-Box Techniques

1. Equivalence Partitioning (EP)
2. Boundary Value Analysis (BVA)
3. Decision Table Testing (DT)
4. Scenario/sequence checks (supporting persistence analysis)

### 3.2 Technique Selection Rationale

- EP is used for valid/invalid auth and field partitions.
- BVA is used for empty/non-empty and repeated action boundaries.
- DT is mandatory for multi-condition tagList update semantics.
- Scenario checks ensure lifecycle ordering is observable.

## 4. Test Case Design

### 4.1 Final Case Set

Final integrated set includes 19 cases (`TC-001` to `TC-021`, with review-added IDs) in:

- `Assignment 02/outputs/black-box-design/final/article-lifecycle/test-cases.csv`

Highlights:

- Core happy-path CRUD behaviors
- Validation negatives
- Auth negatives for create, update, and delete
- Decision-table-driven tagList branches
- Persistence checks after update/delete

### 4.2 Decision Table Example

`DT-1` (integrated `R6` partial-update semantics) conditions:

- tagList omitted -> preserve tags
- tagList empty array -> clear tags
- tagList null -> reject
- tagList non-empty array -> replace tags

Mapped cases: `TC-010`,`TC-011`,`TC-012`,`TC-013`

## 5. Traceability

Traceability artifact:

- `Assignment 02/outputs/black-box-design/final/article-lifecycle/traceability.csv`

Mapping chain:

- `requirement_id -> coverage_item_id -> technique/analysis_id -> test_case_id`

Coverage status in final package:

- Requirements covered: 12/12
- Coverage status: all marked `Full`

## 6. Prompt Design

Skill definition:

- `Assignment 02/skills/black-box-design/SKILL.md`

Prompt version progression:

- v1.0: baseline generation (standalone mode)
- v1.1: risk-aware + auth-negative strengthening
- v1.2: revision-aware final freeze

Template used:

- `Assignment 02/skills/black-box-design/template.md`

## 7. Results Analysis

### 7.1 Baseline (Integrated, revision 0)

- Path: `Assignment 02/outputs/black-box-design/runs/article-lifecycle-integrated-run-20260602/baseline/`
- Cases: 16
- Key gap found during review: missing explicit integrated coverage for `R7`, `R10`, and `R12`.

### 7.2 Final (revision 2)

- Path: `Assignment 02/outputs/black-box-design/final/article-lifecycle/`
- Cases: 19
- Added auth/ownership negatives for update (`R7`) and delete (`R10`).
- Added explicit post-delete list observability coverage (`R12`).
- Refined post-delete expected result wording for detail + list verifiability.

## 8. Improvement with Evidence

Revision inputs:

- `Assignment 02/inputs/review/article-lifecycle-integrated-rev1.json`
- `Assignment 02/inputs/review/article-lifecycle-integrated-rev2.json`

Revision log:

- `Assignment 02/outputs/black-box-design/final/article-lifecycle/revision_log.json`

### 8.1 Before/After Summary

| Aspect                      | Baseline v1 | Final v2 | Improvement                               |
| --------------------------- | ----------- | -------- | ----------------------------------------- |
| Test case count             | 16          | 19       | Added `TC-019`,`TC-020`,`TC-021`          |
| Update auth/ownership neg   | Missing     | Explicit | Added dedicated `R7` branch (`EP-11`)     |
| Delete auth/ownership neg   | Missing     | Explicit | Added dedicated `R10` branch (`EP-12`)    |
| Post-delete list visibility | Missing     | Explicit | Added integrated `R12` observability case |

## 9. Handoff to Execution (Member5)

Handoff package:

- `Assignment 02/outputs/black-box-design/final/article-lifecycle/HANDOFF.md`
- `Assignment 02/outputs/black-box-design/final/article-lifecycle/test-cases.csv`
- `Assignment 02/outputs/black-box-design/final/article-lifecycle/traceability.csv`
- `Assignment 02/outputs/black-box-design/final/article-lifecycle/report.md`

Execution target remains implementation-neutral and can be applied to both ASP.NET Core and Golang-Gin implementations.

## 10. Skill Execution Record

Authoritative AI run:

- `Assignment 02/outputs/black-box-design/runs/article-lifecycle-integrated-run-20260602/`

Execution evidence:

- `Assignment 02/outputs/black-box-design/final/article-lifecycle/RUN_LOG.json`
- `Assignment 02/outputs/black-box-design/final/article-lifecycle/RUN_LOG.md`

Execution type:

- Mode: `ai_skill`
- Input mode: `integrated`
- Skill: `Assignment 02/skills/black-box-design/SKILL.md`

Pipeline result:

- Baseline AI generation: 16 cases (`revision=0`, prompt `v1.0`)
- Designer rev1: 19 cases (`revision=1`, prompt `v1.1`)
- Designer rev2 + final freeze: 19 cases (`revision=2`, prompt `v1.2`)
- Requirement coverage: `12/12` (`1.0`)

## 11. Sections Owned by Member5 (Placeholders)

### 11.1 White-Box Test Design

Completed by member5 at:

- `Assignment 02/outputs/white-box-modeling/article-lifecycle/`

### 11.2 Test Tool Implementation and Execution Results

Pending member5 execution against the updated integrated black-box final package.

### 11.3 Final Execution Result Analysis

Pending member5 test execution summary and defect/smoke analysis.
