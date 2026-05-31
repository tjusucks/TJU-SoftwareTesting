# Black-Box Testing Run Report Template

## 1. Run Metadata

| Field                                    | Value                                                                                                                                            |
| ---------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| Project Name                             | RealWorld (Conduit)                                                                                                                              |
| Feature Name                             | Settings Null Fields                                                                                                                             |
| Run ID                                   | BB-ASPNETCORE-SETTINGS-NULL-001                                                                                                                  |
| Date                                     | 2026-04-19                                                                                                                                       |
| Author / Operator                        | Claude (blackbox-testing skill)                                                                                                                  |
| Skill / Tool Name                        | `blackbox-testing`                                                                                                                               |
| Model / Agent Version                    | glm-5.1                                                                                                                                          |
| Prompt Version                           | 1.0                                                                                                                                              |
| Input Type                               | Requirement / API Spec (Mixed)                                                                                                                   |
| Input Source Path / Link                 | `Assignment 01/codebases/realworld/specification/features/settings-null-fields.md` + upstream bruno specs                                        |
| Target System / Implementation           | ASP.NET Core RealWorld implementation                                                                                                            |
| Target Module / Endpoint / Feature Scope | `PUT /api/user` (update user), `GET /api/user` (get current user)                                                                                |
| Execution Scope                          | Design + Automation                                                                                                                              |
| Notes                                    | Focuses on nullable vs non-nullable field semantics: empty-string normalization, null acceptance, and rejection of null/empty on required fields |

## 2. Input Summary

### 2.1 Input Overview

- **Project / System Under Test**: RealWorld (Conduit) — a Medium.com-clone backend API
- **Feature Under Test**: Profile update semantics for nullable and non-nullable user fields, with emphasis on empty-string normalization and null handling
- **Actors**: Authenticated user (updating own profile settings)
- **Preconditions**: The RealWorld API server is running; an authenticated user exists with a valid token
- **Business Rules**:
  - Nullable fields (bio, image): updating to empty string normalizes to null; updating to explicit null is accepted
  - Non-nullable fields (username, email): updating to empty string is rejected; updating to null is rejected
  - Normalization and null assignments must persist when the user profile is fetched again
- **Input Constraints**:
  - PUT /api/user requires Authorization header with valid token
  - Request body wrapped in `user` key; fields may include username, email, bio, image
  - username and email must be non-null and non-empty strings
  - bio and image accept null or may be set to empty string (normalized to null)
- **Error Conditions**:
  - 422 for null or empty-string values on non-nullable fields (username, email)

### 2.2 Requirement Items

| Requirement ID | Requirement Description                                                                    | Priority | Notes                                            |
| -------------- | ------------------------------------------------------------------------------------------ | -------- | ------------------------------------------------ |
| R1             | Updating bio to an empty string succeeds (200) and the response normalizes bio to null     | High     | Empty-string normalization for nullable field    |
| R2             | Updating bio to null succeeds (200) and the response reflects bio as null                  | High     | Explicit null acceptance for nullable field      |
| R3             | Updating image to an empty string succeeds (200) and the response normalizes image to null | High     | Empty-string normalization for nullable field    |
| R4             | Updating image to null succeeds (200) and the response reflects image as null              | High     | Explicit null acceptance for nullable field      |
| R5             | Normalized/null values persist when the user profile is fetched again via GET /api/user    | High     | Persistence verification across read-after-write |
| R6             | Updating email to an empty string is rejected with 422                                     | High     | Non-nullable field rejection                     |
| R7             | Updating username to an empty string is rejected with 422                                  | High     | Non-nullable field rejection                     |
| R8             | Updating email to null is rejected with 422                                                | High     | Non-nullable field rejection                     |
| R9             | Updating username to null is rejected with 422                                             | High     | Non-nullable field rejection                     |

### 2.3 Assumptions About Input

- Assumption 1: The API uses JSON request/response bodies with a top-level `user` key wrapping user fields, consistent with the RealWorld API specification and bruno test files.
- Assumption 2: The error response format for validation errors is `{ "errors": { "field": ["message"] } }` or `{ "errors": { "Field": "message" } }`, as shown in the upstream bruno specs and prior test experience with this implementation.
- Assumption 3: Empty-string normalization means the API stores null internally and returns null (not empty string) in both the PUT response and subsequent GET responses.
- Assumption 4: When updating a single field (e.g., bio only), other fields retain their existing values — the spec does not state this explicitly but it is implied by partial-update semantics.

