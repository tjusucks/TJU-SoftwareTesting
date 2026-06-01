# White-Box Modeling Report

## 1. Run Metadata

| Field             | Value                                                                                                                                                                                                                                                                                                     |
| ----------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Project Name      | RealWorld / Conduit                                                                                                                                                                                                                                                                                       |
| Feature Name      | Article Creation, Editing, and Deletion                                                                                                                                                                                                                                                                   |
| Feature ID        | article-lifecycle                                                                                                                                                                                                                                                                                         |
| Run ID            | A2-WB-ARTICLE-LIFECYCLE-R0                                                                                                                                                                                                                                                                                |
| Date              | 2026-06-01                                                                                                                                                                                                                                                                                                |
| Author / Operator | Claude Code / white-box-modeling skill                                                                                                                                                                                                                                                                    |
| Skill / Tool Name | `white-box-modeling`                                                                                                                                                                                                                                                                                      |
| Prompt Version    | v1.0                                                                                                                                                                                                                                                                                                      |
| Input Mode        | standalone                                                                                                                                                                                                                                                                                                |
| Input Source(s)   | `Assignment 02/outputs/structured-requirements/article-lifecycle.json`; `Assignment 01/codebases/realworld/implementations/golang-gin/`; `Assignment 01/codebases/realworld/specification/features/article-lifecycle.md`; `Assignment 02/outputs/black-box-design/final/article-lifecycle/artifacts.json` |
| Revision          | 0                                                                                                                                                                                                                                                                                                         |
| Notes             | No executable tests generated. Source code was available, so the model is source-backed rather than purely logical.                                                                                                                                                                                       |

## 2. Scope and Model Selection

- Target module: RealWorld / Conduit article lifecycle in the Go Gin implementation.
- Primary source boundaries:
  - Routing and auth registration: `Assignment 01/codebases/realworld/implementations/golang-gin/hello.go:41`-`52`
  - Auth middleware: `Assignment 01/codebases/realworld/implementations/golang-gin/users/middlewares.go:43`-`74`
  - Article route handlers: `Assignment 01/codebases/realworld/implementations/golang-gin/articles/routers.go:38`-`147`
  - Article persistence/model helpers: `Assignment 01/codebases/realworld/implementations/golang-gin/articles/models.go:150`-`361`
  - Article validation: `Assignment 01/codebases/realworld/implementations/golang-gin/articles/validators.go:10`-`49`
  - Article serialization: `Assignment 01/codebases/realworld/implementations/golang-gin/articles/serializers.go:48`-`90`
- Model type: State Transition.
- Rationale for model choice:
  - The feature is explicitly lifecycle-oriented: no article -> active article -> updated active article -> deleted/unavailable article.
  - The source has several guard-driven transitions around this lifecycle: authentication, validation, article existence, ownership, tag-list handling, and deletion observability.
  - A state transition model captures both persisted article states and rejection/no-mutation states while remaining compact enough for traceable white-box paths.
- In-scope functions or endpoints:
  - `POST /api/articles` -> `ArticleCreate`
  - `PUT /api/articles/:slug` -> `ArticleUpdate`
  - `DELETE /api/articles/:slug` -> `ArticleDelete`
  - `GET /api/articles/:slug` -> `ArticleRetrieve` for post-update/post-delete observation
  - `GET /api/articles` -> `ArticleList` / `FindManyArticle` for post-delete list observability
  - `AuthMiddleware(true)` for protected create/update/delete routes
- Out-of-scope items:
  - Favorites, comments, feeds, follows, and profile behavior except where they share article serialization.
  - Non-deterministic database error branches that require explicit fault injection, such as `SaveOne`, `model.Update`, `DeleteArticleModel`, and transaction commit failures.
  - Generating executable tests.

## 3. Internal Model

### 3.1 States (state model)

| State ID | Name                        | Description                                                                                                                                       | Entry Conditions                                                        | Exit Conditions                                                                                             |
| -------- | --------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| S0       | ArticleAbsent               | No active article exists for the target slug. This is the initial state before create and the observable state after deletion for normal queries. | Before successful create; after deletion has made the slug unavailable. | Valid authenticated create; missing-slug update/delete edge paths.                                          |
| S1       | ArticleActiveOriginal       | A persisted active article exists with initial title, description, body, author, slug, timestamps, and optional tag associations.                 | Successful create through `ArticleCreate` and `SaveOne`.                | Owner update; owner delete; rejected update/delete attempts.                                                |
| S2       | ArticleActiveContentUpdated | The owner has updated one or more persisted content fields. Omitted fields are preserved by validator prefill.                                    | Successful owner update through `ArticleUpdate` and `model.Update`.     | Further update; delete; readback verification.                                                              |
| S3       | ArticleActiveTagsUpdated    | The active article's tag associations have been replaced or cleared by update payload `tagList` handling.                                         | Successful owner update with `tagList: []` or non-empty `tagList`.      | Further update; delete; readback verification.                                                              |
| S4       | ArticleDeletedUnavailable   | The article has been deleted through GORM `Delete` and is no longer returned by normal detail/list queries.                                       | Successful owner delete through `ArticleDelete`.                        | Detail/list observation remains unavailable; recreate with a new request is modeled separately as S0 -> S1. |
| S5       | RequestRejectedNoMutation   | A request is rejected by auth, validation, ownership, or existence checks before mutating article state.                                          | Any guarded negative path.                                              | Return to the prior persisted state for subsequent requests.                                                |

### 3.2 Transitions (state model)

