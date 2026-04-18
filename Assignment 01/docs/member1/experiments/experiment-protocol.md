# Experiment Protocol for Prompt Iteration (Real Input)

## Purpose

Compare `prompt-v1`, `prompt-v2`, and `prompt-v3` using real requirement sources from RealWorld and TodoMVC, under one fixed scoring protocol.

## Fixed setup

- Model path: Claude Code routed to DeepSeek (`deepseek-chat` default).
- Input set: `real-input-pack.md` (R1-R12), built from:
  - RealWorld OpenAPI spec (`specs/api/openapi.yml`)
  - TodoMVC application spec (`app-spec.md`)
- Output contract: 7 sections + required test-case fields from `SKILL.md` and `02-io-templates-freeze.md`.
- Evaluation mode: requirement-trace review with fixed formulas.

## Input under test

Requirement pack `realworld-todomvc-core` (R1-R12):
- RealWorld API behaviors: user registration/login/current user, article CRUD ownership, follow/favorite toggles, comments, filtering/pagination, auth/validation/not-found/forbidden responses.
- TodoMVC UI behaviors: create/edit/delete lifecycle, mark-all and clear-completed state sync, route filtering (`#/`, `#/active`, `#/completed`), empty-input and trim rules.

## Metrics and formulas

1. Requirement coverage
   - `covered_requirements / total_requirements`
   - covered = at least one executable case references the requirement ID.

2. Boundary hit count
   - count of explicit boundary checks.
   - includes pagination (`limit`, `offset`) boundaries and UI state/route boundary transitions.

3. Duplicate case rate
   - `duplicate_cases / total_cases`
   - duplicate = same intent + same data class + same expected behavior.

4. Executability score
   - rubric 1 to 5.
   - 5 means direct conversion into Newman/Playwright assertions with minimal clarification.

## Evaluation process

1. Use the same R1-R12 input with one prompt version.
2. Normalize output to the frozen table schema.
3. Build requirement-to-case trace matrix.
4. Compute four metrics.
5. Repeat for v1/v2/v3 and compare deltas.

## Threats to validity

- Requirement extraction from documentation is still reviewer-mediated.
- Metric values reflect prompt quality and test design quality, not full E2E pass rate.
- Cross-domain input (API + UI) improves generalizability but also increases scoring complexity.