## 3. Test Design Strategy

### 3.1 Applied Black-Box Techniques

| Technique                   | Applied? | Where Used                                                                                                  | Notes                                      |
| --------------------------- | -------- | ----------------------------------------------------------------------------------------------------------- | ------------------------------------------ |
| Equivalence Partitioning    | Yes      | Nullable vs non-nullable fields; valid/invalid/null/empty partitions for each                               | Primary technique for this feature         |
| Boundary Value Analysis     | Yes      | Empty string as boundary between valid content and null; null as boundary of valid input                    | Empty ↔ null is the core boundary          |
| Decision Table Testing      | Yes      | Field class (nullable/non-nullable) × input value (valid/empty/null)                                        | 2 × 3 decision matrix                      |
| State Transition Testing    | Yes      | Read-after-write persistence: write null → read back null                                                   | State verification across requests         |
| Error Guessing              | Yes      | Whitespace-only strings; updating both nullable and non-nullable fields simultaneously                      | Additional edge cases beyond explicit spec |
| Scenario / Use-Case Testing | Yes      | Full cycle: set bio → normalize to null → verify persisted → restore → set explicit null → verify persisted | Sequencing scenarios from bruno specs      |

### 3.2 Test Dimension Summary

- Valid input classes: bio/image set to a non-empty string; bio/image set to null; bio/image set to empty string (normalized)
- Invalid input classes: username/email set to empty string; username/email set to null
- Boundary values: empty string (boundary between valid content and null normalization); null (boundary of accepted input for nullable fields; boundary of rejected input for non-nullable fields)
- Empty / null / missing cases: empty-string bio → null; null bio → null; empty-string image → null; null image → null; empty-string email → 422; null email → 422; empty-string username → 422; null username → 422
- Format-related cases: whitespace-only bio/image (syntactically non-empty but semantically blank); whitespace-only username/email
- Permission / role cases: N/A — no role differentiation in this feature scope
- State / sequencing cases: write-then-read persistence for normalized and null values
- Combination cases: updating both nullable and non-nullable fields in a single request; setting bio to empty while simultaneously updating email

### 3.3 Edge-Case Design Notes

- Empty-string normalization: The key subtlety is that `""` on a nullable field is not an error — it is silently converted to null. This is the core behavioral boundary and the primary test target.
- Null vs omitted field: Sending `{"user": {"bio": null}}` is different from sending `{"user": {}}` (bio omitted). The spec only covers explicit null; omission behavior is untested but noted as ambiguous.
- Whitespace-only strings: `" "` is non-empty and should not be normalized to null per the spec (only empty string `""` is normalized). This is an edge case that tests whether the implementation over-normalizes.
- Persistence verification: The spec requires that normalization persists across GET. This is a two-step (write-then-read) state verification, not just a single-request check.
- Sequential state changes: Setting bio to a value, then to empty string, then to null — each step should produce the correct intermediate state. The bruno specs follow this pattern.

## 4. Equivalence Partitioning Analysis

