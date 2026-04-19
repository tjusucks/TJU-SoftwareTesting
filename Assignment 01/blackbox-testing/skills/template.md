# Black-Box Testing Run Report Template

## 1. Run Metadata

| Field                                    | Value                                                  |
| ---------------------------------------- | ------------------------------------------------------ |
| Project Name                             |                                                        |
| Feature Name                             |                                                        |
| Run ID                                   |                                                        |
| Date                                     |                                                        |
| Author / Operator                        |                                                        |
| Skill / Tool Name                        | `blackbox-testing`                                     |
| Model / Agent Version                    |                                                        |
| Prompt Version                           |                                                        |
| Input Type                               | Requirement / API Spec / UI Spec / User Story / Mixed  |
| Input Source Path / Link                 |                                                        |
| Target System / Implementation           |                                                        |
| Target Module / Endpoint / Feature Scope |                                                        |
| Execution Scope                          | Design Only / Design + Automation / Design + Execution |
| Notes                                    |                                                        |

## 2. Input Summary

### 2.1 Input Overview

- **Project / System Under Test**:
- **Feature Under Test**:
- **Actors**:
- **Preconditions**:
- **Business Rules**:
- **Input Constraints**:
- **Error Conditions**:

### 2.2 Requirement Items

| Requirement ID | Requirement Description | Priority | Notes |
| -------------- | ----------------------- | -------- | ----- |
| R1             |                         |          |       |
| R2             |                         |          |       |
| R3             |                         |          |       |

### 2.3 Assumptions About Input

- Assumption 1:
- Assumption 2:
- Assumption 3:

## 3. Test Design Strategy

### 3.1 Applied Black-Box Techniques

| Technique                   | Applied? | Where Used | Notes |
| --------------------------- | -------- | ---------- | ----- |
| Equivalence Partitioning    | Yes / No |            |       |
| Boundary Value Analysis     | Yes / No |            |       |
| Decision Table Testing      | Yes / No |            |       |
| State Transition Testing    | Yes / No |            |       |
| Error Guessing              | Yes / No |            |       |
| Scenario / Use-Case Testing | Yes / No |            |       |

### 3.2 Test Dimension Summary

- Valid input classes:
- Invalid input classes:
- Boundary values:
- Empty / null / missing cases:
- Format-related cases:
- Permission / role cases:
- State / sequencing cases:
- Combination cases:

### 3.3 Edge-Case Design Notes

- Edge category 1:
- Edge category 2:
- Edge category 3:

## 4. Equivalence Partitioning Analysis

| EP ID | Requirement ID | Input / Rule | Partition Type  | Description | Expected Outcome | Covered by Test Case ID |
| ----- | -------------- | ------------ | --------------- | ----------- | ---------------- | ----------------------- |
| EP1   |                |              | Valid / Invalid |             |                  |                         |
| EP2   |                |              |                 |             |                  |                         |
| EP3   |                |              |                 |             |                  |                         |

### 4.1 EP Coverage Notes

- Covered partitions:
- Missing partitions:
- Partially covered partitions:

## 5. Boundary Value Analysis

| BVA ID | Requirement ID | Boundary Item | Boundary Definition                 | Test Values | Expected Outcome | Covered by Test Case ID |
| ------ | -------------- | ------------- | ----------------------------------- | ----------- | ---------------- | ----------------------- |
| B1     |                |               | Min / Max / Just Below / Just Above |             |                  |                         |
| B2     |                |               |                                     |             |                  |                         |
| B3     |                |               |                                     |             |                  |                         |

### 5.1 BVA Coverage Notes

- Explicit boundaries tested:
- Missing boundaries:
- Ambiguous boundary definitions from requirements:

## 6. Test Scenarios

| Scenario ID | Requirement Reference | Scenario Title | Scenario Type                                                | Description | Priority            |
| ----------- | --------------------- | -------------- | ------------------------------------------------------------ | ----------- | ------------------- |
| S1          |                       |                | Happy Path / Negative / Boundary / Edge / State / Permission |             | High / Medium / Low |
| S2          |                       |                |                                                              |             |                     |
| S3          |                       |                |                                                              |             |                     |

## 7. Edge Case Matrix

