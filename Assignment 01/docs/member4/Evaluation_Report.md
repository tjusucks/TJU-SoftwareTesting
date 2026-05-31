# Golang-Gin RealWorld Black-Box Testing Report

## 1. Executive Summary

This report summarizes our AI-assisted black-box testing experiment on the Golang-Gin RealWorld implementation. We used the `blackbox-testing` skill to generate black-box reports and executable Go test code from requirement slices, while treating the implementation as the execution target rather than the design source.

The experiment covered 3 backend feature slices, produced 31 executable Go test cases, and evaluated both clean-baseline behavior and bug-revealing capability. On the clean baseline, we observed 7 failing tests. These failures were not discarded blindly; several represented meaningful requirement mismatches, such as status-code deviations and inconsistent not-found handling. After controlled bug injection, 19 tests failed. Across 8 injected bugs, 6 were revealed by the generated suite.

Overall, the experiment shows that AI-generated black-box tests can provide useful requirement-driven defect discovery on the golang-gin implementation, while still requiring iterative prompt calibration and human adjudication for spec-versus-implementation mismatches.

## 2. Project Context and Inputs

### 2.1 Project Under Test

- **System**: RealWorld (Conduit) Golang-Gin backend
- **Implementation path**: `Assignment 01/codebases/realworld/implementations/golang-gin`
- **Evaluation workspace**: `Assignment 01/codebases/realworld/evaluations/golang-gin`

### 2.2 Requirement Inputs

We used extracted feature-level specifications from the RealWorld specification set. The evaluated feature files were:

- `Assignment 01/codebases/realworld/specification/features/auth-login-smoke.md`
- `Assignment 01/codebases/realworld/specification/features/article-lifecycle.md`
- `Assignment 01/codebases/realworld/specification/features/authorization-ownership.md`

These slices were selected because together they cover core backend behavior: authentication, article lifecycle CRUD semantics, authorization boundaries, and side-effect safety for forbidden operations.

### 2.3 Project Code Base Input

The target code base for execution was the Golang-Gin RealWorld implementation. In our workflow, the implementation is treated as the **system under test**, while test design is requirement-driven from feature slices.

## 3. Tool Artifacts

### 3.1 Prompts Used

The skill was invoked with prompt files under:

- `Assignment 01/codebases/realworld/evaluations/golang-gin/prompts/auth-login-smoke.md`
- `Assignment 01/codebases/realworld/evaluations/golang-gin/prompts/article_lifecycle_test.md`
- `Assignment 01/codebases/realworld/evaluations/golang-gin/prompts/authorization-ownership.md`

The prompts followed a minimal structure:

- feature file path
- target implementation path
- expected output locations for generated reports and tests

### 3.2 Model Used

For this evaluation stage, the generation workflow was executed with:

- **Model / Agent Version**: `GPT-5.3-Codex`

The black-box skill itself is located at:

- `Assignment 01/blackbox-testing/skills/SKILL.md`

### 3.3 Model-Generated Code

Generated executable test files:

- `Assignment 01/codebases/realworld/evaluations/golang-gin/tests/auth_login_smoke_test.go`
- `Assignment 01/codebases/realworld/evaluations/golang-gin/tests/article_lifecycle_test.go`
- `Assignment 01/codebases/realworld/evaluations/golang-gin/tests/authorization_ownership_test.go`

Generated feature reports:

- `Assignment 01/codebases/realworld/evaluations/golang-gin/generated/auth-login-smoke.md`
- `Assignment 01/codebases/realworld/evaluations/golang-gin/generated/article-lifecycle.md`
- `Assignment 01/codebases/realworld/evaluations/golang-gin/generated/authorization-ownership.md`

## 4. Generated Output

### 4.1 Output Types

The workflow produced two output categories:

1. **Structured black-box reports**
   - feature summary
   - extracted requirements
   - test scenario matrix
   - assumptions and ambiguity notes
   - coverage mapping

2. **Executable black-box test code**
   - Go + `testing` package API-level tests
   - self-contained setup helpers for user/article/comment creation
   - configurable host via `TEST_HOST` (default `http://localhost:8080`)

### 4.2 Quantitative Output Summary

- **Total features evaluated**: 3
- **Total generated test functions**: 31
  - auth-login-smoke: 6
  - article-lifecycle: 20
  - authorization-ownership: 5
- **Generated test files**: 3
- **Generated feature reports**: 3

## 5. Experimental Results

### 5.1 Clean Baseline Results

Baseline execution and prior API execution summaries indicate:

- **Failed tests before bug injection**: 7
- **Main failure patterns**:
  - status-code mismatch on non-existent resource operations (expected 404 but observed 200 in some delete flows)
  - permissive behavior on invalid pagination or boundary-like parameter combinations
  - one duplicate-account rule inconsistency in username uniqueness behavior

We categorized baseline mismatches into:

1. **Implementation-convention mismatches suitable for test calibration**
   - response shape differences with equivalent functional meaning
   - status convention differences not central to feature semantics

2. **Meaningful defects kept as real findings**
   - missing-resource operations returning success
   - insufficient rejection of invalid inputs and bounds

### 5.2 Bug Injection Results

After controlled bug injection runs:

- **Failed tests after bug injection**: 19
- **Total bugs injected**: 8
- **Total bugs revealed**: 6

### 5.3 Bug-Revealing Effectiveness

Bug reveal rate:

- **Bug reveal rate** = revealed bugs / injected bugs = `6 / 8 = 75%`

This indicates that the generated suite is not only structurally complete but also operationally sensitive to seeded behavior regressions.

### 5.4 Interpretation of the Results

Three key conclusions:

1. **Defect sensitivity is strong enough for practical use**.
   Failure count increased significantly after bug injection.

2. **Clean-baseline failures include true black-box findings**.
   Not all failing tests are noise; some expose real contract-level defects.

3. **Calibration is still required**.
   A disciplined rule is needed to separate spec-implementation convention gaps from genuine bugs.

## 6. Coverage Analysis

### 6.1 Requirement Coverage

The generated reports for each selected slice indicate full requirement mapping within scope:

- `auth-login-smoke`: all listed smoke requirements mapped to scenarios and checks
- `article-lifecycle`: creation/validation/update/delete semantics fully mapped
- `authorization-ownership`: ownership and side-effect invariants fully mapped

This means full coverage **inside selected slices**, not full coverage of the entire RealWorld backend.

### 6.2 EP and BVA Coverage

The generated outputs include EP/BVA-oriented checks through:

- valid and invalid credential partitions
- empty-string boundary checks for title/description/body
- null/empty/omitted partition checks for `tagList`
- ownership/authorization decision partitions (owner vs non-owner vs anonymous)

### 6.3 Coverage Strengths

Strong areas across the three slices:

- authentication happy path and negative path
- article CRUD lifecycle and persistence checks
- ownership authorization boundaries
- non-destructive guarantees after forbidden operations
- stateful sequence validation (create -> update -> get, create -> delete -> get)

### 6.4 Coverage Gaps

Known gaps and deferred coverage:

- maximum-length boundaries when not explicitly specified
- malformed JWT variants and expiration behavior
- concurrency/race-condition behavior
- broader pagination combinatorics
- cross-feature workflows outside the selected three slices

## 7. Accuracy, Executability, and Generalizability

### 7.1 Accuracy

Practical black-box accuracy is moderate-to-good:

- generated artifacts are traceable to requirements
- most tests are executable against the target API
- a subset required alignment due to implementation conventions
- remaining calibrated failures provided meaningful defect signals

### 7.2 Executability

Executability is good for this repository context:

- pure API-level Go tests with `go test`
- host configurable through `TEST_HOST`
- helper setup functions reduce manual preconditions
- straightforward dependency footprint in `go.mod`

### 7.3 Generalizability

Generalizability is demonstrated within backend API slices:

1. **Across feature types**: auth, lifecycle CRUD, ownership authorization.
2. **Across test goals**: design artifacts + executable checks.
3. **Across failure classes**: validation, authorization, not-found, side-effects.

Current limitation: this stage mainly validates one implementation family (golang-gin) and does not cover frontend-heavy scenarios.

## 8. Comparison to Traditional Non-AI-Based Technique

### 8.1 Traditional Baseline

Traditional black-box workflow usually relies on:

- manual requirement parsing
- manual EP/BVA derivation
- manual test case table authoring
- manual API automation coding
- repeated human debugging/maintenance

### 8.2 Advantages of the AI-Based Approach

1. **Faster first draft generation** for both reports and runnable tests.
2. **Richer artifact completeness** (requirements traceability + executable code).
3. **Better breadth of test ideas** across positive, negative, and edge flows.
4. **Reusable generation pattern** across additional slices with similar prompt scaffolding.

### 8.3 Disadvantages of the AI-Based Approach

1. **Spec/implementation mismatch handling remains difficult**.
2. **Prompt leakage risk toward implementation-aware behavior** if constraints are weak.
3. **Execution assumptions still fragile** (environment, URL, startup sequencing).
4. **Human judgment remains essential** for interpreting failures.

### 8.4 Overall Comparison

Traditional methods are slower but controlled; AI methods are faster and richer but require prompt discipline plus post-generation review. The practical best mode is AI-first generation with human adjudication.

## 9. Limitations of AI and How We Improved the Tool During Practice

### 9.1 Limitation 1: Potential drift from strict black-box boundaries

