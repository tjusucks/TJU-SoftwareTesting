# Risk Analysis Report: Article Lifecycle

## Feature Overview

- **Feature:** Article Creation, Editing, and Deletion
- **Feature ID:** article-lifecycle
- **Project:** RealWorld/Conduit
- **Requirements Analyzed:** 12 (R1–R12)
- **Risk Categories Covered:** Authentication, Authorization, InputValidation, BusinessLogic, DataIntegrity, DataPersistence, APIContract, ErrorHandling, StateTransition

## Requirement Inventory

| ID | Description | Risk Categories | Score | Priority |
|----|-------------|-----------------|-------|----------|
| R1 | Creating an article requires authentication | Authentication, Authorization | 8 | Medium |
| R2 | Authenticated author can create an article with valid data | BusinessLogic, DataPersistence, InputValidation | 8 | Medium |
| R3 | Created article responses include identity, content, metadata, author info | APIContract, DataIntegrity | 6 | Low |
| R4 | Invalid/missing required fields are rejected | InputValidation, ErrorHandling | 12 | Medium |
| R5 | Owning author can edit an existing article | Authorization, BusinessLogic, DataPersistence | 8 | Medium |
| R6 | Article updates preserve unchanged fields | DataIntegrity, BusinessLogic | 9 | Medium |
| R7 | Edit attempts by unauthenticated/non-owners are rejected | Authentication, Authorization, ErrorHandling | 15 | High |
| R8 | Owning author can delete an existing article | Authorization, BusinessLogic, DataPersistence | 8 | Medium |
| R9 | After deletion, article not returned as active | DataIntegrity, StateTransition | 12 | Medium |
| R10 | Delete attempts by unauthenticated/non-owners are rejected | Authentication, Authorization, ErrorHandling | 15 | High |
| R11 | After update, later reads return persisted updated values | DataPersistence, DataIntegrity, StateTransition | 12 | Medium |
| R12 | After deletion, article not observable through detail or list reads | DataIntegrity, StateTransition, APIContract | 12 | Medium |

## Risk Summary

| Priority | Count | Requirements |
|----------|-------|-------------|
| **High** | 2 | R7, R10 |
| **Medium** | 9 | R1, R2, R4, R5, R6, R8, R9, R11, R12 |
| **Low** | 1 | R3 |

### Risk Score Distribution

- **High (15–25):** 2 items
- **Medium (8–14):** 9 items
- **Low (1–7):** 1 item

## High-Risk Requirements

### R7 — Edit rejection for unauthenticated/non-owner users (Score: 15)

**Risk Categories:** Authentication, Authorization, ErrorHandling

Authorization boundary enforcement for edit operations. Owner-check logic is a well-known source of security vulnerabilities. If this control fails, unauthorized users could modify articles they do not own, violating a core business rule.

**Rationale:** Impact is critical (5) because unauthorized modification compromises data integrity and trust. Likelihood is possible (3) because ownership verification logic varies across implementations and edge cases (slug changes, user identity resolution) are error-prone.

**Recommendations:**
- Dedicate focused security test scenarios for edit authorization
- Include negative tests for token manipulation and non-owner access
- Automate authorization checks as regression tests

### R10 — Delete rejection for unauthenticated/non-owner users (Score: 15)

**Risk Categories:** Authentication, Authorization, ErrorHandling

Authorization boundary enforcement for delete operations. Mirrors R7 in severity. Failure could allow permanent data loss through unauthorized deletion.

**Rationale:** Same rationale as R7 — critical impact (5) due to permanent data loss, possible likelihood (3) due to implementation variability in ownership checks.

**Recommendations:**
- Pair with R7 authorization testing for efficiency
- Verify article existence after rejected delete attempts
- Test concurrent delete attempts from multiple sessions

## Medium-Risk Requirements

### R4 — Input validation for required fields (Score: 12)

**Risk Categories:** InputValidation, ErrorHandling

The local fixture does not define exact maximum lengths, whitespace handling, or tag validation. This ambiguity increases the likelihood of boundary-value defects.

**Rationale:** Impact is moderate (3) because invalid inputs are expected to be rejected rather than cause corruption. Likelihood is likely (4) because undefined validation rules lead to implementation-specific behavior that may not match expectations.

### R9 — Post-deletion retrieval behavior (Score: 12)

