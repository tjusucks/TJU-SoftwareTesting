# Run Record: Prompt v1 (RealWorld + TodoMVC)

## Version metadata

- Prompt file: `member1/prompts/prompt-v1.txt`
- Goal: validate baseline structure on real API/UI mixed requirements.

## Input snapshot

- Dataset: `realworld-todomvc-core` (R1-R12)
- Source references:
  - RealWorld `specs/api/openapi.yml`
  - TodoMVC `app-spec.md`

## Output behavior summary

- Strengths:
  - Keeps the required 7-section contract.
  - Generates usable baseline cases for login, article creation, and todo create/edit.
  - Distinguishes auth-required from public endpoints at basic level.
- Weaknesses:
  - Some ownership/error branches are not explicit (403/404 separation).
  - Pagination and route-filter boundaries are partially covered only.
  - Several API negative cases overlap in intent.

## Requirement mapping

| Requirement | Covered | Notes |
| --- | --- | --- |
| R1 | Yes | Registration positive and duplicate baseline present |
| R2 | Yes | Login positive/negative present |
| R3 | Yes | Missing-token unauthorized covered |
| R4 | Yes | Article creation baseline covered |
| R5 | No | Owner vs non-owner mutation not complete |
| R6 | Yes | Follow/favorite basic toggling covered |
| R7 | No | Pagination boundaries incomplete |
| R8 | Yes | Comment create/delete baseline present |
| R9 | Yes | Todo create and trim behavior covered |
| R10 | Yes | Edit save/cancel baseline covered |
| R11 | No | Mark-all sync and clear-completed details incomplete |
| R12 | No | Route-state persistence and filter-state updates incomplete |

## Metrics

- Requirement coverage: `8/12 = 66.7%`
- Boundary hit count: `6`
- Duplicate case rate: `4/18 = 22.2%`
- Executability score: `3.0/5`

## Main issues found

1. Ownership/authorization branches are not sufficiently partitioned.
2. API pagination and UI routing boundaries need explicit near-boundary tests.
3. Duplicate invalid-input scenarios increase downstream execution cost.

## Actions for v2

- Enforce requirement extraction and explicit mapping first.
- Require uncovered requirement reporting with reason.
- Add mandatory valid+invalid case per high-priority requirement.
