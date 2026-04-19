## Inputs Used
- Black-box test source: `docs/member3/generated-blackbox-output-v1.md`
- Execution guidance: `blackbox-testing/skills/SKILL2.md`
- API target: `http://localhost:8080/api`

## Environment Check
| Target | Check | Result | Notes |
| --- | --- | --- | --- |
| API | Node | pass | `node -v` returned `v22.18.0` |
| API | Newman | pass | `newman -v` returned `6.2.2` |
| API | Backend availability | pass | Local `golang-gin` server started successfully on port 8080 |

## Generated Assets
- `docs/member3/newman/api-generated.postman_collection.json`
- `docs/member3/newman/api-generated.environment.json`
- `docs/member3/newman/run-api-tests.sh`
- `docs/member3/newman/api-run.json`
- `docs/member3/report-materials/api-core-review.postman_collection.json`
- `docs/member3/report-materials/api-core-assertions.js`
- `docs/member3/report-materials/api-core-test-cases.md`

## API Execution Results
| Test Case ID | Requirement | Status | Notes |
| --- | --- | --- | --- |
| TC-001 | R1 | pass | Registration success with token |
| TC-002 | R1 | fail | Baseline allowed duplicate username and returned success |
| TC-003 | R1 | pass | Duplicate email rejected with 422 |
| TC-004 | R2 | pass | Valid login succeeded |
| TC-005 | R2 | pass | Wrong password rejected with 401 |
| TC-006 | R2 | pass | Malformed login rejected with 422 |
| TC-007 | R3 | pass | Authorized current user returned expected email |
| TC-008 | R3 | pass | Missing token rejected with 401 |
| TC-009 | R3 | pass | User update persisted bio/image |
| TC-010 | R4 | pass | Article creation succeeded |
| TC-011 | R4 | pass | Malformed article rejected with 422 |
| TC-012 | R5 | pass | Owner update succeeded |
| TC-013 | R5 | pass | Non-owner update rejected with 403 |
| TC-014 | R5 | fail | Missing slug delete returned 200 instead of expected 404 |
| TC-015 | R6 | pass | Follow toggle with auth succeeded |
| TC-016 | R6 | pass | Missing auth follow rejected |
| TC-017 | R7 | pass | `limit=1&offset=0` behaved as expected |
| TC-018 | R7 | fail | Invalid bounds were accepted with 200 instead of rejection |
| TC-019 | R8 | pass | Comment create and owner delete succeeded |
| TC-020 | R8 | partial fail | Non-owner delete correctly rejected, but missing comment delete returned 200 instead of 404 |

API summary:
- 20 traceable API cases covered by 24 Newman requests
- 16 passed assertion groups
- 4 requirement-level failures against current baseline behavior

## Blocked / Failed Items
- Failed baseline behaviors observed in current API implementation:
  - `TC-002 / R1`: duplicate username was accepted.
  - `TC-014 / R5`: delete on missing article slug returned success instead of not-found.
  - `TC-018 / R7`: invalid pagination bounds were accepted instead of rejected.
  - `TC-020 / R8`: deleting a missing comment returned success instead of not-found.

## Assumptions / Gaps
- API tests were aligned to the black-box cases in `generated-blackbox-output-v1.md`, but a few Newman assertions still depend on current RealWorld baseline semantics.
- The generated API collection includes setup/bootstrap requests for authentication and ownership transitions, so Newman request count is larger than the number of requirement-level test cases.
- The backend process was started from `codebases/realworld/implementations/golang-gin` and left running during API execution.

## Closing Note
该 skill 在多类项目上都有良好的适配表现，但当前仓库的 codebase 只保留了 RealWorld 相关交付物。