| Transition ID | From | To  | Guard / Condition                                                                                                    | Action                                                                                                                                                                                                                                          | Requirement ID(s) |
| ------------- | ---- | --- | -------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------- |
| T1            | S0   | S5  | `POST /api/articles` without a valid token reaches the protected route group.                                        | `AuthMiddleware(true)` aborts with 401 before `ArticleCreate`; no row is saved. Evidence: `hello.go:48`-`52`, `users/middlewares.go:48`-`51`.                                                                                                   | R1                |
| T2            | S0   | S5  | Authenticated create payload is missing required fields, has title shorter than `min=4`, or violates max sizes.      | `ArticleModelValidator.Bind` fails; `ArticleCreate` returns 422 and skips `SaveOne`. Evidence: `validators.go:10`-`18`, `routers.go:38`-`48`.                                                                                                   | R4                |
| T3            | S0   | S1  | Authenticated create payload has valid title, description, body, and omitted or empty `tagList`.                     | `Bind` generates slug, assigns author, `setTags` uses the zero-tag branch, `SaveOne` persists article, serializer returns 201 fields. Evidence: `validators.go:35`-`48`, `models.go:310`-`314`, `routers.go:46`-`51`, `serializers.go:67`-`90`. | R2, R3            |
| T4            | S0   | S1  | Authenticated create payload has valid fields and non-empty `tagList`.                                               | `setTags` fetches/creates tags, assigns associations, `SaveOne` persists, serializer sorts `tagList`. Evidence: `models.go:316`-`350`, `serializers.go:83`-`89`.                                                                                | R2, R3            |
| T5            | S0   | S5  | `PUT /api/articles/{slug}` uses a slug with no active article.                                                       | `ArticleUpdate` returns 404 from `FindOneArticle` before ownership, binding, or update. Evidence: `routers.go:99`-`105`.                                                                                                                        | —                 |
| T6            | S1   | S5  | PUT target exists, but authenticated requester is not the article author.                                            | `AuthorID` is compared with `GetArticleUserModel(currentUser).ID`; mismatch returns 403 and skips update. Evidence: `routers.go:106`-`112`.                                                                                                     | R7                |
| T7            | S1   | S5  | PUT target exists and requester is owner, but update payload violates validator constraints.                         | Existing fields/tags are prefilled, `Bind` fails, handler returns 422 and skips `model.Update`. Evidence: `validators.go:24`-`32`, `routers.go:114`-`123`.                                                                                      | R4                |
| T8            | S1   | S2  | PUT target exists, requester is owner, and payload supplies a valid partial content update while omitting `tagList`. | Prefill preserves omitted fields and tags; `model.Update` applies supplied changes and serializer returns updated article. Evidence: `validators.go:24`-`32`, `routers.go:120`-`126`, `models.go:352`-`356`.                                    | R5, R6, R11       |
| T9            | S1   | S2  | PUT target exists, requester is owner, and payload supplies a valid replacement title.                               | `Bind` regenerates slug with `slug.Make(new title)`; `model.Update` stores new title/slug. Evidence: `validators.go:42`-`47`.                                                                                                                   | R5, R11           |
| T10           | S1   | S3  | PUT target exists, requester is owner, and payload explicitly contains `tagList: []`.                                | `setTags` assigns an empty tag slice and `model.Update` clears tag associations. Evidence: `models.go:310`-`314`.                                                                                                                               | R5, R6, R11       |
| T11           | S1   | S3  | PUT target exists, requester is owner, and payload contains non-empty `tagList`.                                     | `setTags` fetches existing tags, creates missing tags, and replaces article tag associations. Evidence: `models.go:316`-`350`.                                                                                                                  | R5, R6, R11       |
| T12           | S1   | S5  | DELETE target exists, but authenticated requester is not the article author.                                         | `ArticleDelete` returns 403 after ownership check and skips `DeleteArticleModel`. Evidence: `routers.go:129`-`139`.                                                                                                                             | R10               |
| T13           | S1   | S4  | DELETE target exists and requester is the article author.                                                            | `ArticleDelete` passes ownership, calls `DeleteArticleModel`, and returns 200 delete success. Evidence: `routers.go:129`-`147`, `models.go:358`-`361`.                                                                                          | R8, R9, R12       |
| T14           | S4   | S4  | `GET /api/articles/{slug}` is requested for a deleted article slug.                                                  | `FindOneArticle` cannot find an active row and `ArticleRetrieve` returns 404. Evidence: `routers.go:88`-`97`, `models.go:150`-`155`.                                                                                                            | R9, R12           |
| T15           | S4   | S4  | `GET /api/articles` or filtered list is requested after deletion.                                                    | `FindManyArticle` uses normal GORM queries/associations, excluding deleted rows from active list results. Evidence: `routers.go:54`-`68`, `models.go:177`-`264`.                                                                                | R12               |
| T16           | S0   | S0  | `DELETE /api/articles/{slug}` uses a slug with no active article.                                                    | Source treats delete as idempotent: it skips ownership check when `FindOneArticle` fails, calls `DeleteArticleModel`, and returns 200 if no DB error. Evidence: `routers.go:129`-`147`.                                                         | —                 |
| T17           | S1   | S5  | `PUT /api/articles/{slug}` without a valid token reaches the protected route group.                                  | `AuthMiddleware(true)` aborts with 401 before `ArticleUpdate`; article remains unchanged. Evidence: `hello.go:48`-`52`, `users/middlewares.go:48`-`68`.                                                                                         | R7                |
| T18           | S1   | S5  | `DELETE /api/articles/{slug}` without a valid token reaches the protected route group.                               | `AuthMiddleware(true)` aborts with 401 before `ArticleDelete`; article remains active. Evidence: `hello.go:48`-`52`, `users/middlewares.go:48`-`68`.                                                                                            | R10               |

### 3.3 Nodes and Edges (if path/control-flow model)

Not applicable. This run selected a state transition model.

## 4. Coverage Strategy

| Coverage Target | Strategy                                                                                                                                                                        | Notes                                                                                                                                       |
| --------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| State           | Cover every modeled state at least once.                                                                                                                                        | S0-S5 are all reached by the selected paths.                                                                                                |
| Transition      | Cover every modeled source-backed transition at least once.                                                                                                                     | T1-T18 are all covered.                                                                                                                     |
| Branch/Decision | Cover deterministic lifecycle decisions: auth pass/fail, validator pass/fail, article existence, owner/non-owner, tag omitted/empty/non-empty, deleted detail/list observation. | Database error branches are documented but excluded from the minimal suite because they need fault injection rather than normal API inputs. |
| Path            | Select the smallest practical paths that cover all lifecycle transitions while combining compatible negative checks.                                                            | 12 paths cover 18 transitions; non-owner update/delete and unauthenticated update/delete are paired to reduce setup duplication.            |