| EP ID | Requirement ID | Input / Rule            | Partition Type     | Description                                             | Expected Outcome                               | Covered by Test Case ID |
| ----- | -------------- | ----------------------- | ------------------ | ------------------------------------------------------- | ---------------------------------------------- | ----------------------- |
| EP1   | R1             | bio field value         | Valid              | Non-empty string (e.g., "Hello world")                  | 200; bio = "Hello world" in response           | TC01                    |
| EP2   | R1             | bio field value         | Valid (normalized) | Empty string `""`                                       | 200; bio = null in response                    | TC02                    |
| EP3   | R2             | bio field value         | Valid              | Explicit null                                           | 200; bio = null in response                    | TC03                    |
| EP4   | R3             | image field value       | Valid              | Non-empty URL string                                    | 200; image = URL string in response            | TC04                    |
| EP5   | R3             | image field value       | Valid (normalized) | Empty string `""`                                       | 200; image = null in response                  | TC05                    |
| EP6   | R4             | image field value       | Valid              | Explicit null                                           | 200; image = null in response                  | TC06                    |
| EP7   | R5             | Persistence after write | Valid              | GET /api/user after bio/image set to null or normalized | 200; bio/image = null in GET response          | TC07, TC08, TC09, TC10  |
| EP8   | R6             | email field value       | Invalid            | Empty string `""`                                       | 422 with email validation error                | TC11                    |
| EP9   | R7             | username field value    | Invalid            | Empty string `""`                                       | 422 with username validation error             | TC12                    |
| EP10  | R8             | email field value       | Invalid            | Explicit null                                           | 422 with email validation error                | TC13                    |
| EP11  | R9             | username field value    | Invalid            | Explicit null                                           | 422 with username validation error             | TC14                    |
| EP12  | R1             | bio field value         | Boundary           | Whitespace-only string `" "`                            | 200; bio = `" "` (not normalized, non-empty)   | TC15                    |
| EP13  | R3             | image field value       | Boundary           | Whitespace-only string `" "`                            | 200; image = `" "` (not normalized, non-empty) | TC16                    |

### 4.1 EP Coverage Notes

- Covered partitions: All valid partitions for nullable fields (non-empty, empty-string, null) and all invalid partitions for non-nullable fields (empty-string, null).
- Missing partitions: Invalid email format on update (e.g., `"not-an-email"`) — the spec only covers empty and null, not format validation. Duplicate username/email on update is also out of scope for this feature slice.
- Partially covered partitions: EP12/EP13 (whitespace-only) — the spec does not explicitly state the expected behavior; these tests document the assumption that whitespace-only strings are NOT normalized to null.

## 5. Boundary Value Analysis

| BVA ID | Requirement ID | Boundary Item                 | Boundary Definition                | Test Values                              | Expected Outcome                                       | Covered by Test Case ID |
| ------ | -------------- | ----------------------------- | ---------------------------------- | ---------------------------------------- | ------------------------------------------------------ | ----------------------- |
| B1     | R1, R2         | bio empty ↔ null              | Empty string normalizes to null    | `""` (empty), `null`, `" "` (whitespace) | `""` → null; `null` → null; `" "` → preserved as `" "` | TC02, TC03, TC15        |
| B2     | R3, R4         | image empty ↔ null            | Empty string normalizes to null    | `""` (empty), `null`, `" "` (whitespace) | `""` → null; `null` → null; `" "` → preserved as `" "` | TC05, TC06, TC16        |
| B3     | R6, R8         | email null/empty rejection    | Boundary between valid and invalid | `""` (empty), `null`, `"a@b.c"` (valid)  | `""` → 422; `null` → 422; valid → 200                  | TC11, TC13              |
| B4     | R7, R9         | username null/empty rejection | Boundary between valid and invalid | `""` (empty), `null`, `"user"` (valid)   | `""` → 422; `null` → 422; valid → 200                  | TC12, TC14              |

### 5.1 BVA Coverage Notes

- Explicit boundaries tested: Empty-string-to-null normalization boundary for bio and image; null/empty rejection boundary for username and email.
- Missing boundaries: Maximum length boundaries for bio, image, username, and email — the spec does not define maximum lengths.
- Ambiguous boundary definitions from requirements: The spec does not clarify whether whitespace-only strings (`" "`) should be treated as empty (and thus normalized to null) or treated as non-empty content. The tests assume non-empty (not normalized).

## 6. Test Scenarios

