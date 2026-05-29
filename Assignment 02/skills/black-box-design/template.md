# Assignment 02 Black-Box Design Report Template

## 1. Run Metadata

| Field             | Value                                  |
| ----------------- | -------------------------------------- |
| Project Name      |                                        |
| Feature Name      |                                        |
| Feature ID        |                                        |
| Run ID            |                                        |
| Date              |                                        |
| Author / Operator |                                        |
| Skill / Tool Name | `black-box-design`                     |
| Prompt Version    |                                        |
| Input Mode        | standalone / integrated / orchestrated |
| Input Source(s)   |                                        |
| Revision          |                                        |
| Notes             |                                        |

## 2. Scope and Concept

### 2.1 Feature Summary

- System under test:
- Feature scope:
- In-scope endpoints/actions:
- Out-of-scope items:

### 2.2 Actors and Preconditions

- Actors:
- Preconditions:
- Dependencies:

## 3. Coverage Item Identification

| Coverage Item ID | Requirement ID(s) | Technique Focus    | Description | Priority        |
| ---------------- | ----------------- | ------------------ | ----------- | --------------- |
| CI-1             |                   | EP/BVA/DT/Scenario |             | High/Medium/Low |

### 3.1 Coverage Item Notes

- Why these coverage items:
- Deferred coverage items (if any):

## 4. Coverage Strategy and Method

| Requirement ID | Selected Techniques | Strategy Notes |
| -------------- | ------------------- | -------------- |
| R1             | EP / BVA / DT       |                |

### 4.1 Strategy Rationale

- EP rationale:
- BVA rationale:
- DT rationale:
- Scenario rationale:

## 5. Equivalence Partitioning Analysis

| EP ID | Requirement ID | Input / Rule | Partition Type | Description | Expected Outcome | Covered by Test Case ID |
| ----- | -------------- | ------------ | -------------- | ----------- | ---------------- | ----------------------- |
| EP-1  |                |              | Valid/Invalid  |             |                  |                         |

### 5.1 EP Coverage Notes

- Covered partitions:
- Missing partitions:
- Partially covered partitions:

## 6. Boundary Value Analysis

| BVA ID | Requirement ID | Boundary Item | Boundary Definition           | Test Values | Expected Outcome | Covered by Test Case ID |
| ------ | -------------- | ------------- | ----------------------------- | ----------- | ---------------- | ----------------------- |
| BVA-1  |                |               | Min/Max/Just Below/Just Above |             |                  |                         |

### 6.1 BVA Coverage Notes

- Explicit boundaries tested:
- Missing boundaries:
- Ambiguous boundary definitions:

## 7. Decision Table Analysis

| DT ID | Requirement ID | Conditions | Rule Combinations | Expected Outcomes | Covered by Test Case ID |
| ----- | -------------- | ---------- | ----------------- | ----------------- | ----------------------- |
| DT-1  |                |            |                   |                   |                         |

### 7.1 Decision Table Notes

- Rule completeness:
- Conflict handling:
- Missing combinations:

## 8. Test Scenarios

| Scenario ID | Requirement ID(s) | Scenario Title | Scenario Type                                                | Description | Priority        |
| ----------- | ----------------- | -------------- | ------------------------------------------------------------ | ----------- | --------------- |
| SC-01       |                   |                | Happy Path / Negative / Boundary / Edge / State / Permission |             | High/Medium/Low |

## 9. Detailed Test Cases

| Test Case ID | Title | Requirement Reference | Preconditions | Test Data | Steps | Expected Result | Priority        | Risk / Notes |
| ------------ | ----- | --------------------- | ------------- | --------- | ----- | --------------- | --------------- | ------------ |
| TC-001       |       |                       |               |           |       |                 | High/Medium/Low |              |

## 10. Traceability Matrix

| Requirement ID | Coverage Item ID | Technique | Analysis ID     | Test Case ID(s) | Coverage Status      | Notes |
| -------------- | ---------------- | --------- | --------------- | --------------- | -------------------- | ----- |
| R1             | CI-1             | EP/BVA/DT | EP-1/BVA-1/DT-1 | TC-001          | Full/Partial/Missing |       |

## 11. Coverage Summary and Metrics

### 11.1 Requirement Coverage

| Requirement ID | EP Covered?    | BVA Covered?   | DT Covered?    | Negative Covered? | Covered by Test Cases | Coverage Status      |
| -------------- | -------------- | -------------- | -------------- | ----------------- | --------------------- | -------------------- |
| R1             | Yes/No/Partial | Yes/No/Partial | Yes/No/Partial | Yes/No/Partial    | TC-001                | Full/Partial/Missing |

### 11.2 Metrics

| Metric               | Formula                                   | Value |
| -------------------- | ----------------------------------------- | ----- |
| Requirement Coverage | covered_requirements / total_requirements |       |
| EP Coverage          | covered_ep / total_ep                     |       |
| BVA Coverage         | covered_bva / total_bva                   |       |
| DT Coverage          | covered_dt / total_dt                     |       |
| Duplicate Case Rate  | duplicate_cases / total_cases             |       |

### 11.3 Coverage Notes

- Strongest area:
- Weakest area:
- Under-covered risks:

## 12. Review and Revision Log

| Revision | Source File | Change Summary | Impacted IDs | Outcome |
| -------- | ----------- | -------------- | ------------ | ------- |
| 1        |             |                |              |         |

### 12.1 Before/After Evidence

- Added:
- Modified:
- Removed:

## 13. Ambiguities / Missing Information / Assumptions

### 13.1 Ambiguities

- Item 1:

### 13.2 Missing Information

- Missing rule:

### 13.3 Assumptions

- Assumption 1:

## 14. Export Index

- `artifacts.json`:
- `test-cases.csv`:
- `traceability.csv`:
- `revision_log.json`:
