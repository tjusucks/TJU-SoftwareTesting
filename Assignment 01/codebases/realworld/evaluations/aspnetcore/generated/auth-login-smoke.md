# Black-Box Testing Run Report Template

## 1. Run Metadata

| Field                                    | Value                                                                                                 |
| ---------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| Project Name                             | RealWorld (Conduit)                                                                                   |
| Feature Name                             | Auth Login Smoke                                                                                      |
| Run ID                                   | BB-ASPNETCORE-AUTH-LOGIN-SMOKE-001                                                                    |
| Date                                     | 2026-04-19                                                                                            |
| Author / Operator                        | Claude (blackbox-testing skill)                                                                       |
| Skill / Tool Name                        | `blackbox-testing`                                                                                    |
| Model / Agent Version                    | glm-5.1                                                                                               |
| Prompt Version                           | 1.0                                                                                                   |
| Input Type                               | Requirement / API Spec (Mixed)                                                                        |
| Input Source Path / Link                 | `Assignment 01/codebases/realworld/specification/features/auth-login-smoke.md` + upstream bruno specs |
| Target System / Implementation           | ASP.NET Core RealWorld implementation                                                                 |
| Target Module / Endpoint / Feature Scope | `POST /api/users` (register), `POST /api/users/login` (login)                                         |
| Execution Scope                          | Design + Automation                                                                                   |
| Notes                                    | Smoke/calibration slice only; not intended as a comprehensive auth benchmark                          |

## 2. Input Summary

### 2.1 Input Overview

- **Project / System Under Test**: RealWorld (Conduit) — a Medium.com-clone backend API
- **Feature Under Test**: User registration and login authentication flow
- **Actors**: Unauthenticated user (registering), registered user (logging in)
- **Preconditions**: The RealWorld API server is running and reachable; no prior user state required for registration
- **Business Rules**:
  - A user must register before logging in
  - Registration requires username, email, and password; all must be non-empty
  - Login requires email and password; email must be non-empty, password must match
  - Duplicate username or email is rejected on registration
  - Nullable profile fields (bio, image) default to null
- **Input Constraints**:
  - Registration: username (non-empty string), email (non-empty, valid format), password (non-empty string)
  - Login: email (non-empty string), password (non-empty string)
- **Error Conditions**:
  - 422 for empty/blank required fields with field-specific validation messages
  - 401 for wrong password or invalid credentials
  - 409 for duplicate username or email on registration

### 2.2 Requirement Items

| Requirement ID | Requirement Description                                                                                                                                                             | Priority | Notes                             |
| -------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | --------------------------------- |
| R1             | Registering a new user with username, email, and password succeeds (201); response contains the created username, email, bio=null, image=null, and a non-empty authentication token | High     | Core happy path; smoke validation |
| R2             | Logging in with the correct email and password succeeds (200); response contains the correct username, email, bio=null, image=null, and a non-empty authentication token            | High     | Core happy path; depends on R1    |
| R3             | Login with an empty email is rejected with status 422; the error response includes an email validation message indicating the field cannot be blank                                 | High     | Validation error path             |
| R4             | Login with the wrong password is rejected with status 401; the error response indicates invalid credentials                                                                         | High     | Authentication error path         |

### 2.3 Assumptions About Input

- Assumption 1: The API uses JSON request/response bodies with a top-level `user` key wrapping user fields, consistent with the RealWorld API specification and bruno test files.
- Assumption 2: The error response format for validation errors is `{ "errors": { "field": ["message"] } }` and for credential errors is `{ "errors": { "credentials": ["invalid"] } }`, as shown in the upstream bruno specs.
- Assumption 3: Email format validation on registration follows standard patterns; the smoke scope does not require exhaustive format testing beyond the explicitly stated acceptance criteria.
- Assumption 4: The target server is accessible at a configurable base URL (default `http://localhost:3000`), matching the bruno environment configuration.

## 3. Test Design Strategy

### 3.1 Applied Black-Box Techniques