| Requirement ID | Edge Category                                                                               | Concrete Case | Covered by Test Case ID | Notes |
| -------------- | ------------------------------------------------------------------------------------------- | ------------- | ----------------------- | ----- |
| R1             | Empty / Null / Missing / Boundary / Wrong Type / Duplicate / State / Sequence / Combination |               |                         |       |
| R2             |                                                                                             |               |                         |       |
| R3             |                                                                                             |               |                         |       |

## 8. Detailed Test Cases

| Test Case ID | Title | Requirement Reference | Preconditions | Test Data | Steps | Expected Result | Priority            | Risk / Notes |
| ------------ | ----- | --------------------- | ------------- | --------- | ----- | --------------- | ------------------- | ------------ |
| TC1          |       |                       |               |           |       |                 | High / Medium / Low |              |
| TC2          |       |                       |               |           |       |                 |                     |              |
| TC3          |       |                       |               |           |       |                 |                     |              |

## 9. How to Run the Generated Test Codes

### 9.1 Prerequisites

- Target system is available and can be started locally or in a test environment.
- Required runtime, package manager, and test framework are installed.
- Required environment variables, credentials, and test data are prepared.
- Any dependent services, databases, or mock services are available.

### 9.2 Test Code Location

- **Generated Test Code Path**:
- **Project Root Path**:
- **Test Entry File / Directory**:
- **Related Configuration Files**:

### 9.3 Environment Setup

(Working Directory, Runtime Version, Dependency Install Command, Build Command, Test Environment Variables, Test Data / Seed Command, Service Start Command, etc.)

### 9.4 Run Commands

(Commands to run all tests/specific tests, with or without cache, with coverage collection, etc.)

### 9.5 Execution Notes

- Start the target service before running black-box tests, if the tests depend on a live system.
- Use isolated test data, dedicated test accounts, or a separate test database when possible.
- If the test framework caches results, force re-execution when needed, for example by disabling cache or using a no-cache flag such as `-count=1` for Go tests.
- Record the exact command, environment variables, and target host used for reproducibility.
- If some steps are manual or environment-specific, document them explicitly.

## 10. Coverage Summary

### 10.1 Requirement Coverage Table

| Requirement ID | EP Covered?        | BVA Covered?       | Edge Case Covered? | Negative Case Covered? | State / Sequence Covered? | Covered by Test Cases | Coverage Status          | Notes |
| -------------- | ------------------ | ------------------ | ------------------ | ---------------------- | ------------------------- | --------------------- | ------------------------ | ----- |
| R1             | Yes / No / Partial | Yes / No / Partial | Yes / No / Partial | Yes / No / Partial     | Yes / No / Partial        | TC1, TC2              | Full / Partial / Missing |       |
| R2             |                    |                    |                    |                        |                           |                       |                          |       |
| R3             |                    |                    |                    |                        |                           |                       |                          |       |

### 10.2 EP / BVA to Test Case Mapping

| Analysis Item ID | Type | Requirement ID | Description | Mapped Test Case ID(s) | Covered?           | Notes |
| ---------------- | ---- | -------------- | ----------- | ---------------------- | ------------------ | ----- |
| EP1              | EP   |                |             |                        | Yes / No / Partial |       |
| EP2              | EP   |                |             |                        |                    |       |
| B1               | BVA  |                |             |                        |                    |       |
| B2               | BVA  |                |             |                        |                    |       |

### 10.3 Coverage Metrics

| Metric                 | Formula                                              | Value |
| ---------------------- | ---------------------------------------------------- | ----- |
| Requirement Coverage   | covered_requirements / total_requirements            |       |
| EP Coverage            | covered_partitions / total_partitions                |       |
| BVA Coverage           | covered_boundaries / total_boundaries                |       |
| Edge Case Coverage     | covered_edge_categories / applicable_edge_categories |       |
| Negative Case Coverage | negative_cases_present / applicable_requirements     |       |
| Duplicate Case Rate    | duplicate_cases / total_cases                        |       |
| Executability Score    | 1-5                                                  |       |

### 10.4 Coverage Notes

- Strongest covered area:
- Weakest covered area:
- Over-covered or duplicated areas:
- Under-covered areas:

## 11. Ambiguities / Missing Information / Assumptions

### 11.1 Ambiguous Requirements

- Item 1:
- Item 2:

### 11.2 Missing Information

- Missing validation rule:
- Missing boundary definition:
- Missing error behavior:
- Missing state transition rule:

### 11.3 Assumptions

- Assumption 1:
- Assumption 2:
- Assumption 3:
