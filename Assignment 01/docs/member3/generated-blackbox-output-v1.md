## Feature Summary
- This suite covers mixed black-box testing for the RealWorld core API and TodoMVC core UI behavior.
- The scope includes authentication, protected profile access, article lifecycle mutations, follow/favorite toggles, article list filtering and pagination, comment lifecycle behavior, and TodoMVC create/edit/bulk/routing behavior.
- The suite emphasizes externally observable behavior only: HTTP status/body, visible UI state, route/filter results, and user-triggered state transitions.
- Edge coverage is mandatory for authorization, ownership, missing fields, duplicate identities, pagination bounds, trimmed input, empty input, and state synchronization behavior.

## Requirements Extracted
| Req ID | Requirement | Notes |
| --- | --- | --- |
| R1 | `POST /users` registers a user and returns a token. Duplicate username or email must fail with conflict or validation error. | Covers successful registration plus duplicate identity rejection. |
| R2 | `POST /users/login` authenticates an existing user. Invalid credentials must fail and must not return a token. | Covers successful login and credential rejection. |
| R3 | `GET /user` and `PUT /user` require authorization. Missing token returns unauthorized. Supported fields should persist after update. | Includes protected access and persistence of updated values. |
| R4 | `POST /articles` creates an article with required title, description, and body. Malformed payload should fail validation. | Required field validation is explicitly in scope. |
| R5 | `PUT/DELETE /articles/{slug}` allow only the owner. Non-owner gets forbidden. Missing slug returns not found. | Ownership and missing-resource behavior are both required. |
| R6 | `POST/DELETE /profiles/{username}/follow` and `/articles/{slug}/favorite` require auth and must toggle state correctly. | Covers auth and visible state transitions. |
| R7 | `GET /articles` supports `tag`, `author`, `favorited`, `limit`, and `offset` with valid boundaries. | Boundary behavior includes `limit >= 1` and `offset >= 0`. |
| R8 | `POST /articles/{slug}/comments` creates a comment for an existing article. Comment delete enforces ownership and proper errors. | Includes create, ownership, and missing-resource error behavior. |
| R9 | TodoMVC create trims input, ignores empty text, Enter creates an item, and input clears. | Create flow and empty-input rejection are required. |
| R10 | TodoMVC edit saves on blur/enter, Escape cancels, and empty edited title deletes the item. | Includes save, cancel, and delete-on-empty. |
| R11 | TodoMVC mark-all synchronizes with item states, and clear-completed removes completed items and updates UI state. | Bulk toggle and cleanup state synchronization are required. |
| R12 | TodoMVC routing supports `#/`, `#/active`, and `#/completed` and updates filtered list and selected state consistently. | Route filtering and selected-tab state are required. |

## Test Design Strategy
- Techniques used: equivalence partitioning, boundary value analysis, scenario-based testing, state transition testing, permission testing, and error guessing.
- Scope decisions: API cases are grouped around externally visible request/response behavior and ownership/auth rules; UI cases are grouped around visible list state, route state, and keyboard/blur interactions.
- Priority decisions: high-priority API auth/ownership/validation cases receive both normal and negative coverage; medium-priority filtering and UI flows receive representative normal, negative, and state-synchronization coverage.
- Non-goals: internal database state, slug generation algorithm details, token format internals, implementation-specific DOM structure, and performance behavior.
- Boundary focus: duplicate identity, missing wrappers/fields, unauthorized access, forbidden mutation, not-found resource, pagination lower bounds, trimmed todo input, empty edited title, and repeated route/filter transitions.

## Test Scenarios
| Scenario ID | Related Requirements | Scenario Description | Type |
| --- | --- | --- | --- |
| S01 | R1 | Register with unique username/email and receive tokenized user response. | happy-path |
| S02 | R1 | Attempt registration with duplicate username or duplicate email. | negative |
| S03 | R2 | Log in with valid existing credentials. | happy-path |
| S04 | R2 | Log in with invalid credentials or missing required login field. | negative |
| S05 | R3 | Access current user and update supported profile fields with valid token. | happy-path |
| S06 | R3 | Access protected user endpoints without token. | negative |
| S07 | R4 | Create article with all required fields present. | happy-path |
| S08 | R4 | Submit malformed article payload missing required wrapper or field. | negative |
| S09 | R5 | Owner updates or deletes own article successfully. | happy-path |
| S10 | R5 | Non-owner update/delete is forbidden; missing slug returns not found. | negative |
| S11 | R6 | Authenticated user follows/unfollows profile and favorites/unfavorites article with visible state toggle. | happy-path |
| S12 | R6 | Missing token on follow/favorite actions returns unauthorized. | negative |
| S13 | R7 | Query articles by filter combination and valid pagination bounds. | happy-path |
| S14 | R7 | Use out-of-bound pagination values such as `limit=0` or negative offset. | boundary/negative |
| S15 | R8 | Create a comment on an existing article and owner deletes own comment. | happy-path |
| S16 | R8 | Non-owner comment delete is forbidden; missing article/comment returns not found. | negative |
| S17 | R9 | Create todo with trimmed text using Enter, then verify input clears. | happy-path |
| S18 | R9 | Attempt todo creation with blank or whitespace-only input. | negative |
| S19 | R10 | Edit todo and save by Enter or blur; cancel by Escape; empty edited title deletes item. | state/edge |
| S20 | R11 | Mark-all and clear-completed keep item states and counters synchronized. | state/edge |
| S21 | R12 | Switch among all/active/completed routes and verify filtered list and selected state. | state/route |

## Detailed Test Cases
| Test Case ID | Title | Requirement Reference | Preconditions | Test Data | Steps | Expected Result | Priority | Risk/Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| TC-001 | Register new user successfully | R1 | API server reachable; no existing user with same username/email. | `user.username` unique, `user.email` unique, valid `user.password`. | 1. Send `POST /users` with wrapped `user` object.<br>2. Observe status and response body. | Response indicates success; response contains created user identity and non-empty token; returned username/email match submitted values. | High | Baseline status code may vary by implementation, but success and token return are mandatory. |
| TC-002 | Reject duplicate username on registration | R1 | Existing user account already created. | New request reuses existing username with different email. | 1. Register first user.<br>2. Send second `POST /users` with duplicate username.<br>3. Observe status/body. | Second request fails with conflict or validation-style error; no token is returned for the failed registration. | High | Confirms unique username enforcement. |
| TC-003 | Reject duplicate email on registration | R1 | Existing user account already created. | New request reuses existing email with different username. | 1. Register first user.<br>2. Send second `POST /users` with duplicate email.<br>3. Observe status/body. | Second request fails with conflict or validation-style error; no token is returned for the failed registration. | High | Confirms unique email enforcement. |
| TC-004 | Login with valid credentials | R2 | Existing user account created. | Valid email/password pair for existing user. | 1. Send `POST /users/login` with wrapped credentials.<br>2. Observe status/body. | Authentication succeeds; response contains user identity and non-empty token; returned identity matches the account used to log in. | High | Representative positive auth case. |
| TC-005 | Reject invalid login credentials | R2 | Existing user account created. | Valid email with wrong password. | 1. Send `POST /users/login` using wrong password.<br>2. Observe status/body. | Request fails with authentication error; no token is returned. | High | Confirms invalid credential rejection. |
| TC-006 | Reject malformed login payload | R2 | API server reachable. | Missing wrapped `user` object or missing required field such as email/password. | 1. Send malformed `POST /users/login` payload.<br>2. Observe status/body. | Request fails with validation error; response does not contain token. | High | Covers wrapper/required-field rule. |
| TC-007 | Get current user with valid token | R3 | Existing authenticated user and valid token available. | Authorization token header. | 1. Send `GET /user` with valid token.<br>2. Observe status/body. | Request succeeds and returns current user data corresponding to the token owner. | High | Confirms protected read access. |
| TC-008 | Reject current user access without token | R3 | API server reachable. | No authorization token. | 1. Send `GET /user` without token.<br>2. Observe status/body. | Request fails with unauthorized error. | High | Mandatory auth-negative case. |
| TC-009 | Update current user and persist supported fields | R3 | Existing authenticated user and valid token available. | Valid update payload containing supported fields such as bio/image/email/username. | 1. Send `PUT /user` with valid token and supported field updates.<br>2. Observe update response.<br>3. Send `GET /user` with same token. | Update request succeeds; updated fields in response match submitted values; subsequent `GET /user` shows persisted changes. | High | Confirms write persistence, not just echo response. |
| TC-010 | Create article successfully | R4 | Authenticated user and valid token available. | Wrapped `article` payload with non-empty title, description, and body. | 1. Send `POST /articles` with valid token and required fields.<br>2. Observe status/body. | Request succeeds; response contains created article with submitted title/description/body and accessible slug or identifier. | High | Article owner context needed for downstream ownership cases. |
| TC-011 | Reject malformed article payload | R4 | Authenticated user and valid token available. | Payload missing wrapper or one required field among title/description/body. | 1. Send malformed `POST /articles` request.<br>2. Observe status/body. | Request fails with validation error; no article is created. | High | Covers wrapper + required-field constraints. |
| TC-012 | Allow article owner to update own article | R5 | Existing article owned by authenticated user. | Valid token for owner; updated article field(s). | 1. Send `PUT /articles/{slug}` as owner.<br>2. Observe status/body.<br>3. Retrieve article if needed. | Update succeeds; changed field values are visible in response and in subsequent retrieval. | High | Positive ownership case. |
| TC-013 | Forbid non-owner article mutation | R5 | Existing article created by user A; user B authenticated separately. | Valid token for non-owner user B. | 1. Send `PUT` or `DELETE /articles/{slug}` as non-owner.<br>2. Observe status/body. | Request fails with forbidden error; target article remains unaffected. | High | Ownership enforcement is a key business rule. |
| TC-014 | Return not found for missing article slug mutation | R5 | Authenticated user and valid token available. | Non-existent article slug. | 1. Send `PUT` or `DELETE /articles/{missingSlug}`.<br>2. Observe status/body. | Request fails with not-found error. | High | Missing-resource edge case. |
| TC-015 | Toggle follow and favorite state with auth | R6 | Authenticated user; target profile and article exist. | Valid token; target username; target article slug. | 1. `POST /profiles/{username}/follow`.<br>2. Verify followed state in response.<br>3. `DELETE /profiles/{username}/follow`.<br>4. Verify unfollowed state.<br>5. `POST /articles/{slug}/favorite` then `DELETE` favorite.<br>6. Observe favorite state/count changes. | Each authenticated action succeeds; returned follow/favorite state toggles correctly after each action; visible state in responses matches action order. | Medium | Combines both toggle endpoints to cover state transitions. |
| TC-016 | Reject follow and favorite operations without auth | R6 | Target profile and article exist. | No authorization token. | 1. Send follow request without token.<br>2. Send favorite request without token.<br>3. Observe status/body. | Each protected action fails with unauthorized error. | Medium | Mandatory protected-action negative case. |
| TC-017 | Filter articles with valid pagination bounds | R7 | Seed data exists with distinguishable tags/authors/favorites. | Query values for `tag`, `author`, or `favorited`; `limit=1`; `offset=0` or larger valid offset. | 1. Send `GET /articles` with one or more supported filters and valid pagination.<br>2. Repeat with another valid offset.<br>3. Observe list/count behavior. | Request succeeds; returned items satisfy supplied filters; `limit=1` returns at most one item; `offset=0` works as valid lower boundary. | Medium | Covers valid lower bounds explicitly. |
| TC-018 | Reject invalid pagination bounds | R7 | API server reachable. | Invalid query such as `limit=0` and/or `offset=-1`. | 1. Send `GET /articles` with invalid bound values.<br>2. Observe status/body. | Request fails with validation error or equivalent rejection for invalid bounds. | Medium | Derived from `limit >= 1`, `offset >= 0`. |
| TC-019 | Create and delete own comment successfully | R8 | Existing article; authenticated user with valid token. | Wrapped `comment.body`; valid article slug. | 1. Send `POST /articles/{slug}/comments` with valid token.<br>2. Capture returned comment identifier.<br>3. Send `DELETE /articles/{slug}/comments/{id}` as same user.<br>4. Observe status/body. | Comment creation succeeds and returns created comment content; owner delete succeeds; deleted comment is no longer available through normal retrieval flow. | High | Positive create/delete lifecycle. |
| TC-020 | Enforce comment ownership and missing-resource errors | R8 | Existing article and comment owned by user A; user B authenticated separately. | Valid token for non-owner user B; also a missing article slug or comment id. | 1. Attempt to delete existing comment as non-owner.<br>2. Attempt comment create or delete on missing article/comment resource.<br>3. Observe status/body. | Non-owner delete fails with forbidden error; missing article or comment operations fail with not-found error. | High | Covers both ownership and missing-resource negatives. |
| TC-021 | Create todo by trimmed Enter input and clear entry field | R9 | TodoMVC app loaded on all-items route. | Input text with leading/trailing spaces, e.g. `"  buy milk  "`. | 1. Enter spaced text into new-todo input.<br>2. Press Enter.<br>3. Observe list and input field. | One new todo is created using trimmed text `buy milk`; whitespace-only edges are not preserved; input field becomes empty after creation. | Medium | Positive create + trimming behavior in one case. |
| TC-022 | Ignore empty or whitespace-only todo creation | R9 | TodoMVC app loaded. | Empty string and whitespace-only string. | 1. Submit empty input with Enter.<br>2. Submit whitespace-only input with Enter.<br>3. Observe list/count. | No new todo is created for either submission; list and counters remain unchanged. | Medium | Negative input omission case. |
| TC-023 | Edit todo save on Enter and blur, cancel on Escape, delete on empty | R10 | At least one existing todo item available. | Replacement text A, replacement text B, empty edited text. | 1. Start editing existing todo.<br>2. Change text to A and press Enter.<br>3. Start editing same or another todo, change text to B, click/blur outside.<br>4. Start editing again, type different text, press Escape.<br>5. Start editing again, clear text to empty, commit edit. | Enter save persists text A; blur save persists text B; Escape cancels unsaved edit and original text remains; committing an empty edited title removes the item. | Medium | Consolidates all required edit-state rules. |
| TC-024 | Synchronize mark-all and clear-completed state | R11 | At least two todos exist in mixed active/completed state. | Todo set containing both active and completed items. | 1. Use mark-all control once.<br>2. Verify all items become completed and control reflects all-complete state.<br>3. Toggle one item back to active and verify mark-all state updates.<br>4. Use clear-completed.<br>5. Observe remaining items, counters, and control visibility/state. | Mark-all updates all item states consistently; changing one item updates aggregate state; clear-completed removes only completed items and updates remaining count and related UI state accordingly. | Medium | State synchronization edge case. |
| TC-025 | Filter todos by route and maintain selected state | R12 | Todo list contains at least one active and one completed item. | Routes `#/`, `#/active`, `#/completed`. | 1. Visit `#/` and note visible items and selected filter.
2. Switch to `#/active` and observe list.
3. Switch to `#/completed` and observe list.
4. Switch back to `#/`.
5. Observe selected filter state each time. | All route shows all items; active route shows only active items; completed route shows only completed items; selected filter indicator matches current route consistently after each switch. | Medium | Covers route state plus visible filtering. |

## Coverage Summary
| Requirement | Covered Cases | Boundary Covered | Negative Covered | Status |
| --- | --- | --- | --- | --- |
| R1 | TC-001, TC-002, TC-003 | yes | yes | full |
| R2 | TC-004, TC-005, TC-006 | yes | yes | full |
| R3 | TC-007, TC-008, TC-009 | no | yes | full |
| R4 | TC-010, TC-011 | yes | yes | full |
| R5 | TC-012, TC-013, TC-014 | yes | yes | full |
| R6 | TC-015, TC-016 | no | yes | full |
| R7 | TC-017, TC-018 | yes | yes | full |
| R8 | TC-019, TC-020 | yes | yes | full |
| R9 | TC-021, TC-022 | yes | yes | full |
| R10 | TC-023 | yes | yes | full |
| R11 | TC-024 | yes | no | full |
| R12 | TC-025 | no | no | full |

## Ambiguities / Missing Information / Assumptions
- Ambiguity: The exact success status code for some API create/delete operations is not fixed in the input pack, so the suite asserts successful behavior and observable effects rather than a single implementation-specific code where not explicitly stated.
- Ambiguity: The exact response schema for validation and conflict errors is not fully specified beyond the expected error class (`401`, `403`, `404`, `422`, `409` or equivalent validation/conflict outcome in some cases).
- Ambiguity: For `R7`, the requirement states valid pagination boundaries, but does not specify whether invalid boundaries must return `422` or another validation-style rejection; the test expects rejection rather than a silent accept.
- Missing info: The input pack does not define exact TodoMVC selectors, accessibility labels, or DOM hooks, so UI cases are expressed in behavior-first terms suitable for later Playwright mapping.
- Missing info: The input pack does not define whether comment deletion returns an empty body, a confirmation object, or only a status code.
- Assumption: Protected API endpoints use a standard authorization mechanism where omission of a valid token produces unauthorized behavior.
- Assumption: Supported `PUT /user` fields include at least the user-visible profile fields commonly exposed by the API, and the implementation ignores unsupported fields or rejects them without violating persistence checks for supported ones.
- Assumption: TodoMVC route changes are directly observable through URL hash and selected filter state, and item counts/visibility are externally visible to the user.
