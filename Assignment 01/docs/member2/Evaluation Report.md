# ASP.NET Core RealWorld Black-Box Testing Report

## 1. Executive Summary

This report summarizes our AI-assisted black-box testing experiment on the ASP.NET Core RealWorld implementation. We used the `blackbox-testing` Claude Code skill to generate black-box test reports and executable C# test code from requirement slices only, without relying on the target implementation as the design source. The experiment covered 5 backend feature slices, produced 86 generated test cases, and evaluated both clean-baseline behavior and bug-revealing capability.

At a high level, the generated suite achieved broad requirement coverage and successfully exposed meaningful defects. Before bug injection, 20 tests failed on the clean codebase; these failures were not discarded blindly, because they revealed real mismatches between the implementation and the expected black-box behavior, especially cases where invalid inputs produced `500 InternalServerError` instead of proper validation responses. After bug injection, 72 tests failed. Across 20 injected bugs, 14 were revealed by the generated suite.

This outcome shows that AI-generated black-box tests can provide useful requirement-driven defect discovery, but also that practical use requires iterative prompt refinement, explicit treatment of spec/implementation mismatches, and careful separation between black-box design and implementation-aware execution setup.

## 2. Project Context and Inputs

### 2.1 Project Under Test

- **System**: RealWorld (Conduit) ASP.NET Core backend
- **Implementation path**: `Assignment 01/codebases/realworld/implementations/aspnetcore`
- **Evaluation workspace**: `Assignment 01/codebases/realworld/evaluations/aspnetcore`

### 2.2 Requirement Inputs

We used extracted feature-level specifications from the RealWorld specification set. The evaluated feature files were:

- `Assignment 01/codebases/realworld/specification/features/auth-login-smoke.md`
- `Assignment 01/codebases/realworld/specification/features/article-lifecycle.md`
- `Assignment 01/codebases/realworld/specification/features/comment-lifecycle.md`
- `Assignment 01/codebases/realworld/specification/features/settings-null-fields.md`
- `Assignment 01/codebases/realworld/specification/features/authorization-ownership.md`

These feature slices were chosen because together they cover core backend behaviors: authentication, CRUD lifecycle, authorization, state persistence, null/empty handling, and nested-resource behavior.

### 2.3 Project Code Base Input

The target code base for execution was the ASP.NET Core RealWorld implementation. In our intended workflow, the target implementation is the **system under test**, not the source of truth for test design. The test design comes from the requirement slices; the implementation is only used as the execution target.

## 3. Tool Artifacts

### 3.1 Prompts Used

The skill was invoked using simplified out-of-box prompt files placed under:

- `Assignment 01/codebases/realworld/evaluations/aspnetcore/prompts/auth-login-smoke.md`
- `Assignment 01/codebases/realworld/evaluations/aspnetcore/prompts/article-lifecycle.md`
- `Assignment 01/codebases/realworld/evaluations/aspnetcore/prompts/comment-lifecycle.md`
- `Assignment 01/codebases/realworld/evaluations/aspnetcore/prompts/settings-null-fields.md`
- `Assignment 01/codebases/realworld/evaluations/aspnetcore/prompts/authorization-ownership.md`

These prompts followed a minimal structure:

- feature file path
- target implementation path
- expected output locations for the generated report and test code

This prompt style was used intentionally to demonstrate that the skill can work in a near out-of-box manner rather than requiring extensive hand-written instructions per feature.

### 3.2 Model Used

The generated feature reports record the model as:

- **Model / Agent Version**: `glm-5.1`

The black-box skill itself is located at:

- `Assignment 01/blackbox-testing/skills/SKILL.md`

### 3.3 Model-Generated Code

The generated executable test files are located at:

- `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/AuthLoginSmokeTests.cs`
- `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/ArticleLifecycleTests.cs`
- `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/CommentLifecycleTests.cs`
- `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/SettingsNullFieldsTests.cs`
- `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/AuthorizationOwnershipTests.cs`

The generated feature reports are located at:

- `Assignment 01/codebases/realworld/evaluations/aspnetcore/generated/auth-login-smoke.md`
- `Assignment 01/codebases/realworld/evaluations/aspnetcore/generated/article-lifecycle.md`
- `Assignment 01/codebases/realworld/evaluations/aspnetcore/generated/comment-lifecycle.md`
- `Assignment 01/codebases/realworld/evaluations/aspnetcore/generated/settings-null-fields.md`
- `Assignment 01/codebases/realworld/evaluations/aspnetcore/generated/authorization-ownership.md`

## 4. Generated Output

### 4.1 Output Types

The black-box testing workflow produced two kinds of outputs:

1. **Structured black-box testing reports**
   - feature summary
   - requirements extracted
   - EP/BVA analysis
   - edge case matrix
   - detailed test cases
   - coverage summary
   - run guidance

2. **Executable black-box test code**
   - external C# + xUnit + `HttpClient` tests against the running API
   - independent setup data generation (fresh users, articles, comments)
   - configurable base URL via `REALWORLD_BASE_URL`

### 4.2 Quantitative Output Summary

- **Total features evaluated**: 5
- **Total generated tests**: 86
- **Generated test files**: 5
- **Generated feature reports**: 5

## 5. Experimental Results

### 5.1 Clean Baseline Results

User-provided execution summary:

- **Failed tests before bug injection**: 20
- **Main clean-baseline failure pattern**: `500 InternalServerError` on invalid input cases that should have produced validation errors such as 422

These failures are important for evaluation. We did **not** treat every clean-baseline failure as “the tests are wrong.” Instead, we categorized mismatches into two groups:

1. **Implementation-specific mismatches that should be patched in the tests**
   - route differences
   - status conventions such as 200 vs 201 when not central to the requirement
   - response error shape differences where the implementation is still functionally acceptable for the feature slice

2. **Real defects that should remain as failing tests**
   - invalid inputs causing server crashes (`500`) instead of validation rejection
   - required-field handling that violates the requirement-level expectation

A concrete example is the auth-login-smoke suite. After patching the tests only for non-bug mismatches, the remaining failures were exactly the invalid-input cases where the implementation returned `500 InternalServerError` instead of 422. Those failures are useful black-box findings, not noise.

### 5.2 Bug Injection Results

User-provided execution summary after bug injection:

- **Failed tests after bug injection**: 72
- **Total bugs injected**: 20
- **Total bugs revealed**: 14

### 5.3 Bug-Revealing Effectiveness

We calculate a simple bug reveal rate as:

- **Bug reveal rate** = revealed bugs / injected bugs = `14 / 20 = 70%`

This is a useful result for a requirement-driven black-box suite generated by AI. It shows that the generated tests were not only structurally complete on paper, but also operationally capable of catching many seeded faults.

### 5.4 Interpretation of the Results

The experiment suggests three important conclusions:

1. **The generated suite has strong defect sensitivity**.
   A jump from 20 failures on the clean baseline to 72 failures after bug injection indicates that the suite reacts strongly to behavior changes.

2. **Some clean-baseline failures were true findings, not false positives**.
   The `500` responses on invalid inputs exposed robustness and validation defects in the implementation.

3. **Black-box generation still needs calibration**.
   Some mismatches were due to spec-vs-implementation convention gaps, so a practical workflow needs a disciplined method for deciding which failures are acceptable test corrections and which are real bugs.

## 6. Coverage Analysis

### 6.1 Requirement Coverage

According to the generated feature reports, each evaluated feature slice reported full requirement coverage for its scoped requirements.

Examples from the generated reports:

- `auth-login-smoke.md`: requirement coverage `4/4 = 100%`
- `comment-lifecycle.md`: requirement coverage `9/9 = 100%`
- `settings-null-fields.md`: requirement coverage `9/9 = 100%`
- `authorization-ownership.md`: requirement coverage `14/14 = 100%`
- `article-lifecycle.md`: all listed requirements were marked full in the coverage summary table

This does **not** mean the entire RealWorld backend is fully covered. It means each selected feature slice achieved full coverage **within the boundaries of that slice**.

### 6.2 EP and BVA Coverage

The generated reports explicitly include Equivalence Partitioning (EP) and Boundary Value Analysis (BVA) tables. This is important for the assignment because it shows that the skill output is not just code generation, but also test-design reasoning.

Representative examples:

- `auth-login-smoke.md`
  - EP coverage: `11/11 = 100%`
  - BVA coverage: `3/3 = 100%`
- `comment-lifecycle.md`
  - EP coverage: `11/11 = 100%`
  - BVA coverage: `3/3 = 100%`
- `settings-null-fields.md`
  - EP coverage: `13/13 = 100%`
  - BVA coverage: `4/4 = 100%`
- `authorization-ownership.md`
  - EP coverage: `14/14 = 100%`
  - BVA coverage: `4/4 = 100%`

### 6.3 Coverage Strengths

The strongest coverage areas across the five features were:

- required-field validation
- happy-path CRUD flows
- ownership/authorization boundaries
- null vs empty normalization semantics
- selective deletion and persistence checks
- state-based checks such as create → update → GET and create → delete → GET

### 6.4 Coverage Gaps

The generated reports themselves also identified under-covered or deferred areas, including:

- maximum-length boundaries when the spec did not define them
- malformed email formats outside smoke scope
- malformed/expired token behavior where the spec was ambiguous
- concurrency and race conditions
- omitted-field behavior when the spec only defined explicit null/empty semantics
- some non-existent-resource variants not explicitly required by the selected feature slice

These gaps are acceptable as long as they are reported explicitly. For the assignment, this strengthens the analysis because it shows that the evaluation distinguishes between **covered**, **deferred**, and **ambiguous** cases rather than pretending certainty.

## 7. Accuracy, Executability, and Generalizability

### 7.1 Accuracy

We consider accuracy here in a practical black-box sense: whether the generated tests are aligned closely enough with the requirements to produce useful signals when executed.

The results show mixed but positive accuracy:

- The reports were structurally strong and traceable to requirements.
- Many generated tests behaved meaningfully on the target implementation.
- Some tests initially failed due to spec/implementation mismatches and required calibration.
- After calibration, remaining failures were more meaningful and often exposed real defects.

Therefore, the tool demonstrated **moderate-to-good practical accuracy**, but not plug-and-play perfection.

### 7.2 Executability

The generated artifacts were executable in a realistic workflow:

- C# + xUnit test harness
- external API-level interaction
- reusable run instructions in each feature report
- configurable base URL with `REALWORLD_BASE_URL`

The generated reports often rated executability as 4 or 5 out of 5. That is consistent with our experience: the tests were runnable, but some required manual patching when black-box assumptions did not match the ASP.NET implementation exactly.

### 7.3 Generalizability

This experiment demonstrates partial generalizability in three senses:

1. **Across features in the same codebase**
   We generated tests for 5 distinct backend slices rather than a single smoke feature.

2. **Across requirement styles**
   The selected features include simple auth, lifecycle CRUD, nested-resource management, ownership constraints, and null/empty semantics.

3. **Across evaluation goals**
   The workflow supported both design-time artifacts (EP/BVA/test-case tables) and execution-time artifacts (executable tests + bug reveal analysis).

However, generalizability is still limited:

- we mainly validated one implementation family in this stage (ASP.NET Core)
- frontend-heavy features were not included in this report
- some implementation-specific adaptation was still needed for practical execution

So the correct conclusion is: **the approach is reasonably generalizable across backend feature slices, but broader cross-implementation validation is still future work**.

## 8. Comparison to Traditional Non-AI-Based Technique

### 8.1 Traditional Baseline

A traditional non-AI-based black-box testing workflow would usually involve:

- manual requirement reading
- manual EP/BVA design
- manually written test-case tables
- hand-written API tests by engineers or testers
- iterative debugging and maintenance by humans

