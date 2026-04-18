# Limitations and Improvements (Real Input Version)

## Current limitations

1. Specification-to-requirement translation risk
   - RealWorld OpenAPI and TodoMVC spec are rich, but requirement extraction still needs reviewer interpretation.

2. Mixed-domain complexity
   - Combining API and UI requirements increases branch diversity and scoring complexity.

3. Manual scoring elements
   - Duplicate-rate and executability scoring still include human judgment.

4. Verbose high-coverage output
   - v3 output quality is high but may require formatting cleanup before direct script conversion.

## Improvements applied

1. Real input replacement
   - Replaced synthetic scenario with RealWorld+TodoMVC source-linked requirement pack.

2. Fixed scoring protocol
   - Unified formulas for coverage, boundary hit, duplicate rate, and executability across v1/v2/v3.

3. Branch-focused prompt evolution
   - v2 adds requirement-level traceability.
   - v3 adds explicit boundary/negative scan and dedup constraints.

4. Handoff alignment
   - Output remains consistent with member2/member3/member4 contracts and execution/evaluation needs.

## Next improvements

1. Add automatic parser validation for:
   - required section presence,
   - required columns in Detailed Test Cases,
   - requirement-to-case mapping completeness.
2. Add a second realworld backend implementation for cross-implementation robustness checks.
3. Add structured tags in generated cases (`auth`, `ownership`, `validation`, `routing`) to speed downstream execution mapping.

## Comparison with traditional non-AI approach

- Advantages:
  - Faster scenario expansion over broad API/UI requirements.
  - Prompt iteration quickly improves branch completeness.
- Limitations:
  - Requires strong prompt constraints and strict review protocol to maintain consistency.

## Final note

On real project sources, prompt engineering clearly improves coverage and usability, but stable delivery still depends on frozen I/O contracts and disciplined evaluation workflows.
