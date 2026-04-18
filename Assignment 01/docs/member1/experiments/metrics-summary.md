# Prompt Iteration Metrics Summary (RealWorld + TodoMVC)

## Evaluation scope

- Input pack: `experiments/real-input-pack.md` (R1-R12)
- Sources:
  - RealWorld OpenAPI (`specs/api/openapi.yml`)
  - TodoMVC app spec (`app-spec.md`)

## Summary table

| Prompt version | Requirement coverage | Boundary hit count | Duplicate case rate | Executability |
| --- | --- | --- | --- | --- |
| v1 | 66.7% (8/12) | 6 | 22.2% (4/18) | 3.0/5 |
| v2 | 91.7% (11/12) | 12 | 10.0% (2/20) | 4.2/5 |
| v3 | 100.0% (12/12) | 17 | 4.3% (1/23) | 4.7/5 |

## Relative improvements

- v2 vs v1:
  - Coverage: `+25.0%`
  - Boundary hits: `+6`
  - Duplicate rate: `-12.2%`
  - Executability: `+1.2`
- v3 vs v2:
  - Coverage: `+8.3%`
  - Boundary hits: `+5`
  - Duplicate rate: `-5.7%`
  - Executability: `+0.5`
- v3 vs v1:
  - Coverage: `+33.3%`
  - Boundary hits: `+11`
  - Duplicate rate: `-17.9%`
  - Executability: `+1.7`

## Interpretation

1. Requirement-first extraction in v2 closes most missing branches from v1.
2. Boundary/error scan protocol in v3 gives the largest gain for API ownership/auth edges and UI routing edges.
3. Dedup guidance in v3 reduces redundant execution workload for member3.

## Recommendation

- Team default prompt: `prompt-v3.txt`
- Fallback prompt: `prompt-v2.txt` when output compactness is preferred