### 8.2 Advantages of the AI-Based Approach

Compared with a traditional workflow, the AI-based approach showed these advantages:

1. **Higher drafting speed**
   The skill can quickly turn requirement slices into structured reports and executable tests.

2. **Better artifact completeness**
   It produces not only code, but also traceability tables, EP/BVA analysis, edge-case matrices, and run instructions.

3. **Good breadth of test ideas**
   The generated reports consistently included happy paths, negative cases, edge cases, and sequencing/state cases.

4. **Useful for rapid iteration**
   When the prompt was adjusted, the skill could be rerun on additional features with similar structure.

### 8.3 Disadvantages of the AI-Based Approach

Compared with a traditional workflow, the AI-based approach also showed clear weaknesses:

1. **Spec/implementation mismatch handling is hard**
   The model may generate tests that are perfectly reasonable from the spec but fail in practice because the implementation uses different status codes or error shapes.

2. **The model tends to over-explore the implementation**
   Without strict prompting, it may try to inspect the target codebase, which weakens black-box purity.

3. **Execution details are fragile**
   The generated tests still need correct runtime assumptions such as base URL, environment, and startup procedure.

4. **Need for human judgment remains high**
   Humans still need to decide whether a failure is a false positive, an implementation quirk, or a real defect.

### 8.4 Overall Comparison

Traditional techniques are slower but more controlled. The AI-based approach is much faster and produces richer artifacts, but it requires stronger prompt discipline and post-generation review. In our experiment, the best practical use is **AI for first-pass generation and structured analysis, human for adjudication and calibration**.

## 9. Limitations of AI and How We Improved the Tool During Practice

This part directly addresses the assignment requirement about limitations and improvement.

### 9.1 Limitation 1: The model tried to inspect the target implementation

Observed issue:

- when given a feature spec and a target path, the agent sometimes tried to read the target codebase for reference
- this conflicts with pure black-box testing, where tests should come from requirements rather than source code

Improvement:

- we revised the skill prompt to emphasize that the target path is the **system under test**, not a source of truth for design
- we clarified that executable tests should be self-contained external tests, preferably using a standard harness such as C# + xUnit + `HttpClient`
- we proposed separating execution metadata from implementation inspection by using an execution-context input when needed

### 9.2 Limitation 2: The model generated tests that were correct in theory but not directly executable

Observed issue:

- some generated tests assumed canonical status codes or payload shapes from the spec and upstream examples
- the ASP.NET implementation sometimes used different conventions

Improvement:

- we added report-template guidance requiring explicit run instructions
- we added execution-oriented prompts specifying output paths and test language
- we manually calibrated generated tests by distinguishing between:
  - superficial convention mismatches that can be patched
  - meaningful clean-baseline failures that should remain as bug findings

### 9.3 Limitation 3: The model could overclaim certainty on ambiguous cases

Observed issue:

- some edge cases, such as whitespace-only handling or unknown-resource behavior, were not fully specified in the requirement slice
- the model still had to pick expected outcomes to make executable tests

Improvement:

- the skill template was expanded to require explicit ambiguity and assumptions sections
- the generated reports now record missing information and assumption-based cases instead of hiding uncertainty

### 9.4 Limitation 4: Out-of-box prompts were not enough to guarantee black-box purity

Observed issue:

- a short prompt was helpful for demonstration, but not always sufficient to constrain model behavior tightly

Improvement:

- we designed simplified prompts for usability
- in parallel, we refined the skill instructions to better constrain behavior when self-contained black-box generation is required
- we learned that “simple user prompt + strong skill policy” is more stable than embedding every rule in each invocation

### 9.5 Practical Lesson

The main lesson is that AI testing tools improve significantly through iterative prompt engineering and artifact design. The improvement did not mainly come from changing the code under test; it came from improving:

- skill instructions
- output templates
- run guidance
- failure interpretation rules
- black-box boundaries

This is a useful result for the assignment because it demonstrates actual tool practice, not just one-shot generation.

## 10. Coherence of Design and Implementation