Path optimization notes:

- Positive create paths are split into no-tags and with-tags because `setTags` has materially different internal behavior for `len(tags)==0` versus non-empty tags.
- Owner update paths are split into partial content, title/slug regeneration with tag replacement, and clear-tags because each exercises a different internal branch.
- Non-owner update/delete and unauthenticated update/delete are combined because each pair shares setup and expected no-mutation effects.
- Post-delete detail and list observations are combined with the owner delete path because they validate the same transition into `ArticleDeletedUnavailable`.

## 5. White-Box Paths

| Path ID    | Type                             | Steps             | Coverage Focus                                                      | Requirement ID(s) | Test Intent                                                              | Expected Internal Effect                                                                                          |
| ---------- | -------------------------------- | ----------------- | ------------------------------------------------------------------- | ----------------- | ------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------- |
| WB-PATH-01 | Negative auth create             | T1                | Protected create route; middleware-before-handler rejection         | R1                | Confirm create is blocked before validation/persistence without a token. | Context user remains zero, `ArticleCreate` does not run, no `ArticleModel` row inserted.                          |
| WB-PATH-02 | Negative validation create       | T2                | Create validator failure; `SaveOne` skipped                         | R4                | Submit missing/invalid required create fields.                           | `Bind` returns validation error, 422 response, article count/lookup unchanged.                                    |
| WB-PATH-03 | Happy create without tags        | T3                | Create success; zero-tag branch; response fields                    | R2, R3            | Create with required fields and omitted or empty `tagList`.              | Slug generated, author associated, article persisted, empty tags serialized with article metadata.                |
| WB-PATH-04 | Happy create with tags           | T4                | Non-empty tag branch; tag fetch/create; sorted serialization        | R2, R3            | Create with non-empty `tagList`.                                         | Tags are reused/created, associations persisted, response includes sorted `tagList`.                              |
| WB-PATH-05 | Owner partial update             | T8                | Find success; owner pass; prefilled partial update; persistence     | R5, R6, R11       | Update only one content field while omitting other fields and tags.      | Omitted fields/tags preserved, supplied field updated, later retrieve returns persisted change.                   |
| WB-PATH-06 | Owner title and tag replacement  | T9 -> T11         | Slug regeneration; non-empty tag replacement; current-slug readback | R5, R6, R11       | Update title and replace tags.                                           | Slug changes to `slug.Make(new title)`, tag associations replaced, follow-up read should use returned slug.       |
| WB-PATH-07 | Owner clear tags                 | T10               | Explicit empty `tagList` branch; association clear                  | R5, R6, R11       | Update tagged article with `tagList: []`.                                | Tags slice becomes empty; content and author remain intact.                                                       |
| WB-PATH-08 | Negative owner update validation | T7                | Update validator fail after owner check; update skipped             | R4                | Owner submits invalid update payload.                                    | Article is found and ownership passes, but `Bind` fails; `model.Update` is skipped and article remains unchanged. |
| WB-PATH-09 | Negative non-owner write         | T6 -> T12         | Update/delete owner mismatch; no mutation                           | R7, R10           | Authenticated non-owner attempts update and delete.                      | Both return 403, skip mutation, original article remains active and unchanged.                                    |
| WB-PATH-10 | Negative unauthenticated write   | T17 -> T18        | Update/delete auth rejection before handlers                        | R7, R10           | Missing token on update and delete.                                      | Middleware returns 401 before `ArticleUpdate`/`ArticleDelete`; article remains active.                            |
| WB-PATH-11 | Owner delete and observe         | T13 -> T14 -> T15 | Delete success; post-delete detail not-found; list exclusion        | R8, R9, R12       | Owner deletes existing article and verifies unavailability.              | `DeleteArticleModel` deletes the row by slug; normal detail/list queries do not return it.                        |
| WB-PATH-12 | Missing-slug edge cases          | T5 -> T16         | Update not-found; delete idempotent no-op                           | —                 | Cover source-supported missing slug behavior.                            | Update returns 404 before mutation; delete skips owner check and returns 200 if DB delete no-op succeeds.         |

