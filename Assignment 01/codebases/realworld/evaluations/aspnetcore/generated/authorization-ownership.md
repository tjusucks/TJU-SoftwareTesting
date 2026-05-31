# Black-Box Testing Run Report Template

## 1. Run Metadata

| Field                                    | Value                                                                                                                                                                                                                                 |
| ---------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Project Name                             | RealWorld (Conduit)                                                                                                                                                                                                                   |
| Feature Name                             | Authorization Ownership                                                                                                                                                                                                               |
| Run ID                                   | BB-ASPNETCORE-AUTHZ-OWNERSHIP-001                                                                                                                                                                                                     |
| Date                                     | 2026-04-19                                                                                                                                                                                                                            |
| Author / Operator                        | Claude (blackbox-testing skill)                                                                                                                                                                                                       |
| Skill / Tool Name                        | `blackbox-testing`                                                                                                                                                                                                                    |
| Model / Agent Version                    | glm-5.1                                                                                                                                                                                                                               |
| Prompt Version                           | 1.0                                                                                                                                                                                                                                   |
| Input Type                               | Requirement / API Spec (Mixed)                                                                                                                                                                                                        |
| Input Source Path / Link                 | `Assignment 01/codebases/realworld/specification/features/authorization-ownership.md` + upstream bruno specs                                                                                                                          |
| Target System / Implementation           | ASP.NET Core RealWorld implementation                                                                                                                                                                                                 |
| Target Module / Endpoint / Feature Scope | `PUT /api/articles/{slug}`, `DELETE /api/articles/{slug}`, `DELETE /api/articles/{slug}/comments/{id}`                                                                                                                                |
| Execution Scope                          | Design + Automation                                                                                                                                                                                                                   |
| Notes                                    | Covers cross-user ownership authorization for article and comment modification/deletion, forbidden status code behavior, resource persistence after failed operations, and unauthenticated vs authenticated-but-forbidden distinction |

## 2. Input Summary

### 2.1 Input Overview

- **Project / System Under Test**: RealWorld (Conduit) — a Medium.com-clone backend API
- **Feature Under Test**: Ownership-based authorization rules for article and comment modification and deletion
- **Actors**: User A (resource owner), User B (authenticated non-owner), Unauthenticated user
- **Preconditions**: The RealWorld API server is running and reachable; two registered user accounts exist (User A and User B); User A owns at least one article and optionally a comment on that article
- **Business Rules**:
  - A resource owner can modify or delete their own content
  - A different authenticated user must not be able to update or delete someone else's article
  - A different authenticated user must not be able to delete someone else's comment
  - Forbidden operations return status 403
  - Failed forbidden operations do not mutate or remove the original article
  - Failed forbidden operations do not remove the original comment
  - Unauthenticated requests to protected endpoints return 401 (not 403)
- **Input Constraints**:
  - Article update: `PUT /api/articles/{slug}` with `Authorization: Token {token}` header; at least one article field in the request body
  - Article delete: `DELETE /api/articles/{slug}` with `Authorization: Token {token}` header
  - Comment delete: `DELETE /api/articles/{slug}/comments/{id}` with `Authorization: Token {token}` header
  - Authentication format: `Authorization: Token {jwt}` (RealWorld convention, not standard `Bearer`)
- **Error Conditions**:
  - 403 for authenticated non-owner attempting to delete an article
  - 403 for authenticated non-owner attempting to update an article
  - 403 for authenticated non-owner attempting to delete a comment
  - 401 for unauthenticated requests to protected endpoints
  - 404 for non-existent article slug or comment id

### 2.2 Requirement Items