| Scenario ID | Requirement Reference | Scenario Title                                     | Scenario Type         | Description                                                                                           | Priority |
| ----------- | --------------------- | -------------------------------------------------- | --------------------- | ----------------------------------------------------------------------------------------------------- | -------- |
| S1          | R1                    | Bio empty-string normalization                     | Happy Path / Boundary | Update bio to empty string; verify 200 and bio is null in response                                    | High     |
| S2          | R2                    | Bio explicit null acceptance                       | Happy Path / Boundary | Update bio to null; verify 200 and bio is null in response                                            | High     |
| S3          | R3                    | Image empty-string normalization                   | Happy Path / Boundary | Update image to empty string; verify 200 and image is null in response                                | High     |
| S4          | R4                    | Image explicit null acceptance                     | Happy Path / Boundary | Update image to null; verify 200 and image is null in response                                        | High     |
| S5          | R5                    | Null bio persists across GET                       | State                 | Set bio to null (via empty string or explicit null), then GET /api/user; verify bio is still null     | High     |
| S6          | R5                    | Null image persists across GET                     | State                 | Set image to null (via empty string or explicit null), then GET /api/user; verify image is still null | High     |
| S7          | R6                    | Email empty-string rejection                       | Negative              | Update email to empty string; verify 422                                                              | High     |
| S8          | R7                    | Username empty-string rejection                    | Negative              | Update username to empty string; verify 422                                                           | High     |
| S9          | R8                    | Email null rejection                               | Negative              | Update email to null; verify 422                                                                      | High     |
| S10         | R9                    | Username null rejection                            | Negative              | Update username to null; verify 422                                                                   | High     |
| S11         | R1, R2                | Full bio lifecycle: value → empty → null → value   | Scenario              | Set bio to a value, then empty string (normalizes), then null, then restore — verify each step        | Medium   |
| S12         | R3, R4                | Full image lifecycle: value → empty → null → value | Scenario              | Set image to a URL, then empty string (normalizes), then null, then restore — verify each step        | Medium   |
| S13         | R1                    | Bio whitespace-only not normalized                 | Boundary              | Set bio to `" "` (whitespace); verify it is NOT normalized to null                                    | Medium   |
| S14         | R3                    | Image whitespace-only not normalized               | Boundary              | Set image to `" "` (whitespace); verify it is NOT normalized to null                                  | Medium   |
| S15         | R5                    | Normalized null image persists across GET          | State                 | Set image to empty string, then GET; verify image is null                                             | High     |

## 7. Edge Case Matrix

| Requirement ID | Edge Category | Concrete Case                                                     | Covered by Test Case ID | Notes                                       |
| -------------- | ------------- | ----------------------------------------------------------------- | ----------------------- | ------------------------------------------- |
| R1             | Boundary      | Bio empty string normalizes to null                               | TC02                    | Core spec requirement                       |
| R1             | Boundary      | Bio whitespace-only string not normalized                         | TC15                    | Assumption: whitespace ≠ empty              |
| R2             | Null          | Bio set to explicit null accepted                                 | TC03                    | Core spec requirement                       |
| R3             | Boundary      | Image empty string normalizes to null                             | TC05                    | Core spec requirement                       |
| R3             | Boundary      | Image whitespace-only string not normalized                       | TC16                    | Assumption: whitespace ≠ empty              |
| R4             | Null          | Image set to explicit null accepted                               | TC06                    | Core spec requirement                       |
| R5             | State         | Null bio persists after GET                                       | TC07, TC08              | Two-step write-then-read                    |
| R5             | State         | Null image persists after GET                                     | TC09, TC10              | Two-step write-then-read                    |
| R6             | Empty         | Email empty string rejected                                       | TC11                    | Core spec requirement                       |
| R7             | Empty         | Username empty string rejected                                    | TC12                    | Core spec requirement                       |
| R8             | Null          | Email null rejected                                               | TC13                    | Core spec requirement                       |
| R9             | Null          | Username null rejected                                            | TC14                    | Core spec requirement                       |
| R1, R2         | Sequence      | Bio lifecycle: value → empty → null → value                       | TC17                    | Multi-step scenario from bruno spec pattern |
| R3, R4         | Sequence      | Image lifecycle: value → empty → null → value                     | TC18                    | Multi-step scenario from bruno spec pattern |
| R6–R9          | Combination   | Nullable field valid + non-nullable field invalid in same request | TC19                    | Tests partial vs full rejection             |

## 8. Detailed Test Cases

| Test Case ID | Title                                                  | Requirement Reference | Preconditions                                           | Test Data                                                                            | Steps                                                                                                                                                                                                                                                                               | Expected Result                                                                                        | Priority | Risk / Notes                                                           |
| ------------ | ------------------------------------------------------ | --------------------- | ------------------------------------------------------- | ------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ | -------- | ---------------------------------------------------------------------- |
| TC01         | Set bio to a valid non-empty string                    | R1                    | Authenticated user exists with null bio                 | bio = "Hello world"                                                                  | 1. PUT /api/user with `{"user":{"bio":"Hello world"}}`                                                                                                                                                                                                                              | 200; response `user.bio` = "Hello world"                                                               | High     | Baseline for subsequent normalization tests                            |
| TC02         | Set bio to empty string — normalizes to null           | R1                    | Authenticated user exists                               | bio = ""                                                                             | 1. PUT /api/user with `{"user":{"bio":""}}`                                                                                                                                                                                                                                         | 200; response `user.bio` is null                                                                       | High     | Core spec requirement                                                  |
| TC03         | Set bio to explicit null — accepted                    | R2                    | Authenticated user exists                               | bio = null                                                                           | 1. PUT /api/user with `{"user":{"bio":null}}`                                                                                                                                                                                                                                       | 200; response `user.bio` is null                                                                       | High     | Core spec requirement                                                  |
| TC04         | Set image to a valid URL                               | R3                    | Authenticated user exists with null image               | image = "https://example.com/photo.jpg"                                              | 1. PUT /api/user with `{"user":{"image":"https://example.com/photo.jpg"}}`                                                                                                                                                                                                          | 200; response `user.image` = "https://example.com/photo.jpg"                                           | High     | Baseline for subsequent normalization tests                            |
| TC05         | Set image to empty string — normalizes to null         | R3                    | Authenticated user exists with a non-null image         | image = ""                                                                           | 1. PUT /api/user with `{"user":{"image":""}}`                                                                                                                                                                                                                                       | 200; response `user.image` is null                                                                     | High     | Core spec requirement                                                  |
| TC06         | Set image to explicit null — accepted                  | R4                    | Authenticated user exists with a non-null image         | image = null                                                                         | 1. PUT /api/user with `{"user":{"image":null}}`                                                                                                                                                                                                                                     | 200; response `user.image` is null                                                                     | High     | Core spec requirement                                                  |
| TC07         | Null bio (via empty string) persists across GET        | R5                    | Bio has been set to empty string (normalized to null)   | N/A                                                                                  | 1. PUT /api/user with `{"user":{"bio":""}}` (normalizes to null) 2. GET /api/user                                                                                                                                                                                                   | GET returns 200; `user.bio` is null                                                                    | High     | Persistence verification                                               |
| TC08         | Null bio (via explicit null) persists across GET       | R5                    | Bio has been set to explicit null                       | N/A                                                                                  | 1. PUT /api/user with `{"user":{"bio":null}}` 2. GET /api/user                                                                                                                                                                                                                      | GET returns 200; `user.bio` is null                                                                    | High     | Persistence verification                                               |
| TC09         | Null image (via empty string) persists across GET      | R5                    | Image has been set to empty string (normalized to null) | N/A                                                                                  | 1. PUT /api/user with `{"user":{"image":""}}` (normalizes to null) 2. GET /api/user                                                                                                                                                                                                 | GET returns 200; `user.image` is null                                                                  | High     | Persistence verification                                               |
| TC10         | Null image (via explicit null) persists across GET     | R5                    | Image has been set to explicit null                     | N/A                                                                                  | 1. PUT /api/user with `{"user":{"image":null}}` 2. GET /api/user                                                                                                                                                                                                                    | GET returns 200; `user.image` is null                                                                  | High     | Persistence verification                                               |
| TC11         | Email empty string — rejected with 422                 | R6                    | Authenticated user exists                               | email = ""                                                                           | 1. PUT /api/user with `{"user":{"email":""}}`                                                                                                                                                                                                                                       | 422; response contains email-related validation error                                                  | High     | Core spec requirement                                                  |
| TC12         | Username empty string — rejected with 422              | R7                    | Authenticated user exists                               | username = ""                                                                        | 1. PUT /api/user with `{"user":{"username":""}}`                                                                                                                                                                                                                                    | 422; response contains username-related validation error                                               | High     | Core spec requirement                                                  |
| TC13         | Email null — rejected with 422                         | R8                    | Authenticated user exists                               | email = null                                                                         | 1. PUT /api/user with `{"user":{"email":null}}`                                                                                                                                                                                                                                     | 422; response contains email-related validation error                                                  | High     | Core spec requirement                                                  |
| TC14         | Username null — rejected with 422                      | R9                    | Authenticated user exists                               | username = null                                                                      | 1. PUT /api/user with `{"user":{"username":null}}`                                                                                                                                                                                                                                  | 422; response contains username-related validation error                                               | High     | Core spec requirement                                                  |
| TC15         | Bio whitespace-only — not normalized to null           | R1                    | Authenticated user exists                               | bio = " " (single space)                                                             | 1. PUT /api/user with `{"user":{"bio":" "}}`                                                                                                                                                                                                                                        | 200; response `user.bio` = " " (preserved, not normalized)                                             | Medium   | Boundary edge: spec only normalizes empty string                       |
| TC16         | Image whitespace-only — not normalized to null         | R3                    | Authenticated user exists                               | image = " " (single space)                                                           | 1. PUT /api/user with `{"user":{"image":" "}}`                                                                                                                                                                                                                                      | 200; response `user.image` = " " (preserved, not normalized)                                           | Medium   | Boundary edge: spec only normalizes empty string                       |
| TC17         | Full bio lifecycle: set → normalize → null → restore   | R1, R2, R5            | Authenticated user exists                               | bio values: "Test bio", "", null, "Restored bio"                                     | 1. PUT with bio="Test bio" → assert bio="Test bio" 2. PUT with bio="" → assert bio=null 3. GET → assert bio=null 4. PUT with bio=null → assert bio=null 5. GET → assert bio=null 6. PUT with bio="Restored bio" → assert bio="Restored bio"                                         | Each step reflects the correct bio value; normalization and null persist across GETs                   | Medium   | End-to-end lifecycle scenario matching bruno spec pattern              |
| TC18         | Full image lifecycle: set → normalize → null → restore | R3, R4, R5            | Authenticated user exists                               | image values: "https://example.com/pic.jpg", "", null, "https://example.com/new.jpg" | 1. PUT with image="https://example.com/pic.jpg" → assert image=URL 2. PUT with image="" → assert image=null 3. GET → assert image=null 4. PUT with image=null → assert image=null 5. GET → assert image=null 6. PUT with image="https://example.com/new.jpg" → assert image=new URL | Each step reflects the correct image value; normalization and null persist across GETs                 | Medium   | End-to-end lifecycle scenario matching bruno spec pattern              |
| TC19         | Nullable valid + non-nullable invalid in same request  | R1, R6                | Authenticated user exists                               | bio="", email=""                                                                     | 1. PUT /api/user with `{"user":{"bio":"","email":""}}`                                                                                                                                                                                                                              | 422; request is rejected due to invalid email even though bio is valid; email validation error present | Medium   | Tests whether server validates all fields or fails fast on first error |

## 9. How to Run the Generated Test Codes

### 9.1 Prerequisites

- Target system: ASP.NET Core RealWorld implementation running and reachable
- .NET 10 SDK (or later) installed
- xUnit test runner available via `dotnet test`
- The RealWorld API server must be started before running tests

### 9.2 Test Code Location

- **Generated Test Code Path**: `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/`
- **Project Root Path**: `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/`
- **Test Entry File / Directory**: `SettingsNullFieldsTests.cs`
- **Related Configuration Files**: `RealWorld.Blackbox.Tests.csproj`

### 9.3 Environment Setup

1. **Working Directory**: `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/`
2. **Runtime Version**: .NET 10 SDK or later
3. **Dependency Install Command**: `dotnet restore`
4. **Build Command**: `dotnet build`
5. **Test Environment Variables**:
   - `REALWORLD_BASE_URL`: Base URL of the running RealWorld API (default: `http://localhost:5000`)
6. **Service Start Command**: Start the ASP.NET Core RealWorld server on the configured host/port before running tests
7. **Test Data / Seed Command**: No seed data required; tests register their own users with unique IDs

### 9.4 Run Commands

```bash
# Restore dependencies
dotnet restore

# Run all tests
dotnet test

# Run only SettingsNullFields tests
dotnet test --filter "FullyQualifiedName~SettingsNullFields"

# Run a specific test by name
dotnet test --filter "FullyQualifiedName~BioEmptyString_NormalizesToNull"

# Run with verbose output
dotnet test --filter "FullyQualifiedName~SettingsNullFields" --logger "console;verbosity=detailed"

# Run with custom base URL
REALWORLD_BASE_URL=http://localhost:5000 dotnet test --filter "FullyQualifiedName~SettingsNullFields"
```

