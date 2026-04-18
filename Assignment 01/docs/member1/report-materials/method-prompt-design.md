# Method and Prompt Design (Real Input Version)

## Method overview

This project applies an LLM-based black-box testing workflow on real, externally documented requirements:

- RealWorld API specification (`specs/api/openapi.yml`) for backend/API scenarios.
- TodoMVC application specification (`app-spec.md`) for UI behavior scenarios.

The workflow remains implementation-independent and converts requirement text into structured test artifacts using the fixed `RequirementInputV1` contract.

## Input strategy

We use a mixed input pack (`R1-R12`) to evaluate both API and UI black-box capabilities:

- API axis: auth, ownership control, validation errors, filtering and pagination.
- UI axis: create/edit/delete lifecycle, mark-all synchronization, route-based filtering.

This avoids overfitting to a single feature family and better supports generalizability claims.

## Prompt design strategy

Prompt evolution is purpose-driven:

- v1: lock output shape (7 sections + mandatory case fields).
- v2: enforce requirement-first extraction and explicit mapping.
- v3: enforce boundary and negative scan matrices plus dedup rule.

The objective is to improve coverage, reduce redundant cases, and increase direct executability in Newman/Playwright conversion.

## Reproducibility controls

- Same real input pack for v1/v2/v3.
- Same metric formulas and scoring rubric.
- Same section/field contract for all runs.
- Same model routing path.

## Key result

On the RealWorld+TodoMVC input pack, v3 provides the best overall quality and is selected as default; v2 is retained as compact fallback.
