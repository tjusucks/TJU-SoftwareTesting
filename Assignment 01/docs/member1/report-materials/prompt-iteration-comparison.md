# Prompt Iteration Comparison (RealWorld + TodoMVC)

## Design change log

| Version | Main change | Intended effect |
| --- | --- | --- |
| v1 | Fixed 7-section output + mandatory fields | Stable baseline for mixed API/UI parsing |
| v2 | Requirement-first extraction + explicit mapping + uncovered reporting | Improve traceability and reduce missing requirements |
| v3 | Boundary/negative scan + dedup rule | Improve branch completeness and execution efficiency |

## Quantitative comparison

| Version | Coverage | Boundary hits | Duplicate rate | Executability |
| --- | --- | --- | --- | --- |
| v1 | 66.7% | 6 | 22.2% | 3.0/5 |
| v2 | 91.7% | 12 | 10.0% | 4.2/5 |
| v3 | 100.0% | 17 | 4.3% | 4.7/5 |

## Observed differences on real inputs

- RealWorld API branches:
  - v1 often merges ownership and auth errors.
  - v2 separates most 401/403/404/422 cases.
  - v3 adds stronger boundary and conflict partitions.
- TodoMVC UI branches:
  - v1 covers basic create/edit only.
  - v2 adds most lifecycle branches.
  - v3 captures route/filter transitions and state synchronization more fully.

## Decision

- Team default prompt: `prompt-v3.txt`
- Fallback for compact output: `prompt-v2.txt`
- Baseline regression reference: `prompt-v1.txt`