### 9.5 Execution Notes

- Start the target service before running black-box tests. Tests will fail with connection errors if the server is unreachable.
- Each test class instance registers a fresh user with a unique ID to avoid state collisions between tests.
- Persistence tests (TC07–TC10) perform a write followed by a read in a single test method to ensure atomicity of the state verification.
- Lifecycle tests (TC17, TC18) are sequential multi-step tests within a single method — they verify state transitions step by step.
- Record the exact `REALWORLD_BASE_URL` value used for reproducibility.

## 10. Coverage Summary

### 10.1 Requirement Coverage Table

| Requirement ID | EP Covered? | BVA Covered? | Edge Case Covered? | Negative Case Covered? | State / Sequence Covered? | Covered by Test Cases              | Coverage Status | Notes                                                                             |
| -------------- | ----------- | ------------ | ------------------ | ---------------------- | ------------------------- | ---------------------------------- | --------------- | --------------------------------------------------------------------------------- |
| R1             | Yes         | Yes          | Yes                | N/A                    | Partial                   | TC01, TC02, TC15, TC17             | Full            | Whitespace edge case covered                                                      |
| R2             | Yes         | Yes          | Yes                | N/A                    | Partial                   | TC03, TC17                         | Full            | Explicit null acceptance                                                          |
| R3             | Yes         | Yes          | Yes                | N/A                    | Partial                   | TC04, TC05, TC16, TC18             | Full            | Whitespace edge case covered                                                      |
| R4             | Yes         | Yes          | Yes                | N/A                    | Partial                   | TC06, TC18                         | Full            | Explicit null acceptance                                                          |
| R5             | Yes         | N/A          | Yes                | N/A                    | Yes                       | TC07, TC08, TC09, TC10, TC17, TC18 | Full            | Both empty-string-normalized and explicit-null persistence tested for both fields |
| R6             | Yes         | Yes          | Yes                | Yes                    | N/A                       | TC11, TC19                         | Full            | Empty-string rejection on non-nullable field                                      |
| R7             | Yes         | Yes          | Yes                | Yes                    | N/A                       | TC12                               | Full            | Empty-string rejection on non-nullable field                                      |
| R8             | Yes         | Yes          | Yes                | Yes                    | N/A                       | TC13                               | Full            | Null rejection on non-nullable field                                              |
| R9             | Yes         | Yes          | Yes                | Yes                    | N/A                       | TC14                               | Full            | Null rejection on non-nullable field                                              |

### 10.2 EP / BVA to Test Case Mapping

| Analysis Item ID | Type | Requirement ID | Description                            | Mapped Test Case ID(s) | Covered? | Notes            |
| ---------------- | ---- | -------------- | -------------------------------------- | ---------------------- | -------- | ---------------- |
| EP1              | EP   | R1             | Valid non-empty bio                    | TC01                   | Yes      |                  |
| EP2              | EP   | R1             | Empty-string bio (normalized)          | TC02                   | Yes      |                  |
| EP3              | EP   | R2             | Null bio                               | TC03                   | Yes      |                  |
| EP4              | EP   | R3             | Valid non-empty image                  | TC04                   | Yes      |                  |
| EP5              | EP   | R3             | Empty-string image (normalized)        | TC05                   | Yes      |                  |
| EP6              | EP   | R4             | Null image                             | TC06                   | Yes      |                  |
| EP7              | EP   | R5             | Persistence of null values             | TC07–TC10              | Yes      |                  |
| EP8              | EP   | R6             | Invalid: empty-string email            | TC11                   | Yes      |                  |
| EP9              | EP   | R7             | Invalid: empty-string username         | TC12                   | Yes      |                  |
| EP10             | EP   | R8             | Invalid: null email                    | TC13                   | Yes      |                  |
| EP11             | EP   | R9             | Invalid: null username                 | TC14                   | Yes      |                  |
| EP12             | EP   | R1             | Whitespace-only bio                    | TC15                   | Yes      | Assumption-based |
| EP13             | EP   | R3             | Whitespace-only image                  | TC16                   | Yes      | Assumption-based |
| B1               | BVA  | R1, R2         | Bio empty ↔ null boundary              | TC02, TC03, TC15       | Yes      |                  |
| B2               | BVA  | R3, R4         | Image empty ↔ null boundary            | TC05, TC06, TC16       | Yes      |                  |
| B3               | BVA  | R6, R8         | Email null/empty rejection boundary    | TC11, TC13             | Yes      |                  |
| B4               | BVA  | R7, R9         | Username null/empty rejection boundary | TC12, TC14             | Yes      |                  |