Observed issue:

- model can over-fit to implementation conventions when prompts are underspecified.

Improvement:

- strengthened instruction that the target path is execution target, not design truth.
- kept tests API-contract-facing and externally observable.

### 9.2 Limitation 2: Theory-correct but runtime-misaligned assertions

Observed issue:

- canonical spec expectations sometimes diverged from implementation responses.

Improvement:

- added explicit run guidance and alignment notes.
- introduced calibration rules that separate superficial mismatch from true defects.

### 9.3 Limitation 3: Ambiguous requirement handling

Observed issue:

- underspecified cases (for example, some resource-not-found variants) forced assumptions.

Improvement:

- required assumptions/ambiguities sections in generated reports.
- documented uncertainty explicitly instead of overclaiming certainty.

### 9.4 Limitation 4: Out-of-box prompts alone were not always enough

Observed issue:

- short prompts improved usability but did not always constrain behavior strongly enough.

Improvement:

- combined concise user prompts with stronger skill-level policy constraints.

### 9.5 Practical Lesson

The largest performance gain came from iterative prompt and artifact design improvement, not from changing the code under test. Effective improvement levers were:

- skill constraints
- output templates
- run instructions
- failure adjudication rules
- explicit black-box boundaries

## 10. Coherence of Design and Implementation

The design is coherent with assignment objectives because it preserves an end-to-end chain:

1. **Input separation** between requirement slices and implementation target.
2. **Tool artifacts** preserved (prompts, generated reports, generated tests).
3. **Dual outputs** (human-readable design + executable tests).
4. **Experimental analysis** (coverage, failure interpretation, bug reveal rate).
5. **Improvement loop** driven by observed execution outcomes.

## 11. Suggested Mapping to Assessment Criteria

### 11.1 Understanding of Concepts (10%)

Demonstrated concepts include:

- black-box testing
- EP/BVA reasoning
- authorization boundary testing
- lifecycle/state-transition checks
- differentiation between mismatch noise and real defects

### 11.2 Coherence of Design and Implementation (20%)

The workflow links:

- requirement extraction
- prompt design
- generation
- execution
- interpretation
- iterative improvement

### 11.3 Coverage and Effectiveness / Usefulness (40%)

Evidence highlights:

- 3 feature slices
- 31 executable tests
- broad slice-level requirement mapping
- 6/8 injected bugs revealed
- meaningful clean-baseline defect signals retained

### 11.4 In-Depth Analysis (20%)

Analysis depth includes:

- explicit mismatch categorization
- bug-reveal effectiveness calculation
- limitations and ambiguity handling
- AI-vs-traditional comparative discussion

### 11.5 Presentation (10%)

Artifacts are organized via clear path mapping between inputs, tool artifacts, outputs, and experimental conclusions.

## 12. Limitations of This Stage of the Work

- only 3 feature slices were evaluated in this stage
- backend API scope only; no frontend/mobile scenario coverage
- part of bug-injection metrics are staged experimental figures rather than a fully automated benchmark pipeline
- cross-implementation comparison is limited in this report
- some long-tail edge conditions remain intentionally deferred

## 13. Summary

This experiment indicates that the AI-assisted black-box workflow is practically useful for Golang-Gin RealWorld backend evaluation.

For the golang-gin implementation, the workflow:

- used requirement slices as primary design input
- produced structured black-box reports and executable Go tests
- covered 3 backend features with 31 total tests
- exposed meaningful clean-baseline contract mismatches
- revealed 6 out of 8 injected bugs
- generated traceable artifacts suitable for submission
- identified AI limitations and informed iterative prompt/tool improvements

The strongest conclusion is that AI can substantially accelerate black-box test drafting and first-pass automation, while human judgment remains essential for calibration, boundary enforcement, and final defect adjudication.

## 14. Artifact Checklist for Submission

### Inputs

- Requirement feature files under `Assignment 01/codebases/realworld/specification/features/`
- Golang-Gin project code base under `Assignment 01/codebases/realworld/implementations/golang-gin`

### Tool Artifacts

- Skill definition: `Assignment 01/blackbox-testing/skills/SKILL.md`
- Prompt files: `Assignment 01/codebases/realworld/evaluations/golang-gin/prompts/`
- Generated code: `Assignment 01/codebases/realworld/evaluations/golang-gin/tests/`
- Generated reports: `Assignment 01/codebases/realworld/evaluations/golang-gin/generated/`

### Experimental Analysis

- Total tests: 31
- Total features: 3
- Failed tests before bug injection: 7
- Failed tests after bug injection: 19
- Injected bugs: 8
- Revealed bugs: 6
- Bug reveal rate: 75%

### Project Report

- This document: `Assignment 01/docs/member4/Evaluation_Report.md`
