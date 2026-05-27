# Assignment 02 Black-Box Design Report (AI Final)

## 1. Run Metadata

| Field | Value |
| --- | --- |
| Project Name | RealWorld |
| Feature Name | Article Lifecycle |
| Feature ID | article-lifecycle |
| Run ID | A2-BB-AI-ARTICLE-LIFECYCLE-R2 |
| Date | 2026-05-27 |
| Generated At | 2026-05-27T16:09:51+08:00 |
| Author / Operator | Member4 |
| Execution Mode | AI skill (`Cursor Agent` + `black-box-design`) |
| Skill / Tool Name | `black-box-design` |
| Prompt Version | v1.2 |
| Input Mode | standalone |
| Input Source(s) | feature spec + structured requirements + risk + rev1 + rev2 |
| Revision | 2 |
| Notes | AI-generated final package following SKILL.md workflow |

## 2. Scope and Concept

### 2.1 Feature Summary

- System under test: RealWorld backend API contract
- Feature scope: authenticated article create, update, delete, and persistence observability
- In-scope endpoints: `POST /articles`, `PUT /articles/{slug}`, `DELETE /articles/{slug}`, `GET /articles/{slug}`
- Out-of-scope: source code internals, database schema, unrelated modules

### 2.2 Actors and Preconditions

- Actors: authenticated owner, authenticated non-owner, unauthenticated user
- Preconditions: API reachable, registered user exists, token available for authenticated operations
- Dependencies: contract-level response observability only

## 3. Coverage Item Identification

| Coverage Item ID | Requirement ID(s) | Technique Focus | Description | Priority |
| --- | --- | --- | --- | --- |
| CI-1 | R1, R8 | EP | Create authentication gate | High |
| CI-2 | R2, R3 | EP | Valid create response contract | High |
| CI-3 | R4 | EP | Duplicate title with unique slug | Medium |
| CI-4 | R5 | EP | Empty title rejection | High |
| CI-5 | R6 | EP | Empty description rejection | High |
| CI-6 | R7 | EP | Empty body rejection | High |
| CI-7 | R9, R10 | EP | Owner update and partial update preservation | High |
| CI-8 | R11, R12, R13 | DT | tagList update decision combinations | High |
| CI-9 | R14 | Scenario | Update persistence via follow-up GET | Medium |
| CI-10 | R15 | EP | Owner delete success | High |
| CI-11 | R16 | Scenario | Post-delete retrieval failure | Medium |
| CI-12 | R9, R15 | EP | Unauthorized update/delete negatives | High |

## 4. Coverage Strategy and Method

High-risk requirements (`R1`, `R2`, `R5`-`R7`, `R9`, `R11`-`R13`, `R15`) drive High-priority cases. Multi-condition tagList semantics are modeled with Decision Table `DT-1`. Empty/non-empty field boundaries use BVA on create and tagList update paths.

## 5. Equivalence Partitioning Analysis

| EP ID | Requirement ID | Description | Test Case ID |
| --- | --- | --- | --- |
| EP-1 | R1, R8 | Missing auth on create | TC-001 |
| EP-2 | R2 | Valid create payload | TC-002 |
| EP-3 | R3 | Complete response fields | TC-003 |
| EP-4 | R4 | Duplicate title accepted with distinct slug | TC-004 |
| EP-5 | R5 | Empty title invalid | TC-005 |
| EP-6 | R6 | Empty description invalid | TC-006 |
| EP-7 | R7 | Empty body invalid | TC-007 |
| EP-8 | R9 | Owner update success | TC-008 |
| EP-9 | R10 | Partial update preserves omitted fields | TC-009 |
| EP-10 | R15 | Owner delete success | TC-014 |
| EP-11 | R9 | Missing auth on update | TC-017 |
| EP-12 | R15 | Missing auth on delete | TC-018 |

## 6. Boundary Value Analysis

| BVA ID | Requirement ID | Boundary | Test Case ID |
| --- | --- | --- | --- |
| BVA-1 | R4 | Same title reused twice | TC-004 |
| BVA-2 | R5 | Empty vs non-empty title | TC-005, TC-002 |
| BVA-3 | R6 | Empty vs non-empty description | TC-006, TC-002 |
| BVA-4 | R7 | Empty vs non-empty body | TC-007, TC-002 |
| BVA-5 | R12 | tagList empty array | TC-011 |
| BVA-6 | R13 | tagList null | TC-012 |

## 7. Decision Table Analysis

`DT-1` covers `R11`, `R12`, `R13`:

| Condition | Expected Outcome | Test Case ID |
| --- | --- | --- |
| tagList omitted | preserve existing tags | TC-010 |
| tagList = [] | clear all tags | TC-011 |
| tagList = null | reject request | TC-012 |
| tagList = non-empty array | replace tags | TC-013 |

## 8. Test Scenarios

18 scenarios (`SC-01` to `SC-14`) mapped to happy path, negative, boundary, and state/persistence flows. See `artifacts.json`.

## 9. Detailed Test Cases

Final set: 18 cases (`TC-001` to `TC-018`). See `test-cases.csv`.

## 10. Traceability Matrix

All 16 requirements mapped with `Full` coverage. See `traceability.csv`.

## 11. Coverage Summary and Metrics

| Metric | Value |
| --- | --- |
| Requirement Coverage | 16/16 = 1.0 |
| EP Coverage | 1.0 |
| BVA Coverage | 1.0 |
| DT Coverage | 1.0 |
| Duplicate Case Rate | 0.0 |

## 12. Review and Revision Log

| Revision | Source | Summary |
| --- | --- | --- |
| 1 | `article-lifecycle-rev1.json` | Added CI-12, TC-017, TC-018 |
| 2 | `article-lifecycle-rev2.json` | Refined TC-016 expected result |

## 13. Ambiguities / Assumptions

- Exact unauthorized/validation status codes and error envelopes are implementation-specific.
- Post-delete not-found contract is assumed observable via 404 or equivalent not-found semantics.

## 14. Export Index

- `artifacts.json`
- `test-cases.csv`
- `traceability.csv`
- `revision_log.json`
- `RUN_LOG.json`

## 15. AI Execution Evidence

Generated by Cursor Agent executing `Assignment 02/skills/black-box-design/SKILL.md` on 2026-05-27. Baseline AI generation produced 16 cases; designer review rounds raised the final set to 18 cases.