### 10.3 Coverage Metrics

| Metric                 | Formula                                              | Value        |
| ---------------------- | ---------------------------------------------------- | ------------ |
| Requirement Coverage   | covered_requirements / total_requirements            | 9/9 = 100%   |
| EP Coverage            | covered_partitions / total_partitions                | 13/13 = 100% |
| BVA Coverage           | covered_boundaries / total_boundaries                | 4/4 = 100%   |
| Edge Case Coverage     | covered_edge_categories / applicable_edge_categories | 8/8 = 100%   |
| Negative Case Coverage | negative_cases_present / applicable_requirements     | 4/4 = 100%   |
| Duplicate Case Rate    | duplicate_cases / total_cases                        | 0/19 = 0%    |
| Executability Score    | 1-5                                                  | 5            |

### 10.4 Coverage Notes

- Strongest covered area: Nullable field semantics (R1–R5) — covers valid, empty-string, null, whitespace-only, and persistence for both bio and image fields.
- Weakest covered area: None — all requirements have full coverage with normal, boundary, and negative cases.
- Over-covered or duplicated areas: TC07/TC08 and TC09/TC10 test similar persistence but via different paths (empty-string normalization vs explicit null). This redundancy is intentional to verify both paths independently.
- Under-covered areas: Update with completely omitted fields (not null, but absent from JSON body) — the spec does not address this, so it is deferred. Also, concurrent update scenarios (two sessions updating the same profile) are out of scope.

## 11. Ambiguities / Missing Information / Assumptions

### 11.1 Ambiguous Requirements

- Item 1: The spec does not clarify whether whitespace-only strings (e.g., `" "`) should be normalized to null like empty strings, or treated as non-empty content. The tests assume whitespace-only strings are NOT normalized (only `""` is), but implementations may vary.
- Item 2: The spec does not specify the exact error response format for 422 rejections on non-nullable fields — whether it uses `{ "errors": { "email": ["can't be blank"] } }` or a different structure. The tests verify the 422 status and the presence of a field-specific error key.

### 11.2 Missing Information

- Missing validation rule: Whether email format is validated on update (beyond non-empty/non-null). The spec only covers empty and null rejection.
- Missing boundary definition: Maximum lengths for bio, image URL, username, and email are not specified.
- Missing error behavior: Behavior when both a nullable field (valid) and a non-nullable field (invalid) are in the same request body — does the server reject the entire request or only the invalid field?
- Missing state transition rule: Whether an update with an omitted field (not null, but absent from JSON) preserves the existing value or resets it. The spec only covers explicit null and empty string.

### 11.3 Assumptions

- Assumption 1: Whitespace-only strings (e.g., `" "`) on nullable fields are NOT normalized to null — only truly empty strings `""` are. This is inferred from the spec's use of "empty string" without mentioning whitespace.
- Assumption 2: When a non-nullable field is set to null or empty string, the entire update request is rejected (422), and no partial update is applied to other fields in the same request.
- Assumption 3: The `user` wrapper key in the JSON body is required for both PUT and GET requests, consistent with the RealWorld API specification.
- Assumption 4: Each test registers a fresh user to ensure clean state; there is no shared mutable state between test methods.