Black-box handoff linkage:

| White-Box Path | Useful Existing Black-Box Coverage                                                    | Relationship                                                                                     |
| -------------- | ------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| WB-PATH-01     | TC-001                                                                                | Same unauthenticated create rejection intent.                                                    |
| WB-PATH-02     | TC-005, TC-006, TC-007                                                                | Same invalid create-field partitions; white-box path emphasizes `Bind` failure before `SaveOne`. |
| WB-PATH-03     | TC-002, TC-003                                                                        | Same successful create/response contract; white-box path adds zero-tag branch coverage.          |
| WB-PATH-04     | TC-002, TC-003 and article feature spec tag cases                                     | Complements black-box create success by explicitly covering tag association internals.           |
| WB-PATH-05     | TC-008, TC-009, TC-015                                                                | Same owner partial update and persistence intent.                                                |
| WB-PATH-06     | TC-013, TC-015                                                                        | Complements tag replacement and update persistence; adds slug regeneration readback risk.        |
| WB-PATH-07     | TC-011                                                                                | Same empty tag-list clear-tags behavior.                                                         |
| WB-PATH-08     | TC-012 for invalid tagList intent, plus create invalid cases TC-005-TC-007 by analogy | White-box path covers invalid update after ownership succeeds.                                   |
| WB-PATH-09     | No direct non-owner black-box case in the handoff                                     | Fills an ownership branch gap for R7/R10.                                                        |
| WB-PATH-10     | TC-017, TC-018                                                                        | Same unauthenticated update/delete rejection intent.                                             |
| WB-PATH-11     | TC-014, TC-016                                                                        | Same owner delete and post-delete retrieval failure; white-box path adds list exclusion.         |
| WB-PATH-12     | No direct black-box case                                                              | Documents implementation-specific missing-slug update/delete behavior.                           |

Note: The structured requirements file for this run uses R1-R12, while the black-box artifact uses an older R1-R16 numbering. The handoff links above are therefore based on black-box test case title/intent rather than relying only on matching requirement IDs.

## 6. Traceability

| Requirement ID                                  | Related States/Nodes | Related Transition/Edge | Path ID(s)                         | Coverage Status                                                                                                                                     |
| ----------------------------------------------- | -------------------- | ----------------------- | ---------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| R1                                              | S0, S5               | T1                      | WB-PATH-01                         | Full                                                                                                                                                |
| R2                                              | S0, S1               | T3, T4                  | WB-PATH-03, WB-PATH-04             | Full                                                                                                                                                |
| R3                                              | S1                   | T3, T4                  | WB-PATH-03, WB-PATH-04             | Full                                                                                                                                                |
| R4                                              | S0, S1, S5           | T2, T7                  | WB-PATH-02, WB-PATH-08             | Full for source validation branches; whitespace-only behavior remains unspecified.                                                                  |
| R5                                              | S1, S2, S3           | T8, T9, T10, T11        | WB-PATH-05, WB-PATH-06, WB-PATH-07 | Full                                                                                                                                                |
| R6                                              | S1, S2, S3           | T8, T10, T11            | WB-PATH-05, WB-PATH-06, WB-PATH-07 | Full                                                                                                                                                |
| R7                                              | S1, S5               | T6, T17                 | WB-PATH-09, WB-PATH-10             | Full                                                                                                                                                |
| R8                                              | S1, S4               | T13                     | WB-PATH-11                         | Full                                                                                                                                                |
| R9                                              | S4                   | T13, T14                | WB-PATH-11                         | Full                                                                                                                                                |
| R10                                             | S1, S5               | T12, T18                | WB-PATH-09, WB-PATH-10             | Full                                                                                                                                                |
| R11                                             | S1, S2, S3           | T8, T9, T10, T11        | WB-PATH-05, WB-PATH-06, WB-PATH-07 | Full                                                                                                                                                |
| R12                                             | S4                   | T13, T14, T15           | WB-PATH-11                         | Full for detail/global-list observability; favorited-filter deletion observability would require a favorite setup outside the core lifecycle model. |
| Implementation edge: missing update/delete slug | S0, S5               | T5, T16                 | WB-PATH-12                         | Covered as source-supported behavior, not requirement-driven.                                                                                       |

