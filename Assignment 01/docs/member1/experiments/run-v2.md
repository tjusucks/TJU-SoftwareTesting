# Run Record: Prompt v2 (RealWorld + TodoMVC)

## Version metadata

- Prompt file: `member1/prompts/prompt-v2.txt`
- Goal: improve requirement-to-case traceability and reduce missed requirements.

## Input snapshot

- Dataset: `realworld-todomvc-core` (R1-R12)
- Same sources and scoring protocol as v1.

## Output behavior summary

- Strengths:
  - Requirement extraction is explicit and easier to audit.
  - Most API auth/error branches are mapped by requirement ID.
  - UI scenarios are grouped by lifecycle and routing state.
- Weaknesses:
  - Some boundary checks still aggregate multiple limits in one case.
  - A few duplicate cases remain for invalid request payloads.

## Requirement mapping

| Requirement | Covered | Notes |
| --- | --- | --- |
| R1 | Yes | Registration conflict/validation expanded |
| R2 | Yes | Invalid credential branch clearer |
| R3 | Yes | Missing token and update path both covered |
| R4 | Yes | Required-field invalid variants added |
| R5 | Yes | Ownership mutation split included |
| R6 | Yes | Follow/favorite toggle plus auth path included |
| R7 | Yes | Pagination and filter baseline covered |
| R8 | Yes | Comment ownership and not-found cases included |
| R9 | Yes | Empty-input and trim rules explicit |
| R10 | Yes | Blur/enter/escape paths explicit |
| R11 | Yes | Mark-all and clear-completed covered |
| R12 | No | Route-state persistence after updates partly implicit |

## Metrics

- Requirement coverage: `11/12 = 91.7%`
- Boundary hit count: `12`
- Duplicate case rate: `2/20 = 10.0%`
- Executability score: `4.2/5`

## Main issues found

1. Route filter persistence after list mutation still needs stronger assertions.
2. Some boundary combinations are grouped rather than isolated.

## Actions for v3

- Force explicit boundary scan matrix and coverage check.
- Force negative/error scan matrix for auth/ownership/validation.
- Apply dedup preference for highest-risk scenario per data class.
