# Handoff to Member3 (Execution Pipeline)

## Objective

Convert generated black-box outputs into executable Postman/Newman or Playwright assets.

## Output contract to consume

Reference: `member1/02-io-templates-freeze.md`

Required section order:
1. `Feature Summary`
2. `Requirements Extracted`
3. `Test Design Strategy`
4. `Test Scenarios`
5. `Detailed Test Cases`
6. `Coverage Summary`
7. `Ambiguities / Missing Information / Assumptions`

Mandatory `Detailed Test Cases` columns:
- `Test Case ID`
- `Title`
- `Requirement Reference`
- `Preconditions`
- `Test Data`
- `Steps`
- `Expected Result`
- `Priority`
- `Risk/Notes`

## Field mapping for execution

- API execution (Postman/Newman):
  - `Preconditions` -> env/token setup
  - `Test Data` -> request params/body/headers
  - `Steps` -> request order
  - `Expected Result` -> status/body assertions
- UI execution (Playwright):
  - `Preconditions` -> account/page state
  - `Steps` -> action sequence
  - `Expected Result` -> locator/assertion list

## Acceptance criteria

1. No case is executed without `Requirement Reference`.
2. `Expected Result` is transformed into explicit assertions.
3. Unclear behavior in Ambiguities is not silently auto-resolved.

## Common failure handling

- Ambiguous expected result:
  - action: keep test as pending and request clarification from member2/member1.
- Missing test data details:
  - action: add placeholder and tag as blocked.
- Duplicate generated cases:
  - action: keep the higher-priority case based on risk and boundary value.

## Current baseline input

- Use `member1/experiments/real-input-pack.md` as the source requirement set for execution conversion.
