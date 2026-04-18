# Run Record: Prompt v3 (RealWorld + TodoMVC)

## Version metadata

- Prompt file: `member1/prompts/prompt-v3.txt`
- Goal: maximize boundary/error coverage and minimize duplicates for mixed API/UI input.

## Input snapshot

- Dataset: `realworld-todomvc-core` (R1-R12)
- Same sources and protocol as v1/v2.

## Output behavior summary

- Strengths:
  - Boundary scan and negative scan are explicit and requirement-linked.
  - Auth/ownership/error branches are better separated (401/403/404/422/409).
  - UI routing/filter and mark-all synchronization are clearly testable.
  - Duplicate cases significantly reduced through dedup guidance.
- Weaknesses:
  - Output is longer and needs light normalization before script generation.

## Requirement mapping

| Requirement | Covered | Notes |
| --- | --- | --- |
| R1 | Yes | Registration success/conflict/validation complete |
| R2 | Yes | Credential positive/negative complete |
| R3 | Yes | Auth-required current user and update complete |
| R4 | Yes | Article create schema validation complete |
| R5 | Yes | Ownership and missing-resource mutation complete |
| R6 | Yes | Follow/favorite auth and toggle behavior complete |
| R7 | Yes | Filter and pagination boundaries complete |
| R8 | Yes | Comment create/delete auth/ownership complete |
| R9 | Yes | Create behavior and empty-input rule complete |
| R10 | Yes | Edit save/cancel/delete-on-empty complete |
| R11 | Yes | Mark-all and clear-completed synchronization complete |
| R12 | Yes | Route filters and selected-state transitions complete |

## Metrics

- Requirement coverage: `12/12 = 100.0%`
- Boundary hit count: `17`
- Duplicate case rate: `1/23 = 4.3%`
- Executability score: `4.7/5`

## Final observations

1. v3 is the recommended default prompt for real-input pipeline.
2. v2 remains acceptable fallback when shorter output is needed.
3. v1 is retained as structure sanity-check baseline.
