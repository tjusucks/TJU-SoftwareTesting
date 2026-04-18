# Handoff to Member2 (Codebase/Spec Preparation)

## Objective

Provide normalized requirement input that is fully compatible with `RequirementInputV1`.

## Input contract to follow

Reference: `member1/02-io-templates-freeze.md`

Required top-level fields:
- `project_name`
- `feature_name`
- `requirement_items` (each item has `id` and `text`)

Recommended optional fields:
- `actors`
- `preconditions`
- `business_rules`
- `input_constraints`
- `error_conditions`
- `priority`

## Acceptance criteria

1. Each requirement is atomic and testable.
2. Requirement IDs are stable (`R1`, `R2`, ...).
3. Numeric, format, and timing constraints are explicit.
4. Invalid/exception behavior is not merged into vague prose.

## Common failure handling

- Missing requirement ID:
  - action: assign temporary ID and mark in data notes before handoff.
- Mixed API/UI statements in one requirement:
  - action: split into separate requirement items.
- Hidden assumptions in source docs:
  - action: add to `error_conditions` or `business_rules` explicitly.

## Delivery format

- One markdown or yaml file per feature, using `RequirementInputV1`.
- Share with member1 and member3 before script conversion starts.

## Current real-input baseline

- Active requirement pack: `member1/experiments/real-input-pack.md`
- RealWorld source: `specs/api/openapi.yml`
- TodoMVC source: `app-spec.md`
