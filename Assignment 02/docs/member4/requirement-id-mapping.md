# Requirement ID Mapping for Integrated Finalization

This document maps member4's historical standalone requirements (`inputs/reference`, R1-R16)
to member2's formal integrated requirements (`outputs/structured-requirements`, R1-R12).

## Mapping Table

| Standalone (old)                    | Integrated (new)             | Mapping type | Notes                                                                      |
| ----------------------------------- | ---------------------------- | ------------ | -------------------------------------------------------------------------- |
| R1 create requires auth             | R1 create requires auth      | 1:1          | Semantics unchanged.                                                       |
| R2 valid create success             | R2 valid create success      | 1:1          | Semantics unchanged.                                                       |
| R3 create response fields           | R3 create response contract  | 1:1          | Integrated text is broader but equivalent.                                 |
| R4 duplicate title unique slug      | R2 / R3                      | split        | Treated as create success + response identity behavior in integrated mode. |
| R5 empty title rejected             | R4 invalid create rejected   | many:1       | Field-level validation merged into R4.                                     |
| R6 empty description rejected       | R4 invalid create rejected   | many:1       | Field-level validation merged into R4.                                     |
| R7 empty body rejected              | R4 invalid create rejected   | many:1       | Field-level validation merged into R4.                                     |
| R8 unauthenticated create rejected  | R1 / R4                      | split        | Auth gate + invalid request branch.                                        |
| R9 owner update success             | R5 owner can edit            | 1:1          | Update success moved under ownership rule.                                 |
| R10 partial update preserves fields | R6 preserve unchanged fields | 1:1          | Semantics unchanged.                                                       |
| R11 omit tagList preserves tags     | R6 preserve unchanged fields | many:1       | tagList omitted behavior becomes a partial-update subcase.                 |
| R12 empty tagList clears tags       | R5 / R6                      | split        | Update behavior remains in edit + preservation semantics.                  |
| R13 null tagList rejected           | R5 / R6                      | split        | Validation branch represented inside update rules.                         |
| R14 update persistence visible      | R11 update persistence       | 1:1          | Persistence rule retained explicitly.                                      |
| R15 owner delete success            | R8 owner can delete          | 1:1          | Deletion success ownership rule.                                           |
| R16 post-delete retrieval fails     | R9 / R12                     | split        | R9 covers detail read, R12 extends to list observability.                  |

## Impact on Member4 Outputs

- Final integrated traceability must only use `R1`-`R12`.
- Decision-table coverage for tagList stays in black-box analysis, but is linked to integrated update requirements (`R5`/`R6`) instead of standalone `R11`-`R13`.
- Post-delete list observability (`R12`) is now a mandatory explicit coverage target.

## Scope Boundary

- Member4 adapts downstream black-box assets to the integrated IDs.
- Member4 does not modify member2/member3 output files or requirement wording.