| Requirement ID | Requirement Description                                                                      | Priority | Notes                                      |
| -------------- | -------------------------------------------------------------------------------------------- | -------- | ------------------------------------------ |
| R1             | User A (owner) can create an article                                                         | High     | Setup prerequisite                         |
| R2             | User B (authenticated non-owner) cannot delete User A's article; returns 403                 | High     | Core authorization rule                    |
| R3             | User B (authenticated non-owner) cannot update User A's article; returns 403                 | High     | Core authorization rule                    |
| R4             | Forbidden operations on articles return status code 403 with `errors.article: ["forbidden"]` | High     | Status code and error format               |
| R5             | After a failed forbidden delete on an article, the original article remains intact           | High     | Non-destructive guarantee                  |
| R6             | After a failed forbidden update on an article, the original article remains unchanged        | High     | Non-destructive guarantee                  |
| R7             | User A (owner) can create a comment on the article                                           | High     | Setup prerequisite                         |
| R8             | User B (authenticated non-owner) cannot delete User A's comment; returns 403                 | High     | Core authorization rule                    |
| R9             | Forbidden comment deletion returns status code 403 with `errors.comment: ["forbidden"]`      | High     | Status code and error format               |
| R10            | After the failed comment delete attempt, the original comment still exists                   | High     | Non-destructive guarantee                  |
| R11            | Unauthenticated requests to protected article/comment endpoints return 401, not 403          | High     | Distinguishes missing auth from wrong auth |
| R12            | An article owner can successfully delete their own article (returns 204)                     | High     | Positive control for delete                |
| R13            | An article owner can successfully update their own article (returns 200)                     | High     | Positive control for update                |
| R14            | A comment author can successfully delete their own comment (returns 204)                     | High     | Positive control for comment delete        |

### 2.3 Assumptions About Input

- Assumption 1: The API uses JSON request/response bodies with top-level `article` or `comment` wrapper keys, consistent with the RealWorld API specification and bruno test files.
- Assumption 2: Authentication uses the `Authorization: Token {token}` header format, where `{token}` is obtained from user registration or login.
- Assumption 3: Error responses follow the format `{"errors": {"field": ["message"]}}`, as shown in the upstream bruno specs. Forbidden errors use `errors.article: ["forbidden"]` for articles and `errors.comment: ["forbidden"]` for comments.
- Assumption 4: The target server is accessible at a configurable base URL (default `http://localhost:5000`).
- Assumption 5: Delete operations return 204 (no content body) on success, as shown in the bruno spec.
- Assumption 6: Update operations return 200 with the updated article payload on success.
- Assumption 7: Unauthenticated requests to protected endpoints return 401 with `errors.token: ["is missing"]`, distinguishing them from 403 forbidden responses for authenticated-but-unauthorized users.

## 3. Test Design Strategy

### 3.1 Applied Black-Box Techniques

| Technique                   | Applied? | Where Used                                                                                                                                                      | Notes                                              |
| --------------------------- | -------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------- |
| Equivalence Partitioning    | Yes      | Auth state partitions: unauthenticated, authenticated owner, authenticated non-owner                                                                            | Three mutually exclusive actor classes             |
| Boundary Value Analysis     | Yes      | Just-below-authorized: authenticated non-owner attempting to act as owner                                                                                       | The boundary between "can" and "cannot"            |
| Decision Table Testing      | Yes      | Article operations x actor roles (owner/non-owner/unauthenticated)                                                                                              | 3x3 decision matrix for update/delete              |
| State Transition Testing    | Yes      | Resource state after forbidden operation: unchanged vs deleted/modified                                                                                         | Pre-state → forbidden op → post-state verification |
| Error Guessing              | Yes      | Unauthenticated vs authenticated-but-forbidden distinction; cross-user interference                                                                             | Common authorization failure patterns              |
| Scenario / Use-Case Testing | Yes      | Full sequential scenario: register A, register B, A creates article, B fails to delete/update, A creates comment, B fails to delete comment, verify persistence | Mirrors the bruno spec flow                        |

### 3.2 Test Dimension Summary

- Valid input classes: Owner deletes own article; owner updates own article; owner deletes own comment
- Invalid input classes: Non-owner deletes article; non-owner updates article; non-owner deletes comment
- Boundary values: Authenticated non-owner is the boundary between "can" (owner) and "cannot" (unauthenticated + non-owner); the key distinction is 403 vs 401
- Permission / role cases: Three-way auth partition: unauthenticated (401), authenticated owner (200/204), authenticated non-owner (403)
- State / sequencing cases: Resource persistence after failed forbidden operation (article body unchanged, comment still exists); multiple sequential forbidden attempts
- Combination cases: Non-owner update attempt followed by verification of article integrity; non-owner delete attempt followed by verification of comment existence

### 3.3 Edge-Case Design Notes

- Authenticated-but-unauthorized boundary: The core edge case is that a user who IS authenticated but is NOT the owner must receive 403, not 401 or 200. This is the primary boundary under test.
- Unauthenticated vs forbidden distinction: An unauthenticated user hitting a protected endpoint gets 401; an authenticated non-owner gets 403. These are distinct failure modes and must not be conflated.
- Resource persistence after forbidden operation: The spec explicitly requires that failed operations do not mutate or remove the resource. This requires a two-step verification: (1) attempt forbidden operation → get 403, (2) retrieve resource → verify unchanged.
- Cross-user setup integrity: User B's forbidden operations must not affect User A's resources. This requires verifying the article body, title, and description after a failed update, and verifying the comment body after a failed delete.
- Sequential forbidden attempts: A user who is forbidden from deleting should also be forbidden from updating the same article, and vice versa. Both must be independently verified.
- Multiple non-owners: If User C also attempts forbidden operations, the same 403 should result. While this is an extension, the core spec only defines User A (owner) and User B (non-owner).

## 4. Equivalence Partitioning Analysis

| EP ID | Requirement ID | Input / Rule                                  | Partition Type | Description                             | Expected Outcome                             | Covered by Test Case ID |
| ----- | -------------- | --------------------------------------------- | -------------- | --------------------------------------- | -------------------------------------------- | ----------------------- |
| EP1   | R1             | Actor identity on article create              | Valid          | User A creates article                  | 201 with article payload                     | TC01                    |
| EP2   | R2             | Actor identity on article delete              | Invalid        | Authenticated non-owner deletes article | 403 with article error                       | TC02                    |
| EP3   | R3             | Actor identity on article update              | Invalid        | Authenticated non-owner updates article | 403 with article error                       | TC03                    |
| EP4   | R4             | Error response for article forbidden          | Invalid        | 403 response body format                | `errors.article: ["forbidden"]`              | TC02, TC03              |
| EP5   | R5             | Article state after forbidden delete          | Valid          | Article persists unchanged              | Article still retrievable with original data | TC04                    |
| EP6   | R6             | Article state after forbidden update          | Valid          | Article persists unchanged              | Article body/description/title unchanged     | TC05                    |
| EP7   | R7             | Actor identity on comment create              | Valid          | User A creates comment on own article   | 201 with comment payload                     | TC06                    |
| EP8   | R8             | Actor identity on comment delete              | Invalid        | Authenticated non-owner deletes comment | 403 with comment error                       | TC07                    |
| EP9   | R9             | Error response for comment forbidden          | Invalid        | 403 response body format                | `errors.comment: ["forbidden"]`              | TC07                    |
| EP10  | R10            | Comment state after forbidden delete          | Valid          | Comment persists                        | Comment still visible in listing             | TC08                    |
| EP11  | R11            | Unauthenticated access to protected endpoints | Invalid        | No auth token on update/delete          | 401 with token error                         | TC09, TC10, TC11        |
| EP12  | R12            | Owner deletes own article                     | Valid          | User A deletes own article              | 204                                          | TC12                    |
| EP13  | R13            | Owner updates own article                     | Valid          | User A updates own article              | 200 with updated payload                     | TC13                    |
| EP14  | R14            | Owner deletes own comment                     | Valid          | User A deletes own comment              | 204                                          | TC14                    |

### 4.1 EP Coverage Notes

- Covered partitions: All valid and invalid partitions for the in-scope requirements (R1–R14).
- Missing partitions: Multiple concurrent non-owners (User C, User D) — the spec only defines two actors (A and B); admin/superuser role — the spec does not define elevated roles; partial ownership (e.g., co-authors) — the spec only defines single ownership.
- Partially covered partitions: EP4 covers the error format for both article delete and update forbidden cases; the error key differs ("article" vs "comment") but the pattern is the same.

## 5. Boundary Value Analysis

| BVA ID | Requirement ID | Boundary Item                                      | Boundary Definition                                  | Test Values                                              | Expected Outcome                     | Covered by Test Case ID              |
| ------ | -------------- | -------------------------------------------------- | ---------------------------------------------------- | -------------------------------------------------------- | ------------------------------------ | ------------------------------------ |
| B1     | R2, R3         | Auth state boundary on article ops                 | Authenticated non-owner (just below owner privilege) | User B's token on User A's article                       | 403 forbidden                        | TC02, TC03                           |
| B2     | R11            | Auth state boundary (unauthenticated vs forbidden) | Unauthenticated vs authenticated-but-unauthorized    | No token vs User B's token                               | No token → 401; User B's token → 403 | TC09, TC10, TC11 vs TC02, TC03, TC07 |
| B3     | R5, R6         | Resource state boundary after forbidden op         | Article unchanged after failed modification          | Original title/body/description vs post-forbidden values | Values must be identical             | TC04, TC05                           |
| B4     | R10            | Comment existence boundary after forbidden delete  | Comment still present after failed delete            | Comment list before vs after forbidden delete            | Comment count and body unchanged     | TC08                                 |

### 5.1 BVA Coverage Notes

- Explicit boundaries tested: Auth-state boundary (owner/non-owner/unauthenticated); resource-state boundary (unchanged after forbidden operation).
- Missing boundaries: Token expiry during a forbidden operation (does an expired token produce 401 instead of 403?); malformed token (should produce 401, not 403); token belonging to a deleted user.
- Ambiguous boundary: Whether a non-owner who is also the article author's follower gets any different treatment (spec says no, but this is not explicitly tested).

## 6. Test Scenarios

| Scenario ID | Requirement Reference | Scenario Title                                           | Scenario Type | Description                                                                           | Priority |
| ----------- | --------------------- | -------------------------------------------------------- | ------------- | ------------------------------------------------------------------------------------- | -------- |
| S1          | R1                    | Owner creates article                                    | Happy Path    | User A creates an article as a setup step and positive control                        | High     |
| S2          | R2                    | Non-owner delete article forbidden                       | Negative      | User B attempts to delete User A's article and receives 403                           | High     |
| S3          | R3                    | Non-owner update article forbidden                       | Negative      | User B attempts to update User A's article and receives 403                           | High     |
| S4          | R4                    | Forbidden article operation returns correct error format | Negative      | Verify the 403 response body contains `errors.article: ["forbidden"]`                 | High     |
| S5          | R5                    | Article persists after forbidden delete                  | Edge          | After User B's failed delete, verify the article still exists with original data      | High     |
| S6          | R6                    | Article persists after forbidden update                  | Edge          | After User B's failed update, verify the article body/title/description are unchanged | High     |
| S7          | R7                    | Owner creates comment                                    | Happy Path    | User A creates a comment on their own article                                         | High     |
| S8          | R8                    | Non-owner delete comment forbidden                       | Negative      | User B attempts to delete User A's comment and receives 403                           | High     |
| S9          | R9                    | Forbidden comment operation returns correct error format | Negative      | Verify the 403 response body contains `errors.comment: ["forbidden"]`                 | High     |
| S10         | R10                   | Comment persists after forbidden delete                  | Edge          | After User B's failed delete, verify the comment still exists in the listing          | High     |
| S11         | R11                   | Unauthenticated access returns 401                       | Negative      | Unauthenticated requests to protected endpoints return 401, not 403                   | High     |
| S12         | R12                   | Owner deletes own article successfully                   | Happy Path    | User A deletes their own article and receives 204                                     | High     |
| S13         | R13                   | Owner updates own article successfully                   | Happy Path    | User A updates their own article and receives 200                                     | High     |
| S14         | R14                   | Owner deletes own comment successfully                   | Happy Path    | User A deletes their own comment and receives 204                                     | High     |
| S15         | R2, R5                | Sequential forbidden operations on article               | Scenario      | User B tries both delete and update on User A's article; article survives both        | Medium   |
| S16         | R8, R10               | Forbidden comment delete with multiple comments          | Scenario      | User B tries to delete one of User A's comments; other comments unaffected            | Medium   |

## 7. Edge Case Matrix

| Requirement ID | Edge Category | Concrete Case                                        | Covered by Test Case ID | Notes                                                                  |
| -------------- | ------------- | ---------------------------------------------------- | ----------------------- | ---------------------------------------------------------------------- |
| R2             | Unauthorized  | Authenticated non-owner deletes article              | TC02                    | Core acceptance criterion                                              |
| R3             | Unauthorized  | Authenticated non-owner updates article              | TC03                    | Core acceptance criterion                                              |
| R4             | Error format  | 403 response body for article forbidden              | TC02, TC03              | `errors.article: ["forbidden"]`                                        |
| R5             | State         | Article intact after forbidden delete                | TC04                    | Non-destructive guarantee                                              |
| R6             | State         | Article intact after forbidden update                | TC05                    | Non-destructive guarantee                                              |
| R8             | Unauthorized  | Authenticated non-owner deletes comment              | TC07                    | Core acceptance criterion                                              |
| R9             | Error format  | 403 response body for comment forbidden              | TC07                    | `errors.comment: ["forbidden"]`                                        |
| R10            | State         | Comment survives after forbidden delete              | TC08                    | Non-destructive guarantee                                              |
| R11            | Missing       | No auth token on article delete                      | TC09                    | 401 vs 403 distinction                                                 |
| R11            | Missing       | No auth token on article update                      | TC10                    | 401 vs 403 distinction                                                 |
| R11            | Missing       | No auth token on comment delete                      | TC11                    | 401 vs 403 distinction                                                 |
| R2, R3         | Sequencing    | Sequential forbidden delete + update on same article | TC15                    | Both operations fail, article survives both                            |
| R8, R10        | Combination   | Forbidden delete on article with multiple comments   | TC16                    | Non-target comments unaffected                                         |
| R11            | Expired/stale | Expired or invalid token on protected endpoint       | Deferred                | Spec does not define token expiry behavior explicitly for this feature |
| R11            | Wrong type    | Malformed token string                               | Deferred                | Lower priority; 401 expected but not explicitly in spec                |

## 8. Detailed Test Cases

| Test Case ID | Title                                                                 | Requirement Reference | Preconditions                                                   | Test Data                                                    | Steps                                                                                                                                         | Expected Result                                                                                                            | Priority | Risk / Notes                            |
| ------------ | --------------------------------------------------------------------- | --------------------- | --------------------------------------------------------------- | ------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------- | -------- | --------------------------------------- |
| TC01         | Owner creates article successfully                                    | R1                    | API server running; User A registered                           | title=`Authz Article {uid}`, description=`test`, body=`test` | 1. Register User A 2. POST `/api/articles` with User A's token and article payload                                                            | Status 201; response contains `article.slug`; article author is User A                                                     | High     | Setup prerequisite and positive control |
| TC02         | Non-owner delete article returns 403                                  | R2, R4                | API server running; User A's article exists                     | slug of User A's article                                     | 1. Register User A, create article 2. Register User B 3. DELETE `/api/articles/{slug}` with User B's token                                    | Status 403; `errors.article[0]` equals `"forbidden"`                                                                       | High     | Core acceptance criterion               |
| TC03         | Non-owner update article returns 403                                  | R3, R4                | API server running; User A's article exists                     | slug of User A's article, body=`hijacked`                    | 1. Register User A, create article 2. Register User B 3. PUT `/api/articles/{slug}` with User B's token and `{"article":{"body":"hijacked"}}` | Status 403; `errors.article[0]` equals `"forbidden"`                                                                       | High     | Core acceptance criterion               |
| TC04         | Article persists after forbidden delete                               | R5                    | User B's delete attempt returned 403                            | (same article from TC02)                                     | 1. User B attempts DELETE → 403 2. GET `/api/articles/{slug}`                                                                                 | Status 200; article still exists; `article.title`, `article.body`, `article.description` match original values             | High     | Non-destructive guarantee               |
| TC05         | Article persists after forbidden update                               | R6                    | User B's update attempt returned 403                            | (same article from TC03)                                     | 1. User B attempts PUT with body=`hijacked` → 403 2. GET `/api/articles/{slug}`                                                               | Status 200; `article.body` is NOT `"hijacked"`; matches original body; `article.title` and `article.description` unchanged | High     | Non-destructive guarantee               |
| TC06         | Owner creates comment on article                                      | R7                    | API server running; User A's article exists                     | body=`A's comment`                                           | 1. POST `/api/articles/{slug}/comments` with User A's token and `{"comment":{"body":"A's comment"}}`                                          | Status 201; `comment.body` equals `"A's comment"`; `comment.author.username` is User A                                     | High     | Setup prerequisite                      |
| TC07         | Non-owner delete comment returns 403                                  | R8, R9                | API server running; User A's comment exists on User A's article | comment id of User A's comment                               | 1. Register User A, create article, create comment 2. Register User B 3. DELETE `/api/articles/{slug}/comments/{id}` with User B's token      | Status 403; `errors.comment[0]` equals `"forbidden"`                                                                       | High     | Core acceptance criterion               |
| TC08         | Comment persists after forbidden delete                               | R10                   | User B's comment delete attempt returned 403                    | (same comment from TC07)                                     | 1. User B attempts DELETE → 403 2. GET `/api/articles/{slug}/comments`                                                                        | Status 200; `comments` array contains at least 1 comment; first comment body equals `"A's comment"`                        | High     | Non-destructive guarantee               |
| TC09         | Unauthenticated article delete returns 401                            | R11                   | API server running; an article exists                           | (slug of existing article)                                   | 1. DELETE `/api/articles/{slug}` with no auth header                                                                                          | Status 401; `errors.token[0]` equals `"is missing"`                                                                        | High     | 401 vs 403 distinction                  |
| TC10         | Unauthenticated article update returns 401                            | R11                   | API server running; an article exists                           | (slug of existing article)                                   | 1. PUT `/api/articles/{slug}` with no auth header                                                                                             | Status 401; `errors.token[0]` equals `"is missing"`                                                                        | High     | 401 vs 403 distinction                  |
| TC11         | Unauthenticated comment delete returns 401                            | R11                   | API server running; a comment exists                            | (slug and comment id)                                        | 1. DELETE `/api/articles/{slug}/comments/{id}` with no auth header                                                                            | Status 401; `errors.token[0]` equals `"is missing"`                                                                        | High     | 401 vs 403 distinction                  |
| TC12         | Owner deletes own article returns 204                                 | R12                   | API server running; User A's article exists                     | (slug of User A's article)                                   | 1. DELETE `/api/articles/{slug}` with User A's token                                                                                          | Status 204; response has no body                                                                                           | High     | Positive control for article delete     |
| TC13         | Owner updates own article returns 200                                 | R13                   | API server running; User A's article exists                     | body=`updated by owner`                                      | 1. PUT `/api/articles/{slug}` with User A's token and `{"article":{"body":"updated by owner"}}`                                               | Status 200; `article.body` equals `"updated by owner"`                                                                     | High     | Positive control for article update     |
| TC14         | Owner deletes own comment returns 204                                 | R14                   | API server running; User A's comment exists                     | (slug and comment id)                                        | 1. DELETE `/api/articles/{slug}/comments/{id}` with User A's token                                                                            | Status 204; response has no body                                                                                           | High     | Positive control for comment delete     |
| TC15         | Sequential forbidden operations — article survives both               | R2, R3, R5, R6        | User A's article exists; User B registered                      | (same article)                                               | 1. User B DELETE → 403 2. User B PUT → 403 3. GET `/api/articles/{slug}`                                                                      | Article still exists with original title, body, description after both forbidden attempts                                  | Medium   | Compound persistence check              |
| TC16         | Forbidden comment delete with multiple comments — non-target survives | R8, R10               | User A's article with multiple comments; User B registered      | (comment id of one comment)                                  | 1. User A creates two comments 2. User B DELETE one comment → 403 3. GET `/api/articles/{slug}/comments`                                      | Both original comments still present; total count unchanged; comment bodies match originals                                | Medium   | Multi-comment persistence               |

## 9. How to Run the Generated Test Codes

### 9.1 Prerequisites

- Target system: ASP.NET Core RealWorld implementation running and reachable
- .NET 8 SDK (or later) installed
- xUnit test runner available via `dotnet test`
- The RealWorld API server must be started before running tests (see Environment Setup)

### 9.2 Test Code Location

- **Generated Test Code Path**: `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/`
- **Project Root Path**: `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/`
- **Test Entry File / Directory**: `AuthorizationOwnershipTests.cs`
- **Related Configuration Files**: `RealWorld.Blackbox.Tests.csproj`

### 9.3 Environment Setup

1. **Working Directory**: `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/`
2. **Runtime Version**: .NET 8 SDK or later
3. **Dependency Install Command**: `dotnet restore`
4. **Build Command**: `dotnet build`
5. **Test Environment Variables**:
   - `REALWORLD_BASE_URL`: Base URL of the running RealWorld API (default: `http://localhost:5000`)
6. **Service Start Command**: Start the ASP.NET Core RealWorld server on the configured host/port before running tests
7. **Test Data / Seed Command**: No seed data required; tests register their own users and create their own articles/comments with unique IDs

### 9.4 Run Commands

```bash
# Restore dependencies
dotnet restore

# Run all tests
dotnet test

# Run only AuthorizationOwnership tests
dotnet test --filter "FullyQualifiedName~AuthorizationOwnership"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run a specific test by name
dotnet test --filter "FullyQualifiedName~NonOwnerDeleteArticle_Returns403"

# Run with no caching (force re-execution)
dotnet test --no-build && dotnet test

# Run with custom base URL
REALWORLD_BASE_URL=http://localhost:3000 dotnet test
```

### 9.5 Execution Notes

- Start the target service before running black-box tests. Tests will fail with connection errors if the server is unreachable.
- Each test generates unique user IDs (GUID-based) to avoid collisions with existing data. Tests are idempotent and can be re-run without manual cleanup.
- TC02–TC05 test the article authorization flow with a single User A / User B pair. TC07–TC08 test the comment authorization flow with the same pattern.
- TC09–TC11 verify the 401 vs 403 distinction: unauthenticated users should get 401, while authenticated non-owners should get 403.
- TC15 and TC16 are compound tests that verify persistence after sequential or multi-resource forbidden operations.
- Record the exact `REALWORLD_BASE_URL` value used for reproducibility.

