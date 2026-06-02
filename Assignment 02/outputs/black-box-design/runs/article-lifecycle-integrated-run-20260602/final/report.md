# Assignment 02 Black-Box Design Report (Integrated Final)

## 1. Run Metadata

| Field | Value |
| --- | --- |
| Project Name | RealWorld |
| Feature Name | Article Lifecycle |
| Feature ID | article-lifecycle |
| Run ID | A2-BB-ARTICLE-LIFECYCLE-INT-R2 |
| Date | 2026-06-02 |
| Generated At | 2026-06-02T12:02:00+08:00 |
| Author / Operator | Member4 |
| Execution Mode | AI skill (`black-box-design`) |
| Prompt Version | v1.2 |
| Input Mode | integrated |
| Input Source(s) | `outputs/structured-requirements/article-lifecycle.json` + `outputs/risk-analysis/article-lifecycle/risk-analysis.json` + integrated reviews |
| Revision | 2 |

## 2. Coverage Summary

- Requirement coverage: **12 / 12** (`1.0`)
- Techniques included: EP, BVA, DT, Scenario
- Final case count: **19**

## 3. Review Effects

- Revision 1 added explicit negative checks for integrated `R7` / `R10` and list observability coverage for `R12`.
- Revision 2 refined `TC-021` expected result to require both detail and list non-observability with implementation-neutral wording.

## 4. Equivalence Partitions (EP)

| ID | Requirements | Description | Test Cases |
| --- | --- | --- | --- |
| EP-1 | R1 | Missing auth token on create | TC-001 |
| EP-2 | R2 | Valid create payload accepted | TC-002 |
| EP-3 | R3 | Create response includes required fields | TC-003 |
| EP-4 | R2 | Duplicate title still accepted | TC-004 |
| EP-5 | R4 | Invalid create values rejected | TC-005, TC-006, TC-007 |
| EP-8 | R5 | Owner update success | TC-008 |
| EP-9 | R6 | Partial update preserves omitted fields | TC-009 |
| EP-10 | R8 | Owner delete success | TC-014 |
| EP-11 | R7 | Update rejected for unauthenticated or non-owner actor | TC-019 |
| EP-12 | R10 | Delete rejected for unauthenticated or non-owner actor | TC-020 |

## 5. Boundary Value Analysis (BVA)

| ID | Requirements | Description | Test Cases |
| --- | --- | --- | --- |
| BVA-1 | R2 | Duplicate title boundary | TC-004 |
| BVA-2 | R4 | Title empty vs non-empty | TC-005, TC-002 |
| BVA-3 | R4 | Description empty vs non-empty | TC-006, TC-002 |
| BVA-4 | R4 | Body empty vs non-empty | TC-007, TC-002 |
| BVA-5 | R6 | tagList empty array boundary | TC-011 |
| BVA-6 | R6 | tagList null boundary | TC-012 |

## 6. Decision Table (DT)

| ID | Requirements | Conditions | Rules (expected) | Test Cases |
| --- | --- | --- | --- | --- |
| DT-1 | R6 | tagList omitted / `[]` / `null` / non-empty array | omit → preserve tags; empty → clear; null → reject; replace → new tags | TC-010, TC-011, TC-012, TC-013 |

## 7. Export Index

- `artifacts.json`
- `test-cases.csv`
- `traceability.csv`
- `revision_log.json`
- `RUN_LOG.json`
