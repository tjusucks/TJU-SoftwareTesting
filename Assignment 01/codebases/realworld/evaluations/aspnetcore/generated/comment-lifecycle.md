# Black-Box Testing Run Report

## 1. Run Metadata

| Field                                    | Value                                                                                                                  |
| ---------------------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| Project Name                             | RealWorld (Conduit)                                                                                                    |
| Feature Name                             | Comment Lifecycle                                                                                                      |
| Run ID                                   | BB-ASPNETCORE-COMMENT-LIFECYCLE-001                                                                                    |
| Date                                     | 2026-04-19                                                                                                             |
| Author / Operator                        | Claude (blackbox-testing skill)                                                                                        |
| Skill / Tool Name                        | `blackbox-testing`                                                                                                     |
| Model / Agent Version                    | glm-5.1                                                                                                                |
| Prompt Version                           | 1.0                                                                                                                    |
| Input Type                               | Requirement / API Spec (Mixed)                                                                                         |
| Input Source Path / Link                 | `Assignment 01/codebases/realworld/specification/features/comment-lifecycle.md` + upstream bruno specs                 |
| Target System / Implementation           | ASP.NET Core RealWorld implementation                                                                                  |
| Target Module / Endpoint / Feature Scope | `POST /api/articles/{slug}/comments`, `GET /api/articles/{slug}/comments`, `DELETE /api/articles/{slug}/comments/{id}` |
| Execution Scope                          | Design + Automation                                                                                                    |
| Notes                                    | Covers nested-resource CRUD, state transitions, persistence, and selective side effects                                |

## 2. Input Summary

### 2.1 Input Overview

- **Project / System Under Test**: RealWorld (Conduit) — a Medium.com-clone backend API
- **Feature Under Test**: Comment creation, listing, deletion, and selective deletion behavior for article comments
- **Actors**: Authenticated user (creates/deletes comments), Unauthenticated observer (lists comments), Different authenticated user (authorization boundary)
- **Preconditions**: An article must exist for comments to be created; a user must be registered and authenticated to create or delete comments
- **Business Rules**:
  - Posting a comment requires authentication
  - Valid comment creation succeeds with status 201
  - The returned comment includes an integer id, body, timestamps (createdAt, updatedAt), and author username
  - Comments for an article can be listed successfully
  - Comment listing works without authentication
  - Deleting a comment requires authentication
  - Deleting an existing owned comment succeeds with status 204
  - After deletion, the deleted comment no longer appears in later listing
  - When two comments exist and only one is deleted, the other comment remains visible
  - Deleting another user's comment is forbidden (403)
- **Input Constraints**:
  - Target article slug: non-empty string identifying an existing article
  - Comment body: non-empty string (minimum length constraint inferred; spec does not state explicit maximum)
  - Authentication token: valid JWT token for create/delete operations
  - Comment identifier: integer id for deletion targeting
- **Error Conditions**:
  - 401 for unauthenticated comment creation or deletion
  - 403 for deleting another user's comment
  - 404 (inferred) for non-existent article slug or comment id

### 2.2 Requirement Items

| Requirement ID | Requirement Description                                                                                                                                                                        | Priority | Notes                                                         |
| -------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ------------------------------------------------------------- |
| R1             | Posting a comment with authentication on an existing article succeeds with status 201; the response contains a comment object with integer id, body, createdAt, updatedAt, and author username | High     | Core creation happy path                                      |
| R2             | Listing comments for an article with authentication succeeds with status 200; returns an array of comment objects                                                                              | High     | Authenticated listing                                         |
| R3             | Listing comments for an article without authentication succeeds with status 200; returns an array of comment objects with id, body, timestamps, and author username                            | High     | Unauthenticated listing — explicitly required by spec         |
| R4             | Posting a comment without authentication is rejected with status 401; the error response indicates the token is missing                                                                        | High     | Auth gate on create                                           |
| R5             | Deleting a comment without authentication is rejected with status 401; the error response indicates the token is missing                                                                       | High     | Auth gate on delete                                           |
| R6             | Deleting an existing owned comment succeeds with status 204                                                                                                                                    | High     | Core deletion happy path                                      |
| R7             | After a comment is deleted, it no longer appears in subsequent comment listings for that article                                                                                               | High     | Deletion persistence                                          |
| R8             | When two comments exist on an article and one is deleted, the remaining comment is still visible in the listing                                                                                | High     | Selective deletion correctness                                |
| R9             | Deleting another user's comment is rejected with status 403; the comment survives the failed deletion attempt                                                                                  | High     | Authorization boundary — from errors-authorization bruno spec |

