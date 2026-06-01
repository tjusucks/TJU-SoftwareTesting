# White-Box Modeling Report Template

## 1. Run Metadata

| Field             | Value                                  |
| ----------------- | -------------------------------------- |
| Project Name      |                                        |
| Feature Name      |                                        |
| Feature ID        |                                        |
| Run ID            |                                        |
| Date              |                                        |
| Author / Operator |                                        |
| Skill / Tool Name | `white-box-modeling`                   |
| Prompt Version    |                                        |
| Input Mode        | standalone / integrated / orchestrated |
| Input Source(s)   |                                        |
| Revision          |                                        |
| Notes             |                                        |

## 2. Scope and Model Selection

- Target module:
- Model type: State Transition / Path / Control Flow
- Rationale for model choice:
- In-scope functions or endpoints:
- Out-of-scope items:

## 3. Internal Model

### 3.1 States (if state model)

| State ID | Name | Description | Entry Conditions | Exit Conditions |
| -------- | ---- | ----------- | ---------------- | --------------- |
| S1       |      |             |                  |                 |

### 3.2 Transitions (if state model)

| Transition ID | From | To  | Guard / Condition | Action | Requirement ID(s) |
| ------------- | ---- | --- | ----------------- | ------ | ----------------- |
| T1            |      |     |                   |        |                   |

### 3.3 Nodes and Edges (if path/control-flow model)

| Node ID | Type (Decision/Action) | Description | Requirement ID(s) |
| ------- | ---------------------- | ----------- | ----------------- |
| N1      |                        |             |                   |

| Edge ID | From | To  | Condition | Notes |
| ------- | ---- | --- | --------- | ----- |
| E1      |      |     |           |       |

## 4. Coverage Strategy

| Coverage Target | Strategy | Notes |
| --------------- | -------- | ----- |
| State           |          |       |
| Transition      |          |       |
| Branch/Decision |          |       |
| Path            |          |       |

## 5. White-Box Paths

| Path ID    | Type | Steps | Coverage Focus | Requirement ID(s) | Test Intent | Expected Internal Effect |
| ---------- | ---- | ----- | -------------- | ----------------- | ----------- | ------------------------ |
| WB-PATH-01 |      |       |                |                   |             |                          |

## 6. Traceability

| Requirement ID | Related States/Nodes | Related Transition/Edge | Path ID(s) | Coverage Status      |
| -------------- | -------------------- | ----------------------- | ---------- | -------------------- |
| R1             |                      |                         |            | Full/Partial/Missing |

## 7. Coverage Summary and Metrics

### 7.1 Coverage Summary

- Strongest internal coverage area:
- Weakest internal coverage area:
- Under-covered decisions:

### 7.2 Metrics

| Metric              | Formula                                 | Value |
| ------------------- | --------------------------------------- | ----- |
| State Coverage      | covered_states / total_states           |       |
| Transition Coverage | covered_transitions / total_transitions |       |
| Decision Coverage   | covered_decisions / total_decisions     |       |
| Path Coverage       | covered_paths / total_paths             |       |

## 8. Assumptions / Missing Information / Open Questions

### 8.1 Assumptions

- Assumption 1:

### 8.2 Missing Information

- Missing rule:

### 8.3 Open Questions

- Question 1:

## 9. Review and Revision Log

| Revision | Source File | Change Summary | Impacted IDs | Outcome |
| -------- | ----------- | -------------- | ------------ | ------- |
| 1        |             |                |              |         |

### 9.1 Before/After Evidence

- Added:
- Modified:
- Removed:

## 10. Export Index

- `artifacts.json`:
- `revision_log.json`:
