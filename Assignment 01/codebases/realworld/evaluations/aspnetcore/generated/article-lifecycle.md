# Black-Box Testing Run Report Template

## 1. Run Metadata

| Field                                    | Value                                                                                                       |
| ---------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| Project Name                             | RealWorld (Conduit)                                                                                         |
| Feature Name                             | Article Lifecycle                                                                                           |
| Run ID                                   | BB-ASPNETCORE-ARTICLE-LIFECYCLE-001                                                                         |
| Date                                     | 2026-04-19                                                                                                  |
| Author / Operator                        | Claude (blackbox-testing skill)                                                                             |
| Skill / Tool Name                        | `blackbox-testing`                                                                                          |
| Model / Agent Version                    | glm-5.1                                                                                                     |
| Prompt Version                           | 1.0                                                                                                         |
| Input Type                               | Requirement / API Spec (Mixed)                                                                              |
| Input Source Path / Link                 | `Assignment 01/codebases/realworld/specification/features/article-lifecycle.md` + upstream bruno specs      |
| Target System / Implementation           | ASP.NET Core RealWorld implementation                                                                       |
| Target Module / Endpoint / Feature Scope | `POST /api/articles`, `GET /api/articles/{slug}`, `PUT /api/articles/{slug}`, `DELETE /api/articles/{slug}` |
| Execution Scope                          | Design + Automation                                                                                         |
| Notes                                    | Covers CRUD, validation, partial update semantics, tag edge cases, and deletion persistence                 |

## 2. Input Summary

### 2.1 Input Overview

- **Project / System Under Test**: RealWorld (Conduit) — a Medium.com-clone backend API
- **Feature Under Test**: Authenticated article creation, update, deletion, and key validation semantics for article fields and tags
- **Actors**: Authenticated user (article owner), authenticated non-owner user, unauthenticated user
- **Preconditions**: The RealWorld API server is running and reachable; a registered user account exists for authenticated operations
- **Business Rules**:
  - Creating an article requires authentication
  - Creating an article succeeds with status 201 when valid input is provided
  - The returned article includes title, slug, description, body, tagList, timestamps, favorite state, favorite count, and author username
  - Duplicate titles are allowed, but each created article must get a unique slug
  - Empty title, description, and body are rejected
  - Unauthenticated create attempts are rejected
  - Updating article body succeeds for the article owner
  - After update, unchanged fields remain unchanged unless explicitly modified
  - Omitting tagList during update preserves the existing tags
  - Setting tagList to an empty array removes all tags
  - Setting tagList to null is rejected
  - Update persistence must be observable when the article is fetched again
  - Deleting an existing owned article succeeds
  - After deletion, later retrieval should fail or indicate the article no longer exists
- **Input Constraints**:
  - Create: title (non-empty string), description (non-empty string), body (non-empty string), tagList (optional array of strings)
  - Update: title, description, body, tagList (all optional; tagList null is rejected)
  - Delete: target article slug, authentication token
  - Authentication: `Authorization: Token {token}` header required for all CUD operations
- **Error Conditions**:
  - 401 for unauthenticated create/update/delete
  - 422 for empty title, description, or body on create
  - 422 for null tagList on update
  - 403 for non-owner update/delete
  - 404 for unknown slug on get/update/delete

### 2.2 Requirement Items

| Requirement ID | Requirement Description                                                                                                                                     | Priority | Notes                                     |
| -------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ----------------------------------------- |
| R1             | Creating an article requires authentication; unauthenticated create is rejected with 401                                                                    | High     | Auth gate for creation                    |
| R2             | Creating an article with valid input succeeds with status 201                                                                                               | High     | Core happy path                           |
| R3             | The created article response includes title, slug, description, body, tagList, createdAt, updatedAt, favorited=false, favoritesCount=0, and author username | High     | Response field completeness               |
| R4             | Duplicate titles are allowed; each article gets a unique slug                                                                                               | High     | Slug uniqueness edge case                 |
| R5             | Empty title is rejected with 422                                                                                                                            | High     | Validation rule                           |
| R6             | Empty description is rejected with 422                                                                                                                      | High     | Validation rule                           |
| R7             | Empty body is rejected with 422                                                                                                                             | High     | Validation rule                           |
| R8             | Unauthenticated create attempts are rejected with 401                                                                                                       | High     | Same as R1; explicit acceptance criterion |
| R9             | Updating article body succeeds for the article owner with status 200                                                                                        | High     | Core update happy path                    |
| R10            | After update, unchanged fields remain unchanged unless explicitly modified                                                                                  | High     | Partial update semantics                  |
| R11            | Omitting tagList during update preserves the existing tags                                                                                                  | High     | Tag preservation rule                     |
| R12            | Setting tagList to an empty array removes all tags                                                                                                          | High     | Tag removal rule                          |
| R13            | Setting tagList to null is rejected with 422                                                                                                                | High     | Null tagList rejection                    |
| R14            | Update persistence must be observable when the article is fetched again                                                                                     | High     | Persistence verification                  |
| R15            | Deleting an existing owned article succeeds with status 204                                                                                                 | High     | Core delete happy path                    |
| R16            | After deletion, later retrieval of that article returns 404                                                                                                 | High     | Deletion persistence verification         |

### 2.3 Assumptions About Input

- Assumption 1: The API uses JSON request/response bodies with a top-level `article` key wrapping article fields, consistent with the RealWorld API specification and bruno test files.
- Assumption 2: Authentication uses the `Authorization: Token {token}` header format, where `{token}` is obtained from user registration or login.
- Assumption 3: Error responses follow the format `{"errors": {"field": ["message"]}}` for validation errors and `{"errors": {"token": ["is missing"]}}` for authentication errors, as shown in the upstream bruno specs.
- Assumption 4: The target server is accessible at a configurable base URL (default `http://localhost:3000`), matching the bruno environment configuration.
- Assumption 5: Slugs are auto-generated from the title; the exact slug generation algorithm is not specified, but slugs must be unique for duplicate titles.
- Assumption 6: Delete returns 204 (no content body), as shown in the bruno spec `16-delete-article.bru`.
- Assumption 7: Non-owner update/delete returns 403 with `errors.article: ["forbidden"]`, as shown in the upstream authorization error bruno specs. This is not in the feature spec but is implied by the phrase "article owner."

## 3. Test Design Strategy

### 3.1 Applied Black-Box Techniques

| Technique                   | Applied? | Where Used                                                                                  | Notes                                          |
| --------------------------- | -------- | ------------------------------------------------------------------------------------------- | ---------------------------------------------- |
| Equivalence Partitioning    | Yes      | Article create/update/delete input fields                                                   | Valid/invalid partitions for each input field  |
| Boundary Value Analysis     | Yes      | Empty vs non-empty for title, description, body; null vs empty-array vs omitted for tagList | Key boundary: empty-string vs minimal valid    |
| Decision Table Testing      | Yes      | TagList update: omitted / empty array / null / new values                                   | 4-way decision on tagList handling             |
| State Transition Testing    | Yes      | Article lifecycle: create → update → delete; post-deletion state                            | State: exists → modified → deleted → not-found |
| Error Guessing              | Yes      | Non-owner operations, unknown slugs, missing auth header                                    | Common API authorization patterns              |
| Scenario / Use-Case Testing | Yes      | Full create-update-delete lifecycle flow                                                    | Primary acceptance criteria                    |

### 3.2 Test Dimension Summary

- Valid input classes: valid create with tags; valid create without tags; valid partial update (body only); valid update with tagList change
- Invalid input classes: empty title; empty description; empty body; null tagList on update
- Boundary values: empty string (below minimum valid) for title, description, body; null vs empty array vs omitted for tagList
- Empty / null / missing cases: empty-string title/description/body; null tagList; omitted tagList; missing auth token
- Format-related cases: N/A — no format constraints beyond non-empty for title/description/body
- Permission / role cases: owner vs non-owner for update and delete; unauthenticated for all CUD operations
- State / sequencing cases: create → update → verify persistence; create → delete → verify 404; delete already-deleted article; get unknown slug
- Combination cases: update body + omit tagList (partial update with tag preservation); multiple validation errors simultaneously

### 3.3 Edge-Case Design Notes

- Empty-string boundary: The spec explicitly calls out empty title (R5), empty description (R6), and empty body (R7). Each is a critical boundary between valid and invalid.
- Null tagList vs empty array vs omitted: The spec explicitly distinguishes these three states for tagList during update (R11, R12, R13). This is the richest edge-case cluster in this feature.
- Partial update semantics: The spec requires that unchanged fields remain unchanged (R10). This implies a test where only one field is sent and all others are verified against their pre-update values.
- Duplicate title slug uniqueness: The spec allows duplicate titles but requires unique slugs (R4). This is a state-dependent edge case requiring two creates.
- Post-deletion state: Retrieving an article after deletion should return 404 (R16). This is a state-transition verification.
- Non-owner authorization: The spec says "article owner" for update (R9) and "owned article" for delete (R15), implying non-owner operations should fail. The bruno specs confirm 403.

## 4. Equivalence Partitioning Analysis

| EP ID | Requirement ID | Input / Rule               | Partition Type | Description                                    | Expected Outcome                              | Covered by Test Case ID |
| ----- | -------------- | -------------------------- | -------------- | ---------------------------------------------- | --------------------------------------------- | ----------------------- |
| EP1   | R2, R3         | Article create inputs      | Valid          | All required fields non-empty, with tagList    | 201 with complete article payload             | TC01                    |
| EP2   | R2, R3         | Article create inputs      | Valid          | All required fields non-empty, without tagList | 201 with article payload, tagList empty array | TC02                    |
| EP3   | R1, R8         | Article create auth        | Invalid        | No authentication token                        | 401 with token error                          | TC03                    |
| EP4   | R5             | Article title              | Invalid        | Empty string title                             | 422 with title validation error               | TC04                    |
| EP5   | R6             | Article description        | Invalid        | Empty string description                       | 422 with description validation error         | TC05                    |
| EP6   | R7             | Article body               | Invalid        | Empty string body                              | 422 with body validation error                | TC06                    |
| EP7   | R4             | Duplicate title            | Valid          | Same title used for two articles               | 201 for both, different slugs                 | TC07                    |
| EP8   | R9, R10        | Article update (body only) | Valid          | Owner sends only body field                    | 200 with body changed, other fields unchanged | TC08                    |
| EP9   | R11            | Article update tagList     | Valid          | tagList omitted from update payload            | 200, existing tags preserved                  | TC09                    |
| EP10  | R12            | Article update tagList     | Valid          | tagList set to empty array                     | 200, all tags removed                         | TC10                    |
| EP11  | R13            | Article update tagList     | Invalid        | tagList set to null                            | 422 with tagList error                        | TC11                    |
| EP12  | R14            | Article update persistence | Valid          | Fetch article after update                     | Updated values visible on GET                 | TC12                    |
| EP13  | R1             | Article update auth        | Invalid        | No authentication token on update              | 401 with token error                          | TC13                    |
| EP14  | R9             | Article update ownership   | Invalid        | Non-owner attempts update                      | 403 with article error                        | TC14                    |
| EP15  | R9             | Article update target      | Invalid        | Update unknown slug                            | 404 with article error                        | TC15                    |
| EP16  | R15            | Article delete (owner)     | Valid          | Owner deletes own article                      | 204 no content                                | TC16                    |
| EP17  | R16            | Article delete persistence | Valid          | Get article after deletion                     | 404 with article error                        | TC17                    |
| EP18  | R1             | Article delete auth        | Invalid        | No authentication token on delete              | 401 with token error                          | TC18                    |
| EP19  | R15            | Article delete ownership   | Invalid        | Non-owner attempts delete                      | 403 with article error                        | TC19                    |
| EP20  | R15            | Article delete target      | Invalid        | Delete unknown slug                            | 404 with article error                        | TC20                    |
| EP21  | R16            | Article retrieval          | Invalid        | Get article with unknown slug                  | 404 with article error                        | TC21                    |

### 4.1 EP Coverage Notes

- Covered partitions: All valid and invalid partitions for the in-scope requirements (R1–R16).
- Missing partitions: Max-length boundaries for title/description/body (spec does not define max lengths); special characters in title affecting slug generation; tag deduplication behavior (e.g., creating with duplicate tags in tagList).
- Partially covered partitions: EP7 (duplicate title) — slug format/structure is not fully specified, so only uniqueness is verified, not the exact slug generation algorithm.

## 5. Boundary Value Analysis

| BVA ID | Requirement ID | Boundary Item        | Boundary Definition                              | Test Values                             | Expected Outcome                                  | Covered by Test Case ID |
| ------ | -------------- | -------------------- | ------------------------------------------------ | --------------------------------------- | ------------------------------------------------- | ----------------------- |
| B1     | R5             | Article title        | Min valid vs just-below-min (empty)              | `""` (empty), `"T"` (minimal non-empty) | `""` → 422; non-empty → 201                       | TC04, TC01              |
| B2     | R6             | Article description  | Min valid vs just-below-min (empty)              | `""` (empty), `"D"` (minimal non-empty) | `""` → 422; non-empty → 201                       | TC05, TC01              |
| B3     | R7             | Article body         | Min valid vs just-below-min (empty)              | `""` (empty), `"B"` (minimal non-empty) | `""` → 422; non-empty → 201                       | TC06, TC01              |
| B4     | R11, R12, R13  | tagList on update    | Three-way boundary: omitted / empty array / null | Omitted, `[]`, `null`                   | Omitted → preserved; `[]` → removed; `null` → 422 | TC09, TC10, TC11        |
| B5     | R4             | Duplicate title slug | Same title → different slug                      | Two creates with same title             | Both 201, slugs differ                            | TC07                    |

### 5.1 BVA Coverage Notes

- Explicit boundaries tested: Empty-string for all three required fields; tagList three-way boundary (omitted / empty / null); slug uniqueness boundary.
- Missing boundaries: Maximum length for title, description, body, and tag values — the spec does not define maximum lengths, so these cannot be tested against a known expected outcome.
- Ambiguous boundary definitions from requirements: The spec does not define what happens with whitespace-only title/description/body. Whether `" "` is treated as empty or valid is unspecified.

## 6. Test Scenarios

| Scenario ID | Requirement Reference | Scenario Title                                    | Scenario Type | Description                                                                      | Priority |
| ----------- | --------------------- | ------------------------------------------------- | ------------- | -------------------------------------------------------------------------------- | -------- |
| S1          | R2, R3                | Successful article creation with tags             | Happy Path    | Create an article with all fields including tags and verify the response payload | High     |
| S2          | R2, R3                | Article creation without tags                     | Happy Path    | Create an article without tagList and verify tagList defaults to empty array     | High     |
| S3          | R1, R8                | Unauthenticated article creation                  | Negative      | Attempt to create an article without authentication token                        | High     |
| S4          | R5                    | Empty title on creation                           | Negative      | Create article with empty title and verify 422 rejection                         | High     |
| S5          | R6                    | Empty description on creation                     | Negative      | Create article with empty description and verify 422 rejection                   | High     |
| S6          | R7                    | Empty body on creation                            | Negative      | Create article with empty body and verify 422 rejection                          | High     |
| S7          | R4                    | Duplicate title produces unique slug              | Edge          | Create two articles with the same title and verify slugs differ                  | High     |
| S8          | R9, R10               | Partial update (body only) preserves other fields | Happy Path    | Update only the body field and verify title, description, tags remain unchanged  | High     |
| S9          | R11                   | Omitting tagList on update preserves tags         | Edge          | Update article body without sending tagList and verify tags are preserved        | High     |
| S10         | R12                   | Empty tagList on update removes all tags          | Edge          | Set tagList to empty array and verify all tags are removed                       | High     |
| S11         | R13                   | Null tagList on update is rejected                | Negative      | Set tagList to null and verify 422 rejection                                     | High     |
| S12         | R14                   | Update persistence verified via GET               | Scenario      | Update article, then GET it, and verify updated values persist                   | High     |
| S13         | R1                    | Unauthenticated article update                    | Negative      | Attempt to update an article without authentication token                        | Medium   |
| S14         | R9                    | Non-owner article update                          | Negative      | User B attempts to update User A's article                                       | Medium   |
| S15         | R9                    | Update unknown slug                               | Negative      | Attempt to update an article that does not exist                                 | Medium   |
| S16         | R15                   | Successful article deletion by owner              | Happy Path    | Delete an owned article and verify 204 response                                  | High     |
| S17         | R16                   | Article retrieval after deletion returns 404      | Scenario      | Delete article, then GET it, and verify 404                                      | High     |
| S18         | R1                    | Unauthenticated article deletion                  | Negative      | Attempt to delete an article without authentication token                        | Medium   |
| S19         | R15                   | Non-owner article deletion                        | Negative      | User B attempts to delete User A's article                                       | Medium   |
| S20         | R15                   | Delete unknown slug                               | Negative      | Attempt to delete an article that does not exist                                 | Medium   |
| S21         | R16                   | Get unknown article slug                          | Negative      | GET an article with a slug that does not exist                                   | Medium   |

## 7. Edge Case Matrix

| Requirement ID | Edge Category  | Concrete Case                                        | Covered by Test Case ID | Notes                                                   |
| -------------- | -------------- | ---------------------------------------------------- | ----------------------- | ------------------------------------------------------- |
| R2, R3         | Valid          | Create article with tagList containing multiple tags | TC01                    | Tag ordering verified                                   |
| R2, R3         | Omission       | Create article without tagList field                 | TC02                    | tagList defaults to empty array                         |
| R5             | Empty          | Empty-string title on creation                       | TC04                    | Explicit acceptance criterion                           |
| R5             | Boundary       | Whitespace-only title on creation                    | Deferred                | Spec does not specify outcome for whitespace-only title |
| R6             | Empty          | Empty-string description on creation                 | TC05                    | Explicit acceptance criterion                           |
| R7             | Empty          | Empty-string body on creation                        | TC06                    | Explicit acceptance criterion                           |
| R4             | Duplicate      | Two articles with same title get different slugs     | TC07                    | Explicit acceptance criterion                           |
| R4             | State          | Slug format for duplicate titles                     | Deferred                | Exact slug generation algorithm not specified           |
| R9, R10        | Partial Update | Update body only; other fields unchanged             | TC08                    | Core partial update semantics                           |
| R11            | Omission       | Omit tagList in update payload                       | TC09                    | Explicit acceptance criterion                           |
| R12            | Empty          | Set tagList to `[]` in update payload                | TC10                    | Explicit acceptance criterion                           |
| R13            | Null           | Set tagList to `null` in update payload              | TC11                    | Explicit acceptance criterion                           |
| R13            | Wrong Type     | tagList set to a string instead of array             | Deferred                | Lower priority; spec only covers null case              |
| R14            | State          | GET after update shows persisted changes             | TC12                    | Persistence verification                                |
| R1             | Missing        | No auth header on create                             | TC03                    | Explicit acceptance criterion                           |
| R1             | Missing        | No auth header on update                             | TC13                    | Symmetric with create                                   |
| R1             | Missing        | No auth header on delete                             | TC18                    | Symmetric with create/update                            |
| R9, R15        | Unauthorized   | Non-owner update attempt                             | TC14                    | Implied by "article owner" phrasing; confirmed by bruno |
| R9, R15        | Unauthorized   | Non-owner delete attempt                             | TC19                    | Implied by "owned article" phrasing; confirmed by bruno |
| R15, R16       | State          | GET after delete returns 404                         | TC17                    | Explicit acceptance criterion                           |
| R15            | Missing        | Delete unknown (non-existent) slug                   | TC20                    | Complement of happy-path delete                         |
| R16            | Missing        | Get unknown slug                                     | TC21                    | Baseline not-found case                                 |
| R9             | Missing        | Update unknown slug                                  | TC15                    | Symmetric with delete unknown                           |

## 8. Detailed Test Cases

| Test Case ID | Title                                              | Requirement Reference | Preconditions                                                   | Test Data                                                                                                                    | Steps                                                                                                                                                                                     | Expected Result                                                                                                                                                                                                                                                                                                                                                                                   | Priority | Risk / Notes                                 |
| ------------ | -------------------------------------------------- | --------------------- | --------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | -------------------------------------------- |
| TC01         | Create article with valid input and tags           | R2, R3                | API server running; authenticated user exists                   | title=`Test Article {uid}`, description=`Test description`, body=`Test body content`, tagList=`["tag1_{uid}", "tag2_{uid}"]` | 1. POST `/api/articles` with `{"article":{"title":"Test Article {uid}","description":"Test description","body":"Test body content","tagList":["tag1_{uid}","tag2_{uid}"]}}` + auth header | Status 201; response `article.title` matches input; `article.slug` is a non-empty string; `article.description` matches; `article.body` matches; `article.tagList` contains both tags in order; `article.createdAt` matches ISO 8601; `article.updatedAt` matches ISO 8601; `article.favorited` is false; `article.favoritesCount` is 0; `article.author.username` matches the authenticated user | High     | Core happy path with full field verification |
| TC02         | Create article without tagList                     | R2, R3                | API server running; authenticated user exists                   | title=`NoTag Article {uid}`, description=`Desc`, body=`Body`                                                                 | 1. POST `/api/articles` with `{"article":{"title":"NoTag Article {uid}","description":"Desc","body":"Body"}}` + auth header                                                               | Status 201; `article.tagList` is an empty array `[]`                                                                                                                                                                                                                                                                                                                                              | High     | Omission case for optional tagList           |
| TC03         | Create article without authentication              | R1, R8                | API server running                                              | title=`NoAuth Article`, description=`test`, body=`test`                                                                      | 1. POST `/api/articles` with `{"article":{"title":"NoAuth Article","description":"test","body":"test"}}` (no auth header)                                                                 | Status 401; `errors.token[0]` equals `"is missing"`                                                                                                                                                                                                                                                                                                                                               | High     | Explicit acceptance criterion                |
| TC04         | Create article with empty title                    | R5                    | API server running; authenticated user exists                   | title=`""`, description=`test`, body=`test`                                                                                  | 1. POST `/api/articles` with `{"article":{"title":"","description":"test","body":"test"}}` + auth header                                                                                  | Status 422; `errors.title[0]` equals `"can't be blank"`                                                                                                                                                                                                                                                                                                                                           | High     | Explicit acceptance criterion                |
| TC05         | Create article with empty description              | R6                    | API server running; authenticated user exists                   | title=`ErrDesc {uid}`, description=`""`, body=`test`                                                                         | 1. POST `/api/articles` with `{"article":{"title":"ErrDesc {uid}","description":"","body":"test"}}` + auth header                                                                         | Status 422; `errors.description[0]` equals `"can't be blank"`                                                                                                                                                                                                                                                                                                                                     | High     | Explicit acceptance criterion                |
| TC06         | Create article with empty body                     | R7                    | API server running; authenticated user exists                   | title=`ErrBody {uid}`, description=`test`, body=`""`                                                                         | 1. POST `/api/articles` with `{"article":{"title":"ErrBody {uid}","description":"test","body":""}}` + auth header                                                                         | Status 422; `errors.body[0]` equals `"can't be blank"`                                                                                                                                                                                                                                                                                                                                            | High     | Explicit acceptance criterion                |
| TC07         | Create two articles with duplicate title           | R4                    | API server running; authenticated user exists                   | title=`Dup Title {uid}` (same for both), description and body differ                                                         | 1. POST `/api/articles` with first article 2. POST `/api/articles` with same title but different description/body                                                                         | Both return 201; `article.slug` values differ between the two responses                                                                                                                                                                                                                                                                                                                           | High     | Slug uniqueness edge case                    |
| TC08         | Update article body only                           | R9, R10               | An article exists with known title, description, body, and tags | body=`Updated body content`                                                                                                  | 1. PUT `/api/articles/{slug}` with `{"article":{"body":"Updated body content"}}` + owner auth header                                                                                      | Status 200; `article.body` equals `"Updated body content"`; `article.title` unchanged; `article.description` unchanged; `article.slug` unchanged; `article.tagList` unchanged; `article.updatedAt` differs from previous value                                                                                                                                                                    | High     | Core partial update semantics                |
| TC09         | Update article without tagList preserves tags      | R11                   | An article with tags exists                                     | body=`Body without touching tags`                                                                                            | 1. PUT `/api/articles/{slug}` with `{"article":{"body":"Body without touching tags"}}` (no tagList key) + owner auth header                                                               | Status 200; `article.body` equals `"Body without touching tags"`; `article.tagList` still contains all original tags                                                                                                                                                                                                                                                                              | High     | Explicit acceptance criterion                |
| TC10         | Update article with empty tagList removes all tags | R12                   | An article with tags exists                                     | tagList=`[]`                                                                                                                 | 1. PUT `/api/articles/{slug}` with `{"article":{"tagList":[]}}` + owner auth header                                                                                                       | Status 200; `article.tagList` is an empty array `[]`                                                                                                                                                                                                                                                                                                                                              | High     | Explicit acceptance criterion                |
| TC11         | Update article with null tagList is rejected       | R13                   | An article exists                                               | tagList=`null`                                                                                                               | 1. PUT `/api/articles/{slug}` with `{"article":{"tagList":null}}` + owner auth header                                                                                                     | Status 422; error response references tagList                                                                                                                                                                                                                                                                                                                                                     | High     | Explicit acceptance criterion                |
| TC12         | Update persistence verified via GET                | R14                   | An article has been updated                                     | (no additional test data)                                                                                                    | 1. Update article body 2. GET `/api/articles/{slug}`                                                                                                                                      | GET returns 200; `article.body` equals the updated value; all other updated fields reflect the update                                                                                                                                                                                                                                                                                             | High     | Explicit acceptance criterion                |
| TC13         | Update article without authentication              | R1                    | API server running; an article exists                           | body=`test`                                                                                                                  | 1. PUT `/api/articles/{slug}` with `{"article":{"body":"test"}}` (no auth header)                                                                                                         | Status 401; `errors.token[0]` equals `"is missing"`                                                                                                                                                                                                                                                                                                                                               | Medium   | Symmetric with unauthenticated create        |
| TC14         | Non-owner attempts to update article               | R9                    | Two users registered; an article owned by User A                | body=`hijacked`                                                                                                              | 1. PUT `/api/articles/{slug}` with User B's auth token                                                                                                                                    | Status 403; `errors.article[0]` equals `"forbidden"`                                                                                                                                                                                                                                                                                                                                              | Medium   | Authorization edge case from bruno specs     |
| TC15         | Update article with unknown slug                   | R9                    | API server running; authenticated user exists                   | body=`test`, slug=`unknown-slug-{uid}`                                                                                       | 1. PUT `/api/articles/unknown-slug-{uid}` with `{"article":{"body":"test"}}` + auth header                                                                                                | Status 404; `errors.article[0]` equals `"not found"`                                                                                                                                                                                                                                                                                                                                              | Medium   | Non-existent resource                        |
| TC16         | Delete article by owner                            | R15                   | An article owned by the authenticated user exists               | (slug of existing article)                                                                                                   | 1. DELETE `/api/articles/{slug}` + owner auth header                                                                                                                                      | Status 204; response has no body                                                                                                                                                                                                                                                                                                                                                                  | High     | Explicit acceptance criterion                |
| TC17         | Get article after deletion returns 404             | R16                   | An article has been deleted                                     | (slug of deleted article)                                                                                                    | 1. GET `/api/articles/{slug}`                                                                                                                                                             | Status 404; `errors.article[0]` equals `"not found"`                                                                                                                                                                                                                                                                                                                                              | High     | Explicit acceptance criterion                |
| TC18         | Delete article without authentication              | R1                    | API server running; an article exists                           | (slug of existing article)                                                                                                   | 1. DELETE `/api/articles/{slug}` (no auth header)                                                                                                                                         | Status 401; `errors.token[0]` equals `"is missing"`                                                                                                                                                                                                                                                                                                                                               | Medium   | Symmetric with other unauthenticated ops     |
| TC19         | Non-owner attempts to delete article               | R15                   | Two users registered; an article owned by User A                | (slug of User A's article)                                                                                                   | 1. DELETE `/api/articles/{slug}` with User B's auth token                                                                                                                                 | Status 403; `errors.article[0]` equals `"forbidden"`                                                                                                                                                                                                                                                                                                                                              | Medium   | Authorization edge case from bruno specs     |
| TC20         | Delete article with unknown slug                   | R15                   | API server running; authenticated user exists                   | slug=`unknown-slug-{uid}`                                                                                                    | 1. DELETE `/api/articles/unknown-slug-{uid}` + auth header                                                                                                                                | Status 404; `errors.article[0]` equals `"not found"`                                                                                                                                                                                                                                                                                                                                              | Medium   | Non-existent resource                        |
| TC21         | Get article with unknown slug                      | R16                   | API server running                                              | slug=`unknown-slug-{uid}`                                                                                                    | 1. GET `/api/articles/unknown-slug-{uid}`                                                                                                                                                 | Status 404; `errors.article[0]` equals `"not found"`                                                                                                                                                                                                                                                                                                                                              | Medium   | Baseline not-found case                      |

## 9. How to Run the Generated Test Codes

### 9.1 Prerequisites

- Target system: ASP.NET Core RealWorld implementation running and reachable
- .NET 8 SDK (or later) installed
- xUnit test runner available via `dotnet test`
- The RealWorld API server must be started before running tests (see Environment Setup)

### 9.2 Test Code Location

- **Generated Test Code Path**: `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/`
- **Project Root Path**: `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/`
- **Test Entry File / Directory**: `ArticleLifecycleTests.cs`
- **Related Configuration Files**: `RealWorld.Blackbox.Tests.csproj`

### 9.3 Environment Setup

1. **Working Directory**: `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/`
2. **Runtime Version**: .NET 8 SDK or later
3. **Dependency Install Command**: `dotnet restore`
4. **Build Command**: `dotnet build`
5. **Test Environment Variables**:
   - `REALWORLD_BASE_URL`: Base URL of the running RealWorld API (default: `http://localhost:3000`)
6. **Service Start Command**: Start the ASP.NET Core RealWorld server on the configured host/port before running tests
7. **Test Data / Seed Command**: No seed data required; tests register their own users and create their own articles with unique IDs

### 9.4 Run Commands

```bash
# Restore dependencies
dotnet restore

# Run all tests
dotnet test

# Run only ArticleLifecycle tests
dotnet test --filter "FullyQualifiedName~ArticleLifecycle"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run a specific test by name
dotnet test --filter "FullyQualifiedName~CreateArticleWithTags_Returns201WithAllFields"

# Run with no caching (force re-execution)
dotnet test --no-build && dotnet test

# Run with custom base URL
REALWORLD_BASE_URL=http://localhost:5000 dotnet test
```

### 9.5 Execution Notes

- Start the target service before running black-box tests. Tests will fail with connection errors if the server is unreachable.
- Each test generates unique user IDs and article titles (GUID-based) to avoid collisions with existing data. Tests are idempotent and can be re-run without manual cleanup.
- TC07 (duplicate title) creates two articles with the same title and verifies slug uniqueness. TC14 and TC19 (non-owner operations) register two separate users.
- TC10 (empty tagList removing tags) and TC09 (omitted tagList preserving tags) are complementary and depend on the article having tags initially.
- If the test framework caches results, force re-execution with `dotnet test --no-build` after a clean build.
- Record the exact `REALWORLD_BASE_URL` value used for reproducibility.

## 10. Coverage Summary

### 10.1 Requirement Coverage Table

| Requirement ID | EP Covered? | BVA Covered? | Edge Case Covered? | Negative Case Covered? | State / Sequence Covered? | Covered by Test Cases | Coverage Status | Notes                                       |
| -------------- | ----------- | ------------ | ------------------ | ---------------------- | ------------------------- | --------------------- | --------------- | ------------------------------------------- |
| R1             | Yes         | N/A          | Yes                | Yes                    | N/A                       | TC03, TC13, TC18      | Full            | Auth gate tested for create, update, delete |
| R2             | Yes         | Yes          | Yes                | N/A                    | N/A                       | TC01, TC02            | Full            | Valid create with and without tags          |
| R3             | Yes         | N/A          | Yes                | N/A                    | N/A                       | TC01, TC02            | Full            | All response fields verified                |
| R4             | Yes         | Yes          | Yes                | N/A                    | Yes                       | TC07                  | Full            | Duplicate title with slug uniqueness        |
| R5             | Yes         | Yes          | Yes                | Yes                    | N/A                       | TC04                  | Full            | Empty title boundary                        |
| R6             | Yes         | Yes          | Yes                | Yes                    | N/A                       | TC05                  | Full            | Empty description boundary                  |
| R7             | Yes         | Yes          | Yes                | Yes                    | N/A                       | TC06                  | Full            | Empty body boundary                         |
| R8             | Yes         | N/A          | Yes                | Yes                    | N/A                       | TC03                  | Full            | Same as R1; explicit acceptance criterion   |
| R9             | Yes         | N/A          | Yes                | Yes                    | N/A                       | TC08, TC14, TC15      | Full            | Owner update + non-owner + unknown slug     |
| R10            | Yes         | N/A          | Yes                | N/A                    | N/A                       | TC08                  | Full            | Partial update field preservation           |
| R11            | Yes         | Yes          | Yes                | N/A                    | N/A                       | TC09                  | Full            | Tag preservation on omit                    |
| R12            | Yes         | Yes          | Yes                | N/A                    | N/A                       | TC10                  | Full            | Tag removal with empty array                |
| R13            | Yes         | Yes          | Yes                | Yes                    | N/A                       | TC11                  | Full            | Null tagList rejection                      |
| R14            | Yes         | N/A          | Yes                | N/A                    | Yes                       | TC12                  | Full            | Persistence via GET after update            |
| R15            | Yes         | N/A          | Yes                | Yes                    | N/A                       | TC16, TC19, TC20      | Full            | Owner delete + non-owner + unknown slug     |
| R16            | Yes         | N/A          | Yes                | N/A                    | Yes                       | TC17, TC21            | Full            | 404 after deletion and for unknown slug     |

### 10.2 EP / BVA to Test Case Mapping

| Analysis Item ID | Type | Requirement ID | Description                       | Mapped Test Case ID(s) | Covered? | Notes                  |
| ---------------- | ---- | -------------- | --------------------------------- | ---------------------- | -------- | ---------------------- |
| EP1              | EP   | R2, R3         | Valid create with tags            | TC01                   | Yes      |                        |
| EP2              | EP   | R2, R3         | Valid create without tags         | TC02                   | Yes      |                        |
| EP3              | EP   | R1, R8         | Invalid: no auth on create        | TC03                   | Yes      |                        |
| EP4              | EP   | R5             | Invalid: empty title              | TC04                   | Yes      |                        |
| EP5              | EP   | R6             | Invalid: empty description        | TC05                   | Yes      |                        |
| EP6              | EP   | R7             | Invalid: empty body               | TC06                   | Yes      |                        |
| EP7              | EP   | R4             | Valid: duplicate title            | TC07                   | Yes      |                        |
| EP8              | EP   | R9, R10        | Valid: partial update body        | TC08                   | Yes      |                        |
| EP9              | EP   | R11            | Valid: omit tagList on update     | TC09                   | Yes      |                        |
| EP10             | EP   | R12            | Valid: empty tagList on update    | TC10                   | Yes      |                        |
| EP11             | EP   | R13            | Invalid: null tagList on update   | TC11                   | Yes      |                        |
| EP12             | EP   | R14            | Valid: update persistence         | TC12                   | Yes      |                        |
| EP13             | EP   | R1             | Invalid: no auth on update        | TC13                   | Yes      |                        |
| EP14             | EP   | R9             | Invalid: non-owner update         | TC14                   | Yes      |                        |
| EP15             | EP   | R9             | Invalid: unknown slug on update   | TC15                   | Yes      |                        |
| EP16             | EP   | R15            | Valid: owner delete               | TC16                   | Yes      |                        |
| EP17             | EP   | R16            | Valid: get after delete           | TC17                   | Yes      |                        |
| EP18             | EP   | R1             | Invalid: no auth on delete        | TC18                   | Yes      |                        |
| EP19             | EP   | R15            | Invalid: non-owner delete         | TC19                   | Yes      |                        |
| EP20             | EP   | R15            | Invalid: unknown slug on delete   | TC20                   | Yes      |                        |
| EP21             | EP   | R16            | Invalid: get unknown slug         | TC21                   | Yes      |                        |
| B1               | BVA  | R5             | Title empty-string boundary       | TC04, TC01             | Yes      |                        |
| B2               | BVA  | R6             | Description empty-string boundary | TC05, TC01             | Yes      |                        |
| B3               | BVA  | R7             | Body empty-string boundary        | TC06, TC01             | Yes      |                        |
| B4               | BVA  | R11, R12, R13  | tagList three-way boundary        | TC09, TC10, TC11       | Yes      | Omitted / empty / null |
| B5               | BVA  | R4             | Duplicate title slug boundary     | TC07                   | Yes      |                        |

### 10.3 Coverage Metrics

| Metric                 | Formula                                              | Value        |
| ---------------------- | ---------------------------------------------------- | ------------ |
| Requirement Coverage   | covered_requirements / total_requirements            | 16/16 = 100% |
| EP Coverage            | covered_partitions / total_partitions                | 21/21 = 100% |
| BVA Coverage           | covered_boundaries / total_boundaries                | 5/5 = 100%   |
| Edge Case Coverage     | covered_edge_categories / applicable_edge_categories | 20/22 = 91%  |
| Negative Case Coverage | negative_cases_present / applicable_requirements     | 8/8 = 100%   |
| Duplicate Case Rate    | duplicate_cases / total_cases                        | 0/21 = 0%    |
| Executability Score    | 1-5                                                  | 4            |

### 10.4 Coverage Notes

- Strongest covered area: R11/R12/R13 (tagList update semantics) — all three boundary states (omitted, empty, null) are explicitly tested, plus the persistence verification.
- Weakest covered area: R4 (duplicate title slug) — only slug uniqueness is verified; the exact slug format and collision-resolution strategy are not tested because they are not specified.
- Over-covered or duplicated areas: R1 and R8 are the same requirement tested by the same test case (TC03).
- Under-covered areas: Whitespace-only inputs for title/description/body; maximum-length inputs; tagList with wrong type (string instead of array); tag deduplication; malformed JSON body; missing Content-Type header.

## 11. Ambiguities / Missing Information / Assumptions

### 11.1 Ambiguous Requirements

- Item 1: The spec says "duplicate titles are allowed, but each created article must still get a unique slug" but does not define the slug generation algorithm or how collisions are resolved (e.g., numeric suffix, random hash, timestamp). Tests verify uniqueness only.
- Item 2: The spec says "empty title is rejected" but does not specify whether whitespace-only titles (e.g., `" "`) are treated as empty or valid. Tests assume empty-string only.
- Item 3: The spec says "after deletion, later retrieval of that article should fail or indicate the article no longer exists" — the expected status code is not explicitly stated. The bruno spec uses 404, which the tests follow.
- Item 4: The spec says "updating article body succeeds for the article owner" — whether non-owner update returns 403 or 404 is not specified. The bruno authorization specs confirm 403.

### 11.2 Missing Information

- Missing validation rule: Maximum length constraints for title, description, body, and individual tag values.
- Missing validation rule: Whether whitespace-only title/description/body is treated as empty.
- Missing error behavior: Behavior when the request body is entirely malformed (not valid JSON) or when the `Content-Type` header is incorrect.
- Missing behavior: Tag deduplication behavior when creating an article with duplicate tags in the tagList.
- Missing behavior: What happens when creating an article with tagList as an empty array `[]` vs omitting tagList entirely.
- Missing behavior: Whether update can set title/description/body to empty (the spec only covers create validation, not update validation).

### 11.3 Assumptions

- Assumption 1: Article request/response bodies use the `{"article": {...}}` JSON wrapper consistent with the RealWorld API specification and bruno test files.
- Assumption 2: Authentication uses the `Authorization: Token {token}` header format, where `{token}` is obtained from user registration or login.
- Assumption 3: Error responses follow the format `{"errors": {"field": ["message"]}}` for validation errors and `{"errors": {"token": ["is missing"]}}` for authentication errors, as shown in the upstream bruno specs.
- Assumption 4: The `REALWORLD_BASE_URL` environment variable (defaulting to `http://localhost:3000`) is sufficient to configure the test target.
- Assumption 5: Non-owner update/delete returns 403 with `errors.article: ["forbidden"]`, as shown in the upstream authorization error bruno specs. This is not in the feature spec but is implied by the "article owner" phrasing.
- Assumption 6: Delete returns 204 with no response body, as shown in the bruno spec `16-delete-article.bru`.
- Assumption 7: Creating an article without a tagList field results in tagList being an empty array in the response, consistent with the bruno spec behavior.