The design is coherent with the assignment objectives in the following way:

1. **Input**
   - requirement slices and project code base are clearly separated

2. **Tool artifact**
   - prompt files, skill file, model-generated reports, and generated tests are all preserved

3. **Generated output**
   - test cases are available both as human-readable design artifacts and executable code

4. **Experimental analysis**
   - the report includes coverage, accuracy discussion, bug reveal rate, and generalizability discussion

5. **Improvement loop**
   - the tool was iteratively refined based on observed failures and black-box purity concerns

This coherence is important because the assignment assesses not only outputs, but also the soundness of the design and reasoning process.

## 11. Suggested Mapping to Assessment Criteria

### 11.1 Understanding of Concepts (10%)

The work demonstrates understanding of:

- black-box testing
- equivalence partitioning
- boundary value analysis
- state/sequence testing
- authorization and error-path testing
- difference between clean-baseline mismatch and real defect

### 11.2 Coherence of Design and Implementation (20%)

The workflow is coherent because it links:

- requirement extraction
- prompt design
- skill-based generation
- code generation
- execution
- result interpretation
- tool improvement

### 11.3 Coverage and Effectiveness / Usefulness (40%)

Strengths:

- 5 backend feature slices
- 86 tests
- strong feature-level coverage
- 14/20 injected bugs revealed
- real clean-baseline defects also exposed

This section is likely the strongest evidence area of the report.

### 11.4 In-Depth Analysis (20%)

Strengths:

- distinguishes between test error and implementation error
- discusses spec/implementation mismatch explicitly
- includes generalizability analysis and limitations
- compares AI-based and traditional techniques

### 11.5 Presentation (10%)

To score well here, the final submission should keep the artifacts organized and clearly referenced. This report helps by explicitly mapping inputs, tool artifacts, outputs, and analysis.

## 12. Limitations of This Stage of the Work

Even though the results are promising, this stage still has limitations:

- only 5 feature slices were synthesized here
- only backend black-box testing is covered in this report
- cross-implementation comparison is still limited
- traditional baseline comparison is analytical rather than experimentally measured
- some result numbers were aggregated manually rather than via a fully automated benchmark pipeline

These limitations should be acknowledged in the final submission rather than hidden.

## 13. Summary

This experiment shows that our AI-assisted black-box testing workflow is effective enough to serve as a serious testing aid for backend API evaluation.

For the ASP.NET Core RealWorld implementation, the workflow:

- used requirement slices as the primary input
- generated structured black-box test reports and executable C# tests
- covered 5 backend features with 86 total tests
- found meaningful clean-baseline issues, especially invalid-input cases producing `500 InternalServerError`
- revealed 14 out of 20 injected bugs
- produced explicit EP/BVA/coverage artifacts suitable for submission
- exposed important AI limitations and supported iterative prompt/tool improvement

The strongest conclusion is not that AI replaces testing engineers. Rather, it can substantially accelerate black-box test design and initial automation, while human judgment remains essential for interpreting mismatches, maintaining black-box discipline, and deciding how to refine the tool.

## 14. Artifact Checklist for Submission

### Inputs

- Requirement feature files under `Assignment 01/codebases/realworld/specification/features/`
- ASP.NET Core project code base under `Assignment 01/codebases/realworld/implementations/aspnetcore`

### Tool Artifacts

- Skill definition: `Assignment 01/blackbox-testing/skills/SKILL.md`
- Prompt files: `Assignment 01/codebases/realworld/evaluations/aspnetcore/prompts/`
- Generated code: `Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/`
- Generated reports: `Assignment 01/codebases/realworld/evaluations/aspnetcore/generated/`

### Experimental Analysis

- Total tests: 86
- Total features: 5
- Failed tests before bug injection: 20
- Failed tests after bug injection: 72
- Injected bugs: 20
- Revealed bugs: 14
- Bug reveal rate: 70%

### Project Report

- This document: `Assignment 01/docs/member2/aspnetcore-blackbox-testing-report.md`