| Technique                   | Applied? | Where Used                                                                             | Notes                                         |
| --------------------------- | -------- | -------------------------------------------------------------------------------------- | --------------------------------------------- |
| Equivalence Partitioning    | Yes      | Registration and login input fields                                                    | Valid/invalid partitions for each input field |
| Boundary Value Analysis     | Yes      | Empty string vs non-empty string boundaries for email and password                     | Boundary between valid and invalid partitions |
| Decision Table Testing      | Yes      | Login validation: email empty × password empty × password wrong                        | Combinations of missing/invalid inputs        |
| State Transition Testing    | No       | Feature is stateless per request; sequencing only matters for register-then-login flow | Covered as scenario test instead              |
| Error Guessing              | Yes      | Non-existent email, whitespace-only email, missing fields                              | Common API error patterns                     |
| Scenario / Use-Case Testing | Yes      | Register-then-login end-to-end flow                                                    | Primary acceptance criterion                  |

### 3.2 Test Dimension Summary

- Valid input classes: valid registration (non-empty username, email, password); valid login (correct email + password)
- Invalid input classes: empty email on login; empty password on login; wrong password; non-existent email
- Boundary values: empty string (just below minimum valid length) for email and password
- Empty / null / missing cases: empty-string email; empty-string password; omitted email field; omitted password field
- Format-related cases: whitespace-only email (boundary between empty and valid)
- Permission / role cases: N/A — no role differentiation in this feature scope
- State / sequencing cases: login without prior registration (non-existent user); register-then-login flow
- Combination cases: both email and password empty simultaneously; email empty + wrong password

### 3.3 Edge-Case Design Notes