## 10. Coverage Summary

### 10.1 Requirement Coverage Table

| Requirement ID | EP Covered? | BVA Covered? | Edge Case Covered? | Negative Case Covered? | State / Sequence Covered? | Covered by Test Cases | Coverage Status | Notes                                    |
| -------------- | ----------- | ------------ | ------------------ | ---------------------- | ------------------------- | --------------------- | --------------- | ---------------------------------------- |
| R1             | Yes         | N/A          | Yes                | N/A                    | N/A                       | TC01                  | Full            | Setup + positive control                 |
| R2             | Yes         | Yes          | Yes                | Yes                    | N/A                       | TC02, TC15            | Full            | Non-owner article delete                 |
| R3             | Yes         | Yes          | Yes                | Yes                    | N/A                       | TC03, TC15            | Full            | Non-owner article update                 |
| R4             | Yes         | N/A          | Yes                | N/A                    | N/A                       | TC02, TC03            | Full            | Error format for article forbidden       |
| R5             | Yes         | Yes          | Yes                | N/A                    | Yes                       | TC04, TC15            | Full            | Article persists after forbidden delete  |
| R6             | Yes         | Yes          | Yes                | N/A                    | Yes                       | TC05, TC15            | Full            | Article persists after forbidden update  |
| R7             | Yes         | N/A          | Yes                | N/A                    | N/A                       | TC06                  | Full            | Owner creates comment                    |
| R8             | Yes         | Yes          | Yes                | Yes                    | N/A                       | TC07, TC16            | Full            | Non-owner comment delete                 |
| R9             | Yes         | N/A          | Yes                | N/A                    | N/A                       | TC07                  | Full            | Error format for comment forbidden       |
| R10            | Yes         | Yes          | Yes                | N/A                    | Yes                       | TC08, TC16            | Full            | Comment persists after forbidden delete  |
| R11            | Yes         | Yes          | Yes                | Yes                    | N/A                       | TC09, TC10, TC11      | Full            | Unauthenticated vs forbidden distinction |
| R12            | Yes         | N/A          | Yes                | N/A                    | N/A                       | TC12                  | Full            | Owner delete article                     |
| R13            | Yes         | N/A          | Yes                | N/A                    | N/A                       | TC13                  | Full            | Owner update article                     |
| R14            | Yes         | N/A          | Yes                | N/A                    | N/A                       | TC14                  | Full            | Owner delete comment                     |

### 10.2 EP / BVA to Test Case Mapping

| Analysis Item ID | Type | Requirement ID | Description                                    | Mapped Test Case ID(s)        | Covered? | Notes |
| ---------------- | ---- | -------------- | ---------------------------------------------- | ----------------------------- | -------- | ----- |
| EP1              | EP   | R1             | Valid: owner creates article                   | TC01                          | Yes      |       |
| EP2              | EP   | R2             | Invalid: non-owner deletes article             | TC02                          | Yes      |       |
| EP3              | EP   | R3             | Invalid: non-owner updates article             | TC03                          | Yes      |       |
| EP4              | EP   | R4             | Invalid: 403 error format for article          | TC02, TC03                    | Yes      |       |
| EP5              | EP   | R5             | Valid: article persists after forbidden delete | TC04                          | Yes      |       |
| EP6              | EP   | R6             | Valid: article persists after forbidden update | TC05                          | Yes      |       |
| EP7              | EP   | R7             | Valid: owner creates comment                   | TC06                          | Yes      |       |
| EP8              | EP   | R8             | Invalid: non-owner deletes comment             | TC07                          | Yes      |       |
| EP9              | EP   | R9             | Invalid: 403 error format for comment          | TC07                          | Yes      |       |
| EP10             | EP   | R10            | Valid: comment persists after forbidden delete | TC08                          | Yes      |       |
| EP11             | EP   | R11            | Invalid: unauthenticated access                | TC09, TC10, TC11              | Yes      |       |
| EP12             | EP   | R12            | Valid: owner deletes own article               | TC12                          | Yes      |       |
| EP13             | EP   | R13            | Valid: owner updates own article               | TC13                          | Yes      |       |
| EP14             | EP   | R14            | Valid: owner deletes own comment               | TC14                          | Yes      |       |
| B1               | BVA  | R2, R3         | Auth-state boundary (non-owner)                | TC02, TC03                    | Yes      |       |
| B2               | BVA  | R11            | Unauthenticated vs forbidden                   | TC09–TC11 vs TC02, TC03, TC07 | Yes      |       |
| B3               | BVA  | R5, R6         | Resource unchanged after forbidden op          | TC04, TC05                    | Yes      |       |
| B4               | BVA  | R10            | Comment exists after forbidden delete          | TC08                          | Yes      |       |