### 2.3 Assumptions About Input

- Assumption 1: The API uses JSON request/response bodies with a top-level `comment` key wrapping comment fields, consistent with the RealWorld API specification and bruno test files.
- Assumption 2: Authentication is provided via an `Authorization: Token {jwt}` header, consistent with the bruno specs and RealWorld convention.
- Assumption 3: The comment body field is required and non-empty; the spec does not define maximum length, so boundary testing for body length is limited to empty vs non-empty.
- Assumption 4: Article slugs are generated by the system on article creation; tests must create an article first to obtain a valid slug.
- Assumption 5: The deletion endpoint returns 204 with no body on success, consistent with the bruno spec assertion.

## 3. Test Design Strategy

### 3.1 Applied Black-Box Techniques

| Technique                   | Applied? | Where Used                                                                                                                                               | Notes                                          |
| --------------------------- | -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------- |
| Equivalence Partitioning    | Yes      | Comment body input (valid non-empty vs invalid empty), auth state (present/absent/wrong-user)                                                            | Partitions for creation, deletion, and listing |
| Boundary Value Analysis     | Yes      | Comment body empty vs non-empty boundary; single-character body as minimum valid                                                                         | String-length boundary for comment body        |
| Decision Table Testing      | Yes      | Auth state × operation: authenticated create, unauthenticated create, authenticated delete (own), authenticated delete (other's), unauthenticated delete | 5 combinations of auth × operation             |
| State Transition Testing    | Yes      | Comment lifecycle: create → list → delete → list (verify absence); create two → delete one → list (verify survivor)                                      | State persistence and selective deletion       |
| Error Guessing              | Yes      | Non-existent article slug, non-existent comment id, empty body, missing body field                                                                       | Common API error patterns                      |
| Scenario / Use-Case Testing | Yes      | Full comment lifecycle: register → create article → create comment → list → delete → verify deletion                                                     | Primary acceptance criteria                    |

### 3.2 Test Dimension Summary

- Valid input classes: non-empty comment body, authenticated user, existing article slug
- Invalid input classes: empty comment body, missing comment body field, unauthenticated request, wrong-user deletion, non-existent article slug, non-existent comment id
- Boundary values: empty-string comment body (just below minimum valid), single-character comment body (minimum valid)
- Empty / null / missing cases: empty-string comment body, omitted comment body field in JSON, missing Authorization header
- Format-related cases: N/A — comment body is a plain string with no specified format constraints
- Permission / role cases: unauthenticated create, unauthenticated delete, different-user delete (403)
- State / sequencing cases: create → delete → list (verify deletion persistence); create two → delete one → list (selective deletion)
- Combination cases: unauthenticated + create, unauthenticated + delete, wrong-user + delete

### 3.3 Edge-Case Design Notes

- Empty-string comment body: The boundary between valid and invalid comment body is empty vs non-empty; testing whether empty body is rejected (spec does not explicitly state, but inferred from required-field convention).
- Missing body field: Sending `{"comment": {}}` without a `body` key — structural omission vs empty value.
- Deleted comment re-deletion: Attempting to delete a comment that has already been deleted — state-related edge.
- Non-existent comment id: Deleting a comment id that does not exist — resource-not-found edge.
- Non-existent article slug: Creating/listing/deleting comments on a slug that doesn't exist — parent-resource edge.
- Authorization cross-user: User B deleting User A's comment — the spec (errors-authorization bruno) explicitly expects 403.
- Selective deletion with identical timestamps: Two comments created in rapid succession — verifies id-based targeting, not timestamp-based.

## 4. Equivalence Partitioning Analysis

| EP ID | Requirement ID | Input / Rule            | Partition Type | Description                       | Expected Outcome                                                | Covered by Test Case ID |
| ----- | -------------- | ----------------------- | -------------- | --------------------------------- | --------------------------------------------------------------- | ----------------------- |
| EP1   | R1             | Comment body            | Valid          | Non-empty string body             | 201 with comment object containing id, body, timestamps, author | TC01, TC02              |
| EP2   | R1             | Comment body            | Invalid        | Empty string body                 | 422 (inferred) or 400 with validation error                     | TC10                    |
| EP3   | R1             | Comment body            | Invalid        | Missing body field in JSON        | 422 or 400 with validation error                                | TC11                    |
| EP4   | R1             | Auth token              | Valid          | Valid token for registered user   | 201 on create                                                   | TC01                    |
| EP5   | R4             | Auth token              | Invalid        | No Authorization header           | 401 with token missing error                                    | TC04                    |
| EP6   | R5             | Auth token              | Invalid        | No Authorization header on delete | 401 with token missing error                                    | TC05                    |
| EP7   | R2, R3         | Auth on list            | Valid (both)   | Authenticated or unauthenticated  | 200 with comments array                                         | TC02, TC03              |
| EP8   | R6             | Delete auth + ownership | Valid          | Authenticated as comment owner    | 204                                                             | TC06                    |
| EP9   | R9             | Delete auth + ownership | Invalid        | Authenticated as non-owner        | 403 with forbidden error                                        | TC09                    |
| EP10  | R7             | Deleted comment state   | Invalid        | Comment previously deleted        | Not present in listing                                          | TC07                    |
| EP11  | R8             | Selective deletion      | Valid          | One of two comments deleted       | Remaining comment still present                                 | TC08                    |

### 4.1 EP Coverage Notes

- Covered partitions: All valid and invalid partitions for the in-scope requirements (R1–R9).
- Missing partitions: Maximum-length comment body — the spec does not define maximum length, so this partition cannot be tested against a known expected outcome.
- Partially covered partitions: EP2/EP3 (empty/missing comment body) — the spec does not explicitly define the rejection status code for invalid comment bodies; the tests assume 422 or 400 but this is an inference.

## 5. Boundary Value Analysis

| BVA ID | Requirement ID | Boundary Item             | Boundary Definition                                  | Test Values                                                        | Expected Outcome                               | Covered by Test Case ID |
| ------ | -------------- | ------------------------- | ---------------------------------------------------- | ------------------------------------------------------------------ | ---------------------------------------------- | ----------------------- |
| B1     | R1             | Comment body length       | Min valid vs just-below-min (empty)                  | `""` (empty), `"a"` (single char), `"Test comment body"` (typical) | `""` → rejection; non-empty → 201              | TC01, TC10, TC12        |
| B2     | R1             | Comment id                | Min valid (auto-generated integer)                   | First comment id from creation response                            | Integer type, positive                         | TC01                    |
| B3     | R6, R7         | Deletion state transition | Before deletion (present) vs after deletion (absent) | List before delete vs list after delete                            | Before: comment present; After: comment absent | TC07                    |

### 5.1 BVA Coverage Notes

- Explicit boundaries tested: Empty vs non-empty comment body; comment id type (integer); deletion state transition (present → absent).
- Missing boundaries: Maximum comment body length — not specified in the requirements.
- Ambiguous boundary definitions from requirements: The spec does not define whether a comment body of `""` (empty string) or `" "` (whitespace-only) is rejected. The tests assume empty string is rejected but this is an inference.

## 6. Test Scenarios

| Scenario ID | Requirement Reference | Scenario Title                              | Scenario Type | Description                                                                                             | Priority |
| ----------- | --------------------- | ------------------------------------------- | ------------- | ------------------------------------------------------------------------------------------------------- | -------- |
| S1          | R1                    | Create comment successfully                 | Happy Path    | Authenticated user creates a comment on an existing article; verify 201 and response payload structure  | High     |
| S2          | R1                    | Create comment response payload correctness | Happy Path    | Verify the returned comment object contains integer id, body, createdAt, updatedAt, and author username | High     |
| S3          | R2                    | List comments with authentication           | Happy Path    | Authenticated user lists comments for an article; verify 200 and array structure                        | High     |
| S4          | R3                    | List comments without authentication        | Happy Path    | Unauthenticated user lists comments; verify 200 and same payload structure                              | High     |
| S5          | R4                    | Create comment without authentication       | Negative      | Attempt to create a comment without auth token; verify 401 with token error                             | High     |
| S6          | R5                    | Delete comment without authentication       | Negative      | Attempt to delete a comment without auth token; verify 401 with token error                             | High     |
| S7          | R6                    | Delete owned comment successfully           | Happy Path    | Authenticated user deletes their own comment; verify 204                                                | High     |
| S8          | R7                    | Deleted comment absent from listing         | State         | After deletion, list comments and verify the deleted comment is gone                                    | High     |
| S9          | R8                    | Selective deletion preserves other comment  | State         | Create two comments, delete one, verify the other remains                                               | High     |
| S10         | R9                    | Delete another user's comment is forbidden  | Permission    | User B attempts to delete User A's comment; verify 403 and comment survives                             | High     |
| S11         | R1                    | Create comment with empty body              | Boundary      | Attempt to create a comment with empty string body; verify rejection                                    | Medium   |
| S12         | R1                    | Create comment with missing body field      | Edge          | Send comment JSON without body key; verify rejection                                                    | Medium   |
| S13         | R1                    | Create comment with single-character body   | Boundary      | Create a comment with body `"a"`; verify 201                                                            | Medium   |
| S14         | R6                    | Delete non-existent comment id              | Edge          | Attempt to delete a comment id that doesn't exist; verify 404 or error                                  | Medium   |
| S15         | R1                    | Create comment on non-existent article      | Edge          | Attempt to create a comment using a slug that doesn't exist; verify 404                                 | Medium   |
| S16         | R6                    | Delete already-deleted comment              | State         | Delete a comment, then attempt to delete it again; verify 404 or error                                  | Low      |

## 7. Edge Case Matrix

| Requirement ID | Edge Category | Concrete Case                             | Covered by Test Case ID | Notes                              |
| -------------- | ------------- | ----------------------------------------- | ----------------------- | ---------------------------------- |
| R1             | Empty         | Empty-string comment body                 | TC10                    | Boundary between valid/invalid     |
| R1             | Missing       | Omitted body field in comment JSON        | TC11                    | Structural omission vs empty value |
| R1             | Boundary      | Single-character comment body             | TC12                    | Minimum valid body                 |
| R1             | State         | Non-existent article slug on create       | TC13                    | Parent resource must exist         |
| R2             | Permission    | List comments without auth                | TC03                    | Explicitly required by spec        |
| R4             | Missing       | No Authorization header on create         | TC04                    | Explicitly required by spec        |
| R5             | Missing       | No Authorization header on delete         | TC05                    | Explicitly required by spec        |
| R6             | State         | Delete already-deleted comment            | TC15                    | Double-delete edge                 |
| R6             | Missing       | Delete non-existent comment id            | TC14                    | Resource-not-found edge            |
| R7             | State         | Verify deletion persistence via listing   | TC07                    | State transition verification      |
| R8             | Combination   | Two comments, delete one, verify survivor | TC08                    | Selective side-effect correctness  |
| R9             | Permission    | Cross-user deletion forbidden             | TC09                    | Authorization boundary             |

## 8. Detailed Test Cases

| Test Case ID | Title                                      | Requirement Reference | Preconditions                                                                          | Test Data                           | Steps                                                                                                                                                                                          | Expected Result                                                                                                                                                                                | Priority | Risk / Notes                                          |
| ------------ | ------------------------------------------ | --------------------- | -------------------------------------------------------------------------------------- | ----------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ----------------------------------------------------- |
| TC01         | Create comment successfully                | R1                    | API server running; user registered and authenticated; article created with valid slug | body=`"Test comment body"`          | 1. Register user and obtain token 2. Create article and obtain slug 3. POST `/api/articles/{slug}/comments` with `{"comment":{"body":"Test comment body"}}` and `Authorization: Token {token}` | Status 201; response contains `comment` object with `id` (integer), `body`="Test comment body", `createdAt` (ISO 8601), `updatedAt` (ISO 8601), `author.username` matching registered username | High     | Core happy path                                       |
| TC02         | List comments with authentication          | R2                    | Same setup as TC01; one comment exists on the article                                  | Authenticated GET                   | 1. GET `/api/articles/{slug}/comments` with `Authorization: Token {token}`                                                                                                                     | Status 200; response contains `comments` array with at least one comment; each comment has `id`, `body`, `createdAt`, `updatedAt`, `author.username`                                           | High     | Authenticated listing                                 |
| TC03         | List comments without authentication       | R3                    | Same article and comment as TC01 exist                                                 | Unauthenticated GET                 | 1. GET `/api/articles/{slug}/comments` without Authorization header                                                                                                                            | Status 200; response contains `comments` array; each comment has `id` (integer), `body`, `createdAt`, `updatedAt`, `author.username`                                                           | High     | Explicitly required by spec                           |
| TC04         | Create comment without authentication      | R4                    | API server running; article exists                                                     | No auth token, body=`"test"`        | 1. POST `/api/articles/{slug}/comments` with `{"comment":{"body":"test"}}` and no Authorization header                                                                                         | Status 401; `errors.token[0]` equals `"is missing"`                                                                                                                                            | High     | Explicit acceptance criterion                         |
| TC05         | Delete comment without authentication      | R5                    | API server running; comment exists                                                     | No auth token                       | 1. DELETE `/api/articles/{slug}/comments/{id}` without Authorization header                                                                                                                    | Status 401; `errors.token[0]` equals `"is missing"`                                                                                                                                            | High     | Explicit acceptance criterion                         |
| TC06         | Delete owned comment successfully          | R6                    | User has created a comment on an article                                               | Auth token of comment owner         | 1. DELETE `/api/articles/{slug}/comments/{id}` with `Authorization: Token {token}`                                                                                                             | Status 204; no response body                                                                                                                                                                   | High     | Core deletion happy path                              |
| TC07         | Deleted comment absent from listing        | R7                    | Comment has been successfully deleted (TC06)                                           | Same article slug                   | 1. After deleting comment, GET `/api/articles/{slug}/comments`                                                                                                                                 | Status 200; `comments` array does not contain the deleted comment's id                                                                                                                         | High     | Deletion persistence verification                     |
| TC08         | Selective deletion preserves other comment | R8                    | Two comments exist on the same article by the same user                                | Two comment ids, delete first only  | 1. Create comment "First comment" 2. Create comment "Second comment" 3. DELETE first comment 4. GET `/api/articles/{slug}/comments`                                                            | Status 200 on list; `comments` array has length 1; the remaining comment has `body`="Second comment"                                                                                           | High     | Explicit acceptance criterion                         |
| TC09         | Delete another user's comment is forbidden | R9                    | User A created a comment; User B is authenticated                                      | User B's token, User A's comment id | 1. Register User A and User B 2. User A creates article and comment 3. User B DELETEs User A's comment                                                                                         | Status 403; `errors.comment[0]` equals `"forbidden"`; subsequent listing shows the comment still exists                                                                                        | High     | Authorization boundary from errors-authorization spec |
| TC10         | Create comment with empty body             | R1                    | API server running; user authenticated; article exists                                 | body=`""`                           | 1. POST `/api/articles/{slug}/comments` with `{"comment":{"body":""}}`                                                                                                                         | Rejected (422 or 400) with body validation error                                                                                                                                               | Medium   | Boundary: just-below-minimum valid                    |
| TC11         | Create comment with missing body field     | R1                    | API server running; user authenticated; article exists                                 | `{"comment":{}}`                    | 1. POST `/api/articles/{slug}/comments` with `{"comment":{}}`                                                                                                                                  | Rejected (422 or 400) with body validation error                                                                                                                                               | Medium   | Structural omission                                   |
| TC12         | Create comment with single-character body  | R1                    | API server running; user authenticated; article exists                                 | body=`"a"`                          | 1. POST `/api/articles/{slug}/comments` with `{"comment":{"body":"a"}}`                                                                                                                        | Status 201; response `comment.body` equals `"a"`                                                                                                                                               | Medium   | Minimum valid boundary                                |
| TC13         | Create comment on non-existent article     | R1                    | API server running; user authenticated                                                 | slug=`"non-existent-slug-xyz"`      | 1. POST `/api/articles/non-existent-slug-xyz/comments` with valid auth and body                                                                                                                | Status 404 (parent resource not found)                                                                                                                                                         | Medium   | Parent-resource edge                                  |
| TC14         | Delete non-existent comment id             | R6                    | API server running; user authenticated; article exists                                 | comment id=999999 (non-existent)    | 1. DELETE `/api/articles/{slug}/comments/999999` with valid auth                                                                                                                               | Status 404 (resource not found)                                                                                                                                                                | Medium   | Resource-not-found edge                               |
| TC15         | Delete already-deleted comment             | R6                    | Comment has been deleted once                                                          | Same comment id, valid auth         | 1. Delete comment (204) 2. Delete same comment id again                                                                                                                                        | Status 404 (already deleted)                                                                                                                                                                   | Low      | Double-delete edge                                    |

## 9. How to Run the Generated Test Codes

### 9.1 Prerequisites

- Target system: ASP.NET Core RealWorld implementation running and reachable
- .NET 8 SDK (or later) installed
- xUnit test runner available via `dotnet test`
- The RealWorld API server must be started before running tests (see Environment Setup)

### 9.2 Test Code Location

- **Generated Test Code Path**: `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/`
- **Project Root Path**: `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/`
- **Test Entry File / Directory**: `CommentLifecycleTests.cs`
- **Related Configuration Files**: `RealWorld.Blackbox.Tests.csproj`

### 9.3 Environment Setup

1. **Working Directory**: `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/`
2. **Runtime Version**: .NET 8 SDK or later
3. **Dependency Install Command**: `dotnet restore`
4. **Build Command**: `dotnet build`
5. **Test Environment Variables**:
   - `REALWORLD_BASE_URL`: Base URL of the running RealWorld API (default: `http://localhost:5000`)
6. **Service Start Command**: Start the ASP.NET Core RealWorld server on the configured host/port before running tests
7. **Test Data / Seed Command**: No seed data required; tests create their own users, articles, and comments with unique IDs

### 9.4 Run Commands

```bash
# Restore dependencies
dotnet restore

# Run all tests
dotnet test

# Run all tests with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run only comment lifecycle tests
dotnet test --filter "FullyQualifiedName~CommentLifecycleTests"

# Run a specific test by name
dotnet test --filter "FullyQualifiedName~CreateCommentSuccessfully"

# Run with custom base URL
REALWORLD_BASE_URL=http://localhost:5000 dotnet test

# Force re-execution (no caching)
dotnet test --no-build && dotnet test
```

### 9.5 Execution Notes

- Start the target service before running black-box tests. Tests will fail with connection errors if the server is unreachable.
- Each test generates unique user IDs (GUID-based) to avoid collisions with existing data. Tests are idempotent and can be re-run without manual cleanup.
- TC08 (selective deletion) and TC09 (cross-user authorization) involve multi-step setups; they register distinct users and create distinct articles/comments to ensure test isolation.
- TC10, TC11 (empty/missing body) and TC13, TC14, TC15 (non-existent resources, double-delete) test edge cases whose exact status codes are inferred from common API conventions. The implementation may differ; the tests document this uncertainty.
- If the test framework caches results, force re-execution with `dotnet test --no-build` after a clean build.
- Record the exact `REALWORLD_BASE_URL` value used for reproducibility.

## 10. Coverage Summary

### 10.1 Requirement Coverage Table

| Requirement ID | EP Covered? | BVA Covered? | Edge Case Covered? | Negative Case Covered? | State / Sequence Covered? | Covered by Test Cases        | Coverage Status | Notes                                                                    |
| -------------- | ----------- | ------------ | ------------------ | ---------------------- | ------------------------- | ---------------------------- | --------------- | ------------------------------------------------------------------------ |
| R1             | Yes         | Yes          | Yes                | Yes                    | N/A                       | TC01, TC10, TC11, TC12, TC13 | Full            | Body boundary (empty/missing/min-valid) and parent-resource edge covered |
| R2             | Yes         | N/A          | N/A                | N/A                    | N/A                       | TC02                         | Full            | Simple listing; no negative case applicable                              |
| R3             | Yes         | N/A          | N/A                | N/A                    | N/A                       | TC03                         | Full            | Unauthenticated listing; no negative case applicable                     |
| R4             | Yes         | N/A          | Yes                | Yes                    | N/A                       | TC04                         | Full            | Missing auth on create                                                   |
| R5             | Yes         | N/A          | Yes                | Yes                    | N/A                       | TC05                         | Full            | Missing auth on delete                                                   |
| R6             | Yes         | Yes          | Yes                | Yes                    | Yes                       | TC06, TC14, TC15             | Full            | Happy path, non-existent id, double-delete                               |
| R7             | Yes         | Yes          | Yes                | N/A                    | Yes                       | TC07                         | Full            | Deletion persistence via listing                                         |
| R8             | Yes         | N/A          | Yes                | N/A                    | Yes                       | TC08                         | Full            | Selective deletion with state verification                               |
| R9             | Yes         | N/A          | Yes                | Yes                    | Yes                       | TC09                         | Full            | Cross-user forbidden delete + survival verification                      |

### 10.2 EP / BVA to Test Case Mapping

| Analysis Item ID | Type | Requirement ID | Description                              | Mapped Test Case ID(s) | Covered? | Notes |
| ---------------- | ---- | -------------- | ---------------------------------------- | ---------------------- | -------- | ----- |
| EP1              | EP   | R1             | Valid: non-empty comment body            | TC01, TC02             | Yes      |       |
| EP2              | EP   | R1             | Invalid: empty comment body              | TC10                   | Yes      |       |
| EP3              | EP   | R1             | Invalid: missing body field              | TC11                   | Yes      |       |
| EP4              | EP   | R1             | Valid: authenticated create              | TC01                   | Yes      |       |
| EP5              | EP   | R4             | Invalid: no auth on create               | TC04                   | Yes      |       |
| EP6              | EP   | R5             | Invalid: no auth on delete               | TC05                   | Yes      |       |
| EP7              | EP   | R2, R3         | Valid: list with/without auth            | TC02, TC03             | Yes      |       |
| EP8              | EP   | R6             | Valid: authenticated owner delete        | TC06                   | Yes      |       |
| EP9              | EP   | R9             | Invalid: non-owner delete                | TC09                   | Yes      |       |
| EP10             | EP   | R7             | Invalid: deleted comment state           | TC07                   | Yes      |       |
| EP11             | EP   | R8             | Valid: selective deletion survivor       | TC08                   | Yes      |       |
| B1               | BVA  | R1             | Comment body empty vs non-empty boundary | TC01, TC10, TC12       | Yes      |       |
| B2               | BVA  | R1             | Comment id integer type                  | TC01                   | Yes      |       |
| B3               | BVA  | R6, R7         | Deletion state transition                | TC07                   | Yes      |       |

### 10.3 Coverage Metrics

| Metric                 | Formula                                              | Value        |
| ---------------------- | ---------------------------------------------------- | ------------ |
| Requirement Coverage   | covered_requirements / total_requirements            | 9/9 = 100%   |
| EP Coverage            | covered_partitions / total_partitions                | 11/11 = 100% |
| BVA Coverage           | covered_boundaries / total_boundaries                | 3/3 = 100%   |
| Edge Case Coverage     | covered_edge_categories / applicable_edge_categories | 12/12 = 100% |
| Negative Case Coverage | negative_cases_present / applicable_requirements     | 4/4 = 100%   |
| Duplicate Case Rate    | duplicate_cases / total_cases                        | 0/15 = 0%    |
| Executability Score    | 1-5                                                  | 4            |

### 10.4 Coverage Notes

- Strongest covered area: R1 (comment creation) — covers valid, empty, missing, boundary, and parent-resource edges. R9 (authorization) — covers forbidden delete with survival verification.
- Weakest covered area: R2/R3 (listing) — only covers happy path; no edge cases for listing on non-existent slug or malformed responses.
- Over-covered or duplicated areas: None identified.
- Under-covered areas: Maximum comment body length, whitespace-only body, listing comments on a non-existent article slug, and listing with an invalid/expired token — these are not specified in the requirements but recommended for a comprehensive test suite.

## 11. Ambiguities / Missing Information / Assumptions

### 11.1 Ambiguous Requirements

- Item 1: The spec says valid comment creation returns 201, but does not specify the expected status code when the comment body is empty or missing. The tests assume 422 or 400, but the implementation may return a different status.
- Item 2: The spec says unauthenticated create/delete returns 401, but does not specify whether an expired or malformed token (present but invalid) also returns 401 or a different error. The tests only cover the "no token" case.
- Item 3: The spec does not explicitly state whether deleting a non-existent comment id returns 404 or some other status. The tests assume 404.

### 11.2 Missing Information

- Missing validation rule: Whether the comment body has a maximum length constraint; whether whitespace-only bodies are accepted.
- Missing boundary definition: Maximum length for comment body is not specified.
- Missing error behavior: Status code and error format for creating a comment on a non-existent article slug.
- Missing error behavior: Status code and error format for deleting a non-existent comment id.
- Missing state transition rule: Whether a deleted comment can be deleted again (double-delete behavior).

### 11.3 Assumptions

- Assumption 1: The API uses `{"comment": {"body": "..."}}` JSON wrapper consistent with the bruno specs and RealWorld API convention.
- Assumption 2: Authentication uses `Authorization: Token {jwt}` header format, consistent with the bruno specs.
- Assumption 3: Empty comment body is rejected with 422 or 400; the exact status code is not specified in the requirements.
- Assumption 4: Non-existent article slugs and comment ids return 404; the exact behavior is not specified in the requirements.
- Assumption 5: The `errors.token[0]` error message for unauthenticated requests equals `"is missing"`, as shown in the bruno error specs.
- Assumption 6: The `errors.comment[0]` error message for cross-user deletion equals `"forbidden"`, as shown in the errors-authorization bruno spec.