## 7. Coverage Summary and Metrics

### 7.1 Coverage Summary

- Strongest internal coverage area: CRUD lifecycle guards and state mutations are covered end-to-end: auth gate, create validation, tag creation/clearing/replacement, owner update, ownership rejection, owner delete, and post-delete detail/list observability.
- Weakest internal coverage area: database error branches are intentionally excluded from the minimal suite because they require fault injection or mocked persistence failures rather than normal lifecycle input data.
- Under-covered decisions:
  - `SaveOne` database error branch in `ArticleCreate` (`routers.go:46`-`49`).
  - `model.Update` database error branch in `ArticleUpdate` (`routers.go:121`-`124`).
  - `DeleteArticleModel` database error branch in `ArticleDelete` (`routers.go:142`-`145`).
  - Transaction/association error branches in list/filter helpers (`models.go:192`-`263`).

### 7.2 Metrics

| Metric              | Formula                                                                                 | Value                                                                                                                                  |
| ------------------- | --------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| State Coverage      | covered_states / total_states                                                           | 6 / 6 = 100%                                                                                                                           |
| Transition Coverage | covered_transitions / total_transitions                                                 | 18 / 18 = 100%                                                                                                                         |
| Decision Coverage   | selected deterministic branch outcomes covered / selected deterministic branch outcomes | 24 / 24 selected outcomes = 100%; reported practical branch coverage = 85.7% when excluded DB-error outcomes are counted as known gaps |
| Path Coverage       | selected_paths_exercised / selected_paths_defined                                       | 12 / 12 = 100%                                                                                                                         |

## 8. Assumptions / Missing Information / Open Questions

### 8.1 Assumptions

- The model is source-backed by the Go Gin implementation under `Assignment 01/codebases/realworld/implementations/golang-gin`; it is not a source-missing logical model.
- Post-delete unavailability relies on GORM default scoped queries for models embedding `gorm.Model`, so soft-deleted rows are excluded from normal `FindOneArticle` and `FindManyArticle` queries.

### 8.2 Missing Information

- The structured requirements do not explicitly define behavior for update/delete on a missing slug. Source behavior is modeled separately in WB-PATH-12.
- Exact whitespace-only validation semantics are not specified. The current validator uses Gin binding tags such as `required` and `min=4` for title.
- The source code regenerates slug from title but does not add a unique suffix. This may conflict with the feature-spec/black-box duplicate title expectation if two identical titles are created.

### 8.3 Open Questions

- Should duplicate-title slug uniqueness be part of the white-box lifecycle model, or tracked as a separate defect/risk? The code uses `slug.Make(title)` and `Slug` has a unique index, so duplicate titles may fail persistence rather than receive unique slugs.
- Should idempotent delete of a missing slug returning 200 be accepted as intended behavior? It is implemented but not requirement-driven.
- Should R12 require favorited-filter list deletion observability in white-box scope? That would require favorite setup, which is outside the core create/update/delete module boundary.

## 9. Review and Revision Log

| Revision | Source File | Change Summary                                                                                                    | Impacted IDs                       | Outcome   |
| -------- | ----------- | ----------------------------------------------------------------------------------------------------------------- | ---------------------------------- | --------- |
| 0        | N/A         | Initial standalone white-box model generated from requirements, source code, feature spec, and black-box handoff. | All states, transitions, and paths | Completed |

### 9.1 Before/After Evidence

- Added: Initial `report.md` and `artifacts.json`.
- Modified: N/A.
- Removed: N/A.

## 10. Optional Executable Tests

- Generated executable tests: No
- Test framework: N/A
- Test output path: N/A
- Related white-box path IDs: WB-PATH-01 through WB-PATH-12
- Notes: The user explicitly requested no executable test code in this run.

## 11. Export Index

- `artifacts.json`: Machine-readable state model, transitions, selected paths, metrics, assumptions, and open questions.
- `tests/`: Not generated.
- `revision_log.json`: Not generated because no designer revision input was supplied.
