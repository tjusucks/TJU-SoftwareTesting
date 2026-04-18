# Member1 Task 2: Frozen I/O Contract

Version: `io-contract-v1`
Status: frozen for team integration

## 1) Input contract (from requirement/spec to prompt)

Input format name: `RequirementInputV1`

Required fields:
- `project_name`: short project identifier
- `feature_name`: feature under test
- `requirement_items`: list of requirement entries

Each item in `requirement_items` must contain:
- `id`: requirement ID (for example `R1`)
- `text`: requirement statement

Recommended optional fields:
- `actors`
- `preconditions`
- `business_rules`
- `input_constraints`
- `error_conditions`
- `priority`

Parsing rule:
- if requirement text is plain markdown, member2 script should normalize it into this structure before calling prompt.

## 2) Output contract (from model to downstream scripts)

Output format name: `GeneratedBlackboxOutputV1`

Top-level ordered sections (must keep exact names):
1. `Feature Summary`
2. `Requirements Extracted`
3. `Test Design Strategy`
4. `Test Scenarios`
5. `Detailed Test Cases`
6. `Coverage Summary`
7. `Ambiguities / Missing Information / Assumptions`

`Detailed Test Cases` mandatory columns:
- `Test Case ID`
- `Title`
- `Requirement Reference`
- `Preconditions`
- `Test Data`
- `Steps`
- `Expected Result`
- `Priority`
- `Risk/Notes`

## 3) Stability rules

1. Field names and section names are frozen after 2026-04-17.
2. Additive changes are allowed; destructive renaming is not allowed.
3. Every test case must reference at least one requirement ID.
4. If requirement ID is missing from source, use temporary ID `RX` and list it in Ambiguities.