### 10.3 Coverage Metrics

| Metric                 | Formula                                              | Value        |
| ---------------------- | ---------------------------------------------------- | ------------ |
| Requirement Coverage   | covered_requirements / total_requirements            | 14/14 = 100% |
| EP Coverage            | covered_partitions / total_partitions                | 14/14 = 100% |
| BVA Coverage           | covered_boundaries / total_boundaries                | 4/4 = 100%   |
| Edge Case Coverage     | covered_edge_categories / applicable_edge_categories | 14/16 = 88%  |
| Negative Case Coverage | negative_cases_present / applicable_requirements     | 6/6 = 100%   |
| Duplicate Case Rate    | duplicate_cases / total_cases                        | 0/16 = 0%    |
| Executability Score    | 1-5                                                  | 4            |

### 10.4 Coverage Notes

- Strongest covered area: R2/R3/R5/R6 (article ownership) — forbidden operations are tested for both delete and update, with persistence verification after each. TC15 adds a compound check.
- Strongest covered area: R8/R10 (comment ownership) — forbidden comment delete is tested with persistence verification. TC16 adds multi-comment persistence.
- Weakest covered area: Expired/malformed tokens on protected endpoints — deferred because the spec does not explicitly define this behavior for this feature slice.
- Under-covered areas: Concurrent forbidden operations from multiple non-owners; token expiry during a request; cross-article interference (User B's forbidden operation on Article 1 should not affect Article 2).

## 11. Ambiguities / Missing Information / Assumptions

### 11.1 Ambiguous Requirements

- Item 1: The spec says "forbidden operations return status 403" but does not specify the error response body format. The bruno specs clarify `errors.article: ["forbidden"]` and `errors.comment: ["forbidden"]`, which the tests follow.
- Item 2: The spec says "failed forbidden operations do not mutate or remove the original article" but does not specify whether partial mutation is possible (e.g., could a non-owner update succeed partially on some fields?). The tests assume the entire operation is atomic — either all fields are updated or none.
- Item 3: The spec distinguishes "unauthenticated failure" from "authenticated-but-forbidden failure" as a test dimension but does not explicitly state the expected status codes. The bruno specs and RealWorld API convention confirm 401 for unauthenticated and 403 for forbidden.

### 11.2 Missing Information

- Missing validation rule: Behavior when a comment is deleted by its author while another user (the article owner) might also have implicit delete rights. The spec only says the comment author can delete.
- Missing behavior: Whether the article owner can delete any comment on their article, or only the comment author. The spec only defines comment author ownership.
- Missing behavior: What happens when the article has been deleted by the owner while a non-owner is attempting operations on it (race condition).
- Missing behavior: Token expiry behavior — whether an expired token produces 401 or 403.
- Missing behavior: Whether a non-owner can favorite/unfavorite another user's article (this is not in scope for this feature but could be confused with ownership rules).

### 11.3 Assumptions

- Assumption 1: The API uses JSON request/response bodies with `{"article": {...}}` and `{"comment": {...}}` wrapper keys, consistent with the RealWorld API specification.
- Assumption 2: Authentication uses the `Authorization: Token {token}` header format, where `{token}` is obtained from user registration or login.
- Assumption 3: Forbidden operations return 403 with `errors.article: ["forbidden"]` or `errors.comment: ["forbidden"]`, as shown in the upstream bruno specs.
- Assumption 4: Unauthenticated requests return 401 with `errors.token: ["is missing"]`, distinguishing them from 403 responses.
- Assumption 5: Only the comment author (not the article owner) can delete a comment. The article owner has no special comment-delete privilege beyond their own comments.
- Assumption 6: The `REALWORLD_BASE_URL` environment variable (defaulting to `http://localhost:5000`) is sufficient to configure the test target.
- Assumption 7: Delete returns 204 with no response body on success; Update returns 200 with the full article payload on success.