- Empty-string boundary: The spec explicitly calls out empty email (R3). The boundary between empty and non-empty is the most critical edge for this smoke scope.
- Missing field vs empty field: Sending a JSON body with the field omitted vs set to `""` may produce different server behavior; both are tested.
- Whitespace-only email: A string like `"   "` is non-empty but semantically blank — this is a boundary-adjacent case.
- Non-existent email login: Not explicitly stated in the spec but implied by R4 (wrong password implies the user exists but password doesn't match; non-existent user is the complement case).

## 4. Equivalence Partitioning Analysis

| EP ID | Requirement ID | Input / Rule            | Partition Type | Description                                                  | Expected Outcome                        | Covered by Test Case ID |
| ----- | -------------- | ----------------------- | -------------- | ------------------------------------------------------------ | --------------------------------------- | ----------------------- |
| EP1   | R1             | Registration inputs     | Valid          | All fields non-empty, email in valid format, username unique | 201 with user object and token          | TC01, TC02              |
| EP2   | R1             | Registration username   | Invalid        | Empty or blank username                                      | 422 with username validation error      | TC10                    |
| EP3   | R1             | Registration email      | Invalid        | Empty or blank email                                         | 422 with email validation error         | TC11                    |
| EP4   | R1             | Registration password   | Invalid        | Empty or blank password                                      | 422 with password validation error      | TC12                    |
| EP5   | R1             | Registration uniqueness | Invalid        | Duplicate username or email                                  | 409 with "has already been taken" error | TC13, TC14              |
| EP6   | R2             | Login email + password  | Valid          | Correct email and password for a registered user             | 200 with user object and token          | TC03, TC04              |
| EP7   | R3             | Login email             | Invalid        | Empty string email                                           | 422 with email "can't be blank"         | TC05                    |
| EP8   | R3             | Login email             | Invalid        | Whitespace-only email (e.g., `"   "`)                        | 422 with email validation error         | TC06                    |
| EP9   | R3             | Login password          | Invalid        | Empty string password                                        | 422 with password "can't be blank"      | TC07                    |
| EP10  | R4             | Login password          | Invalid        | Wrong password for existing email                            | 401 with credentials "invalid"          | TC08                    |
| EP11  | R4             | Login email             | Invalid        | Non-existent email address                                   | 401 (credentials not found)             | TC09                    |

### 4.1 EP Coverage Notes

- Covered partitions: All valid and invalid partitions for the in-scope requirements (R1–R4).
- Missing partitions: Registration with invalid email format (e.g., `"not-an-email"`) — not required by the smoke spec but recommended for a fuller auth test suite.
- Partially covered partitions: EP8 (whitespace-only email) — expected outcome inferred from the empty-email behavior; the spec does not explicitly state the expected response for whitespace-only input.

## 5. Boundary Value Analysis

| BVA ID | Requirement ID | Boundary Item       | Boundary Definition                              | Test Values                             | Expected Outcome                                            | Covered by Test Case ID |
| ------ | -------------- | ------------------- | ------------------------------------------------ | --------------------------------------- | ----------------------------------------------------------- | ----------------------- |
| B1     | R3             | Login email         | Min valid vs just-below-min (empty)              | `""` (empty), `"a@b.c"` (minimal valid) | `""` → 422; valid email → 200 or 401 depending on password  | TC05, TC03              |
| B2     | R3             | Login password      | Min valid vs just-below-min (empty)              | `""` (empty), `"x"` (minimal non-empty) | `""` → 422; non-empty → 200 or 401 depending on correctness | TC07, TC08              |
| B3     | R1             | Registration fields | Min valid vs empty for username, email, password | `""` vs non-empty for each field        | Empty → 422 per field                                       | TC10, TC11, TC12        |

### 5.1 BVA Coverage Notes

- Explicit boundaries tested: Empty-string boundary for login email (R3), empty-string boundary for login password (implied by R3 dimension), empty-string boundaries for registration fields.
- Missing boundaries: Maximum length boundaries for username, email, and password — the spec does not define maximum lengths, so these cannot be tested against a known expected outcome.
- Ambiguous boundary definitions from requirements: The spec does not define what constitutes a "valid" email format beyond being non-empty. Whether whitespace-only strings are treated as empty is unspecified.

## 6. Test Scenarios

| Scenario ID | Requirement Reference | Scenario Title                           | Scenario Type | Description                                                                                                            | Priority |
| ----------- | --------------------- | ---------------------------------------- | ------------- | ---------------------------------------------------------------------------------------------------------------------- | -------- |
| S1          | R1                    | Successful registration                  | Happy Path    | Register a new user with valid, unique credentials and verify all response fields                                      | High     |
| S2          | R1                    | Registration response field correctness  | Happy Path    | Verify that bio and image are null and token is non-empty in registration response                                     | High     |
| S3          | R2                    | Successful login after registration      | Happy Path    | Login with the credentials of a newly registered user and verify response                                              | High     |
| S4          | R2                    | Login response field correctness         | Happy Path    | Verify that login response contains correct username, email, null bio/image, and non-empty token                       | High     |
| S5          | R3                    | Login with empty email                   | Negative      | Submit login with empty email string and verify 422 with email validation error                                        | High     |
| S6          | R3                    | Login with whitespace-only email         | Boundary      | Submit login with whitespace-only email and verify validation rejection                                                | Medium   |
| S7          | R3                    | Login with empty password                | Negative      | Submit login with empty password string and verify 422 with password validation error                                  | Medium   |
| S8          | R3                    | Login with both email and password empty | Combination   | Submit login with both fields empty and verify 422 with validation errors                                              | Medium   |
| S9          | R4                    | Login with wrong password                | Negative      | Submit login with wrong password for a registered email and verify 401                                                 | High     |
| S10         | R4                    | Login with non-existent email            | Negative      | Submit login with an email that has no registered account and verify 401                                               | Medium   |
| S11         | R1+R2                 | End-to-end register then login           | Scenario      | Register a new user, then immediately log in with the same credentials — validates the full smoke acceptance criterion | High     |
| S12         | R3                    | Login with missing email field           | Edge          | Submit login JSON with the email field omitted entirely (not empty string)                                             | Medium   |
| S13         | R1                    | Registration with duplicate username     | Negative      | Attempt to register a second user with a username already taken                                                        | Medium   |
| S14         | R1                    | Registration with duplicate email        | Negative      | Attempt to register a second user with an email already taken                                                          | Medium   |

## 7. Edge Case Matrix

| Requirement ID | Edge Category | Concrete Case                                             | Covered by Test Case ID | Notes                                                        |
| -------------- | ------------- | --------------------------------------------------------- | ----------------------- | ------------------------------------------------------------ |
| R1             | Empty         | Empty-string username on registration                     | TC10                    |                                                              |
| R1             | Empty         | Empty-string email on registration                        | TC11                    |                                                              |
| R1             | Empty         | Empty-string password on registration                     | TC12                    |                                                              |
| R1             | Duplicate     | Duplicate username on registration                        | TC13                    |                                                              |
| R1             | Duplicate     | Duplicate email on registration                           | TC14                    |                                                              |
| R1             | Null          | Null bio/image default on new user                        | TC02                    | Covered by happy path assertions                             |
| R1             | Missing       | Omitted registration fields                               | Deferred                | Lower priority for smoke scope                               |
| R2             | Boundary      | Login immediately after registration                      | TC03, TC15              | State-sequencing edge: token usable right after registration |
| R3             | Empty         | Empty-string email on login                               | TC05                    | Explicitly required by spec                                  |
| R3             | Boundary      | Whitespace-only email on login                            | TC06                    | Near-boundary of empty                                       |
| R3             | Missing       | Omitted email field in login JSON body                    | TC16                    | Structural omission vs empty value                           |
| R3             | Empty         | Empty-string password on login                            | TC07                    | Symmetric with empty email                                   |
| R3             | Combination   | Both email and password empty                             | TC17                    | Multi-field invalid combination                              |
| R4             | Wrong Type    | Wrong password for existing user                          | TC08                    | Explicitly required by spec                                  |
| R4             | State         | Non-existent email (no registered account)                | TC09                    | Complement of "wrong password"                               |
| R4             | Stale         | Login with a previously valid token after password change | Deferred                | Out of scope for smoke                                       |

## 8. Detailed Test Cases

| Test Case ID | Title                                           | Requirement Reference | Preconditions                                                   | Test Data                                                                              | Steps                                                                                                                             | Expected Result                                                                          | Priority | Risk / Notes                                                                      |
| ------------ | ----------------------------------------------- | --------------------- | --------------------------------------------------------------- | -------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------- | -------- | --------------------------------------------------------------------------------- |
| TC01         | Register a new user successfully                | R1                    | API server is running; no user with the test credentials exists | username=`auth_smoke_{uid}`, email=`auth_smoke_{uid}@test.com`, password=`password123` | 1. POST `/api/users` with `{"user":{"username":"auth_smoke_{uid}","email":"auth_smoke_{uid}@test.com","password":"password123"}}` | Status 201; response body contains `user` object with matching `username` and `email`    | High     | Core smoke test                                                                   |
| TC02         | Registration response field correctness         | R1                    | Same as TC01 (combined)                                         | Same as TC01                                                                           | 1. Register user (as TC01)                                                                                                        | Response `user.bio` is null; `user.image` is null; `user.token` is a non-empty string    | High     | Validates nullable defaults and token presence                                    |
| TC03         | Login with valid credentials after registration | R2                    | A user has been successfully registered (TC01)                  | email and password from TC01                                                           | 1. POST `/api/users/login` with `{"user":{"email":"{email}","password":"{password}"}}`                                            | Status 200; response body contains `user` object with correct `username` and `email`     | High     | Core smoke test                                                                   |
| TC04         | Login response field correctness                | R2                    | A user has been registered                                      | Same as TC03                                                                           | 1. Login (as TC03)                                                                                                                | Response `user.bio` is null; `user.image` is null; `user.token` is a non-empty string    | High     | Validates nullable defaults and token presence on login                           |
| TC05         | Login with empty email                          | R3                    | API server is running                                           | email=`""`, password=`password123`                                                     | 1. POST `/api/users/login` with `{"user":{"email":"","password":"password123"}}`                                                  | Status 422; `errors.email[0]` equals `"can't be blank"`                                  | High     | Explicit acceptance criterion                                                     |
| TC06         | Login with whitespace-only email                | R3                    | API server is running                                           | email=`"   "`, password=`password123`                                                  | 1. POST `/api/users/login` with `{"user":{"email":"   ","password":"password123"}}`                                               | Status 422; response contains email-related validation error                             | Medium   | Boundary-adjacent; spec doesn't explicitly state this outcome                     |
| TC07         | Login with empty password                       | R3                    | A user has been registered                                      | email=`{registered_email}`, password=`""`                                              | 1. POST `/api/users/login` with `{"user":{"email":"{email}","password":""}}`                                                      | Status 422; `errors.password[0]` equals `"can't be blank"`                               | Medium   | Symmetric with empty email; implied by validation rules                           |
| TC08         | Login with wrong password                       | R4                    | A user has been registered                                      | email=`{registered_email}`, password=`wrongpassword`                                   | 1. POST `/api/users/login` with `{"user":{"email":"{email}","password":"wrongpassword"}}`                                         | Status 401; `errors.credentials[0]` equals `"invalid"`                                   | High     | Explicit acceptance criterion                                                     |
| TC09         | Login with non-existent email                   | R4                    | API server is running; no account with the test email exists    | email=`nonexistent_{uid}@test.com`, password=`password123`                             | 1. POST `/api/users/login` with `{"user":{"email":"nonexistent_{uid}@test.com","password":"password123"}}`                        | Status 401; response contains a credential-related error                                 | Medium   | Complement of R4; spec doesn't differentiate "wrong password" vs "user not found" |
| TC10         | Registration with empty username                | R1                    | API server is running                                           | username=`""`, email=`blanku_{uid}@test.com`, password=`password123`                   | 1. POST `/api/users` with `{"user":{"username":"","email":"blanku_{uid}@test.com","password":"password123"}}`                     | Status 422; `errors.username[0]` equals `"can't be blank"`                               | Medium   | Invalid partition for registration                                                |
| TC11         | Registration with empty email                   | R1                    | API server is running                                           | username=`blanke_{uid}`, email=`""`, password=`password123`                            | 1. POST `/api/users` with `{"user":{"username":"blanke_{uid}","email":"","password":"password123"}}`                              | Status 422; `errors.email[0]` equals `"can't be blank"`                                  | Medium   | Invalid partition for registration                                                |
| TC12         | Registration with empty password                | R1                    | API server is running                                           | username=`blankp_{uid}`, email=`blankp_{uid}@test.com`, password=`""`                  | 1. POST `/api/users` with `{"user":{"username":"blankp_{uid}","email":"blankp_{uid}@test.com","password":""}}`                    | Status 422; `errors.password[0]` equals `"can't be blank"`                               | Medium   | Invalid partition for registration                                                |
| TC13         | Registration with duplicate username            | R1                    | A user has already been registered with the target username     | username=`{existing_username}`, email=`dup2_{uid}@test.com`, password=`password123`    | 1. POST `/api/users` with the duplicate username                                                                                  | Status 409; `errors.username[0]` equals `"has already been taken"`                       | Medium   | Uniqueness constraint edge case                                                   |
| TC14         | Registration with duplicate email               | R1                    | A user has already been registered with the target email        | username=`dup2_{uid}`, email=`{existing_email}`, password=`password123`                | 1. POST `/api/users` with the duplicate email                                                                                     | Status 409; `errors.email[0]` equals `"has already been taken"`                          | Medium   | Uniqueness constraint edge case                                                   |
| TC15         | End-to-end register then login                  | R1, R2                | API server is running                                           | Unique username, email, password                                                       | 1. Register a new user 2. Login with the same credentials                                                                         | Both requests succeed (201 then 200); login returns matching user data and a valid token | High     | Primary acceptance criterion (AC1)                                                |
| TC16         | Login with missing email field in JSON          | R3                    | API server is running                                           | `{"user":{"password":"password123"}}` (no email key)                                   | 1. POST `/api/users/login` with JSON body omitting the email field                                                                | Status 422; response contains an email-related validation error                          | Medium   | Structural omission vs empty string                                               |
| TC17         | Login with both email and password empty        | R3, R4                | API server is running                                           | email=`""`, password=`""`                                                              | 1. POST `/api/users/login` with `{"user":{"email":"","password":""}}`                                                             | Status 422; response contains validation errors for both email and password              | Medium   | Multi-field invalid combination                                                   |

## 9. How to Run the Generated Test Codes

### 9.1 Prerequisites

- Target system: ASP.NET Core RealWorld implementation running and reachable
- .NET 8 SDK (or later) installed
- xUnit test runner available via `dotnet test`
- The RealWorld API server must be started before running tests (see Environment Setup)

### 9.2 Test Code Location

- **Generated Test Code Path**: `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/`
- **Project Root Path**: `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/`
- **Test Entry File / Directory**: `AuthLoginSmokeTests.cs`
- **Related Configuration Files**: `RealWorld.Blackbox.Tests.csproj`

### 9.3 Environment Setup

1. **Working Directory**: `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/`
2. **Runtime Version**: .NET 8 SDK or later
3. **Dependency Install Command**: `dotnet restore`
4. **Build Command**: `dotnet build`
5. **Test Environment Variables**:
   - `REALWORLD_BASE_URL`: Base URL of the running RealWorld API (default: `http://localhost:3000`)
6. **Service Start Command**: Start the ASP.NET Core RealWorld server on the configured host/port before running tests
7. **Test Data / Seed Command**: No seed data required; tests create their own users with unique IDs

### 9.4 Run Commands

```bash
# Restore dependencies
dotnet restore

# Run all tests
dotnet test

# Run all tests with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run a specific test by name
dotnet test --filter "FullyQualifiedName~RegisterNewUserSuccessfully"

# Run with no caching (force re-execution)
dotnet test --no-build && dotnet test

# Run with custom base URL
REALWORLD_BASE_URL=http://localhost:5000 dotnet test
```

### 9.5 Execution Notes

- Start the target service before running black-box tests. Tests will fail with connection errors if the server is unreachable.
- Each test generates a unique user ID (GUID-based) to avoid collisions with existing data. Tests are idempotent and can be re-run without manual cleanup.
- TC13 and TC14 (duplicate registration) register a user first, then attempt a second registration with the same username/email. These tests depend on the first registration succeeding.
- If the test framework caches results, force re-execution with `dotnet test --no-build` after a clean build.
- Record the exact `REALWORLD_BASE_URL` value used for reproducibility.

## 10. Coverage Summary

### 10.1 Requirement Coverage Table

| Requirement ID | EP Covered? | BVA Covered? | Edge Case Covered? | Negative Case Covered? | State / Sequence Covered? | Covered by Test Cases                    | Coverage Status | Notes                                                                                                                                   |
| -------------- | ----------- | ------------ | ------------------ | ---------------------- | ------------------------- | ---------------------------------------- | --------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| R1             | Yes         | Yes          | Yes                | Yes                    | Partial                   | TC01, TC02, TC10, TC11, TC12, TC13, TC14 | Full            | All partitions and boundaries covered; missing-field registration deferred as lower priority                                            |
| R2             | Yes         | Yes          | Yes                | N/A                    | Yes                       | TC03, TC04, TC15                         | Full            | No negative case applicable to R2 itself (R2 is the valid-login requirement)                                                            |
| R3             | Yes         | Yes          | Yes                | Yes                    | N/A                       | TC05, TC06, TC07, TC16, TC17             | Full            | Empty, whitespace-only, missing-field, and combination cases all covered                                                                |
| R4             | Yes         | N/A          | Yes                | Yes                    | Yes                       | TC08, TC09                               | Full            | BVA not directly applicable (no numeric/string-length boundary defined for "wrongness"); non-existent email covers the complement state |

### 10.2 EP / BVA to Test Case Mapping

| Analysis Item ID | Type | Requirement ID | Description                               | Mapped Test Case ID(s) | Covered? | Notes              |
| ---------------- | ---- | -------------- | ----------------------------------------- | ---------------------- | -------- | ------------------ |
| EP1              | EP   | R1             | Valid registration                        | TC01, TC02             | Yes      |                    |
| EP2              | EP   | R1             | Invalid: empty username                   | TC10                   | Yes      |                    |
| EP3              | EP   | R1             | Invalid: empty email                      | TC11                   | Yes      |                    |
| EP4              | EP   | R1             | Invalid: empty password                   | TC12                   | Yes      |                    |
| EP5              | EP   | R1             | Invalid: duplicate                        | TC13, TC14             | Yes      |                    |
| EP6              | EP   | R2             | Valid login                               | TC03, TC04             | Yes      |                    |
| EP7              | EP   | R3             | Invalid: empty email on login             | TC05                   | Yes      |                    |
| EP8              | EP   | R3             | Invalid: whitespace-only email            | TC06                   | Yes      |                    |
| EP9              | EP   | R3             | Invalid: empty password on login          | TC07                   | Yes      |                    |
| EP10             | EP   | R4             | Invalid: wrong password                   | TC08                   | Yes      |                    |
| EP11             | EP   | R4             | Invalid: non-existent email               | TC09                   | Yes      |                    |
| B1               | BVA  | R3             | Login email empty-string boundary         | TC05, TC03             | Yes      | Empty vs non-empty |
| B2               | BVA  | R3             | Login password empty-string boundary      | TC07, TC08             | Yes      | Empty vs non-empty |
| B3               | BVA  | R1             | Registration fields empty-string boundary | TC10, TC11, TC12       | Yes      |                    |

### 10.3 Coverage Metrics

| Metric                 | Formula                                              | Value        |
| ---------------------- | ---------------------------------------------------- | ------------ |
| Requirement Coverage   | covered_requirements / total_requirements            | 4/4 = 100%   |
| EP Coverage            | covered_partitions / total_partitions                | 11/11 = 100% |
| BVA Coverage           | covered_boundaries / total_boundaries                | 3/3 = 100%   |
| Edge Case Coverage     | covered_edge_categories / applicable_edge_categories | 10/11 = 91%  |
| Negative Case Coverage | negative_cases_present / applicable_requirements     | 3/3 = 100%   |
| Duplicate Case Rate    | duplicate_cases / total_cases                        | 0/17 = 0%    |
| Executability Score    | 1-5                                                  | 4            |

### 10.4 Coverage Notes

- Strongest covered area: R3 (login validation) — covers empty, whitespace-only, missing-field, and multi-field combinations.
- Weakest covered area: R4 (wrong password) — only two cases (wrong password, non-existent email); additional edge cases like expired tokens or password-change scenarios are deferred.
- Over-covered or duplicated areas: None identified.
- Under-covered areas: Registration with malformed email format, maximum-length inputs, and special characters in fields — these are out of scope for the smoke specification but recommended for a comprehensive auth test suite.

## 11. Ambiguities / Missing Information / Assumptions

### 11.1 Ambiguous Requirements

- Item 1: The spec says login with empty email returns 422, but does not specify whether whitespace-only email (`"   "`) should be treated as empty or as a valid (but nonexistent) email. The tests assume it is rejected as 422, but this is an assumption.
- Item 2: The spec says wrong password returns 401 with "invalid credentials," but does not specify whether logging in with a non-existent email should return the same 401 or a different response. The tests assume 401 with a similar error format.
- Item 3: The spec does not specify the exact JSON structure for login request bodies — specifically whether the `user` wrapper key is required. This is inferred from the bruno specs.

### 11.2 Missing Information

- Missing validation rule: Whether email format is validated on registration beyond non-empty (e.g., must contain `@`).
- Missing boundary definition: Maximum lengths for username, email, and password are not specified.
- Missing error behavior: Behavior when the request body is entirely malformed (not valid JSON) or when the `Content-Type` header is incorrect.
- Missing state transition rule: Whether a registered user's token is immediately usable for authenticated endpoints, or whether login is required first.

### 11.3 Assumptions

- Assumption 1: Login and registration requests use the `{"user": {...}}` JSON wrapper consistent with the RealWorld API specification and bruno test files.
- Assumption 2: Error responses follow the format `{"errors": {"field": ["message"]}}` for validation errors and `{"errors": {"credentials": ["invalid"]}}` for authentication errors, as shown in the upstream bruno specs.
- Assumption 3: The `REALWORLD_BASE_URL` environment variable (defaulting to `http://localhost:3000`) is sufficient to configure the test target; no additional authentication or service mesh configuration is needed.