**Risk Categories:** DataIntegrity, StateTransition

Exact not-found status and response body after deletion are unspecified. Soft-delete vs hard-delete implementations behave differently.

**Rationale:** Impact is major (4) because incorrect post-deletion state violates business rules. Likelihood is possible (3) due to implementation ambiguity.

### R11 — Update persistence observability (Score: 12)

**Risk Categories:** DataPersistence, DataIntegrity, StateTransition

If title changes cause slug regeneration, subsequent reads may fail. Caching could serve stale data.

**Rationale:** Impact is major (4) because this verifies lifecycle durability — a core business rule. Likelihood is possible (3) due to slug-regeneration and caching concerns noted in requirements.

### R12 — Deletion observability across all query paths (Score: 12)

**Risk Categories:** DataIntegrity, StateTransition, APIContract

Deleted articles may persist in cached list results or filtered queries. Multiple query dimensions (global, author, tag, favorited) increase complexity.

**Rationale:** Impact is major (4) as comprehensive deletion observability spans multiple endpoints. Likelihood is possible (3) due to list caching, pagination state, and filter interaction complexity.

### R6 — Partial update field preservation (Score: 9)

**Risk Categories:** DataIntegrity, BusinessLogic

Partial-update semantics (PUT vs PATCH behavior) are unspecified by the fixture. Implementations may overwrite omitted fields with defaults.

**Rationale:** Impact is moderate (3) — incorrect field handling causes data loss but not corruption. Likelihood is possible (3) due to unspecified update semantics.

## Low-Risk Requirements

### R3 — Response format completeness (Score: 6)

**Risk Categories:** APIContract, DataIntegrity

Response field completeness affects client integration but does not affect server-side data integrity. Standard REST conventions make deviations unlikely.

**Rationale:** Impact is moderate (3) because missing fields break client consumers. Likelihood is unlikely (2) because standard response patterns are well-established.

## Risk Scoring Rationale

| Factor | Description |
|--------|-------------|
| **Impact Scale** | Business/security consequences of requirement failure, scored 1–5 |
| **Likelihood Scale** | Probability of defect or failure occurrence, scored 1–5 |
| **Risk Score** | Impact × Likelihood (range: 1–25) |
| **Priority Mapping** | High (15–25), Medium (8–14), Low (1–7) |

Security boundaries (R7, R10) received the highest scores because authorization failures have critical business impact and non-trivial likelihood of implementation defects. Input validation (R4) and data integrity requirements (R9, R11, R12) scored medium-high (12) due to specification ambiguity documented in the structured requirements. Standard CRUD operations (R1, R2, R5, R8) scored medium (8) — important but less likely to fail. Response formatting (R3) scored low (6) as it follows standard conventions.

## Open Questions and Assumptions

1. **Slug regeneration:** R11 notes that title changes may or may not regenerate the article slug. The target implementation's behavior is unknown and affects test design for update-read verification.
2. **Soft-delete vs hard-delete:** R9 and R12 behavior depends on whether the implementation uses soft-delete (logical flag) or hard-delete (row removal). This affects expected post-deletion responses.
3. **Validation rules:** R4 exact maximum lengths, whitespace handling, and tag validation rules are not defined and must be confirmed against the target implementation.
4. **PUT vs PATCH semantics:** R6 partial-update behavior depends on whether the implementation treats PUT as full replacement or applies partial updates.
5. **Token expiry behavior:** Token expiration handling for authenticated operations is not specified.

## Recommendations for Test Planning

1. **Security-boundary testing priority:** R7 and R10 (High priority) should be automated first as regression gates — authorization failures are the highest-impact defects.
2. **Specification-ambiguity testing:** R4, R9, R11, R12 (score 12) require exploratory testing against the target implementation to discover unspecified behavior.
3. **Happy-path coverage:** R2, R5, R8 (standard CRUD) should form the baseline smoke test suite.
4. **Update-read persistence chain:** R11 should be tested as an end-to-end flow (create → update → read) rather than isolated checks.
5. **Deletion observability matrix:** R12 should test across multiple list dimensions (global, author-filtered, tag-filtered, favorited-filtered) to ensure complete coverage.
6. **Pair testing:** R7 and R10 authorization tests can share a common test utility for ownership verification.
7. **Response contract validation:** R3 should validate the full response shape against the API contract.
