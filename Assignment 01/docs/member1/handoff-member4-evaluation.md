# Handoff to Member4 (Evaluation and Validation)

## Objective

Use one consistent metric definition for all prompt versions and benchmark features.

## Metric definitions (must keep unchanged)

1. Requirement coverage
   - `covered_requirements / total_requirements`
   - A requirement is covered if at least one executable case maps to it.

2. Boundary hit count
   - Count explicit boundary checks (`min-1/min/min+1`, `max-1/max/max+1`, format/time boundaries).

3. Duplicate case rate
   - `duplicate_cases / total_cases`
   - Duplicate means same data class and expected behavior intent.

4. Executability
   - 1 to 5 score based on direct run-readiness.

## Data sources

- Protocol: `member1/experiments/experiment-protocol.md`
- Input pack: `member1/experiments/real-input-pack.md`
- Per-run records:
  - `member1/experiments/run-v1.md`
  - `member1/experiments/run-v2.md`
  - `member1/experiments/run-v3.md`
- Summary:
  - `member1/experiments/metrics-summary.md`

## Acceptance criteria

1. Same requirement pack and rubric used across v1/v2/v3.
2. Coverage matrix is traceable to `Requirement Reference`.
3. Reported improvements are backed by numeric deltas.

## Common failure handling

- Coverage disagreement between reviewers:
  - action: review requirement-case mapping table and resolve by majority or tie-break note.
- Executability score variance:
  - action: use two-pass scoring and average.
- Invalid comparison due to changed input:
  - action: mark run as non-comparable and exclude from final delta chart.
