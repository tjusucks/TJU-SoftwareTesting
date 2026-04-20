# LLM Based Black-Box Testing Skill

GitHub Repository: https://github.com/tjusucks/TJU-SoftwareTesting

## Introduction

The core of the project is a Claude Code user-invocable skill, `blackbox-testing`, designed to transform natural-language requirements into practical testing outputs that can be used for review, manual testing, or automated execution. Around this skill, we built a broader workflow covering prompt design, output templating, execution conversion, and evaluation. The project emphasizes requirement-to-test traceability, explicit coverage analysis, and clear reporting of ambiguities and assumptions.

To evaluate the workflow on realistic systems rather than toy examples, we used the RealWorld benchmark as the main case study. We extracted feature-level requirement slices, generated black-box tests for multiple backend features, and executed them against different RealWorld implementations. Our experiments focused on both artifact quality and practical usefulness: whether the generated tests were executable, whether they covered important requirement branches, and whether they could reveal real or injected defects.

## Quick Start

This project is packaged as a Claude Code plugin named `blackbox-testing`, listed in the marketplace `software-testing-skills`.

### Installation

You can install it from the marketplace with:

```bash
claude plugin marketplace add https://github.com/tjusucks/TJU-SoftwareTesting.git
claude plugin install blackbox-testing@software-testing-skills
```

Or in claude code tui:

```bash
/plugin marketplace add https://github.com/tjusucks/TJU-SoftwareTesting.git
/plugin install blackbox-testing@software-testing-skills
/reload-plugins
```

### Usage

After installation, invoke the skill with the slash command in claude code:

```text
/blackbox-testing:blackbox-testing
```

Typical usage pattern:

```text
/blackbox-testing:blackbox-testing
Use this feature spec to generate black-box tests.

Feature file:
<feature-spec>

Target:
<system under test>

Write:
- <generated report path>
- <generated test file path>
```

### Update

You can update the plugin to the latest version in the marketplace with:

```bash
claude plugin marketplace update software-testing-skills
```

### Remove the plugin

You can remove the plugin with:

```bash
claude plugin remove blackbox-testing@software-testing-skills
```

## Workflow Overview

1. Prepare a requirement-driven input, such as a feature specification, API behavior description, or user story.
2. Invoke the `blackbox-testing` skill in Claude Code and provide the requirement input together with the intended output location.
3. Let the skill generate structured black-box testing artifacts, including requirement extraction, EP/BVA analysis, test scenarios, detailed test cases, and coverage notes.
4. If executable tests are needed, save the generated test code to the evaluation workspace and run it against the target system as an external black-box harness.
5. Review the execution results, distinguish real defects from spec/implementation mismatches, and refine the prompt or skill if needed.
6. Summarize the final outputs as submission artifacts, including inputs, prompts, generated code, execution evidence, and analysis.

## Experimental Setup

### Target System

- [RealWorld Example Applications](https://github.com/realworld-apps/realworld)
- [Golang Gin Implementation](https://github.com/gothinkster/golang-gin-realworld-example-app)
- [ASP.NET Core Implementation](https://github.com/gothinkster/aspnetcore-realworld-example-app)

### Model

- GLM 5.1 via [NVIDIA NIM API](https://build.nvidia.com/models) (OpenAI Compatible)
- Minimax M2.7 via [NVIDIA NIM API](https://build.nvidia.com/models) (OpenAI Compatible)

### Tools

- Claude Code: Skill integration and execution.
- Claude Code Router: Convert OpenAI compatible API to Anthropic API for Claude Code.

## Input Artifacts

### Main Prompt

The main prompt is the direct user request that calls the skill and tells it what requirement slice to use, what system is under test, and where to write the outputs.

**Example:** `Assignment 01/codebases/realworld/evaluations/aspnetcore/prompts/auth-login-smoke.md`

```text
/blackbox-testing:blackbox-testing
Use this feature spec to generate black-box tests.

Feature file:
Assignment 01/codebases/realworld/specification/features/auth-login-smoke.md

Target:
Assignment 01/codebases/realworld/implementations/aspnetcore

Write:
- Assignment 01/codebases/realworld/evaluations/aspnetcore/generated/auth-login-smoke.md
- Assignment 01/codebases/realworld/evaluations/aspnetcore/tests/AuthLoginSmokeTests.cs
```

### Skill & Template

The skill defines how Claude should convert requirements into black-box testing artifacts. The template constrains the output format so that generated results remain consistent and executable.

**SKILL:** `Assignment 01/blackbox-testing/skills/SKILL.md`

```markdown
---
name: blackbox-testing
description: Skill for automated black-box testing based on the requirements specification. Use this when users request black-box testing for a software system, and provide the requirements specification as input. The skill will generate test cases that cover various scenarios and edge cases based on the provided requirements.
license: Apache-2.0
disable-model-invocation: true
user-invocable: true
---

# Blackbox Testing

## Purpose

You are an expert QA engineer specializing in black-box testing. Your task is to generate high-quality black-box test cases from a requirements specification, without relying on implementation details or source code.

Use this skill when the user provides a software requirements specification, feature description, user story, business rules, API contract, UI behavior description, or any other functional specification, and asks for test design or black-box test generation.

## Project-Aligned Input Guidance

## Core Responsibilities

## Rules of Engagement

## Workflow

### Step 1: Understand the specification

### Step 2: Identify test dimensions

...
```

**Template:** `Assignment 01/blackbox-testing/skills/template.md`

```markdown
# Black-Box Testing Run Report Template

## 1. Run Metadata

## 2. Input Summary

...
```

### Requirement Specification

The requirement specification is the feature-level input used by the skill. It defines expected behavior, business rules, validation conditions, and acceptance criteria.

**Example:** `Assignment 01/codebases/realworld/specification/features/auth-login-smoke.md`

```markdown
---
name: auth-login-smoke
source: realworld
category: backend
complexity: low
recommended_role: smoke
references:
  - upstream/specs/api/bruno/auth/01-register.bru
  - upstream/specs/api/bruno/auth/02-login.bru
  - upstream/specs/api/bruno/errors-auth/07-login-empty-email.bru
  - upstream/specs/api/bruno/errors-auth/09-login-wrong-password.bru
---

**API Specification Example:** `Assignment 01/codebases/realworld/specification/upstream/specs/api/bruno/auth/01-register.bru`

# Auth Login Smoke

## Purpose

This feature slice covers the minimum authentication flow needed to verify that a RealWorld implementation is reachable and behaves correctly for basic user registration and login.

...
```

```bruno
meta {
  name: Register
  type: http
  seq: 1
}

post {
  url: {{host}}/api/users
  body: json
  auth: none
}

body:json {
  {
    "user": {
      "username": "auth_{{uid}}",
      "email": "auth_{{uid}}@test.com",
      "password": "password123"
    }
  }
}

assert {
  res.status: eq 201
}

script:post-response {
  bru.setVar("reg_token", res.body.user.token);
  expect(res.body.user.username).to.eql("auth_" + bru.getVar("uid"));
  expect(res.body.user.email).to.eql("auth_" + bru.getVar("uid") + "@test.com");
  expect(res.body.user.bio).to.be.null;
  expect(res.body.user.image).to.be.null;
  expect(typeof res.body.user.token).to.eql("string");
  expect(res.body.user.token).to.not.eql("");
}
```

## Generate Output

### Black-Box Testing Report

The first major output is a structured report that records requirement extraction, test design strategy, EP/BVA coverage, detailed test cases, and ambiguity analysis.

**Example:** `Assignment 01/codebases/realworld/evaluations/aspnetcore/generated/auth-login-smoke.md`

```md
...

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

...
```

### Executable Test Cases

The second major output is executable test code generated from the requirement slice. In the ASP.NET Core evaluation, the generated tests use C# + xUnit + HttpClient as an external black-box harness.

**Example:** `Assignment 01/codebases/realworld/evaluations/golang-gin/tests/auth_login_smoke_test.go`

```go
// ========== Test: Login with Non-Existent User ==========

func TestLoginNonExistentUser(t *testing.T) {
	host := getTestHost()
	uid := generateUID()

	loginReq := UserLoginRequest{}
	loginReq.User.Email = fmt.Sprintf("nonexistent_%s@test.com", uid)
	loginReq.User.Password = "password123"

	loginBytes, _ := json.Marshal(loginReq)
	loginEndpoint, _ := url.JoinPath(host, "/api/users/login")

	resp, err := http.Post(loginEndpoint, "application/json", bytes.NewReader(loginBytes))
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	// Should return 401 for non-existent user (same as wrong password for security)
	if resp.StatusCode != http.StatusUnauthorized {
		t.Errorf("Expected status 401 for non-existent user, got %d", resp.StatusCode)
	}
}
```

The testcase can be executed against the target system as an external black-box harness:

```bash
cd "Assignment 01/codebases/realworld/evaluations/golang-gin/tests"
TEST_HOST=http://localhost:8080 go test ./... -v -count=1
```

### Execution Summaries

After generated tests are executed, the project records summarized execution evidence, including environment readiness, pass/fail results, and requirement-level failures.

**Example:** `Assignment 01/docs/member3/test-report.md`

```markdown
...

## API Execution Results

| Test Case ID | Requirement | Status       | Notes                                                                                       |
| ------------ | ----------- | ------------ | ------------------------------------------------------------------------------------------- |
| TC-001       | R1          | pass         | Registration success with token                                                             |
| TC-002       | R1          | fail         | Baseline allowed duplicate username and returned success                                    |
| TC-003       | R1          | pass         | Duplicate email rejected with 422                                                           |
| TC-004       | R2          | pass         | Valid login succeeded                                                                       |
| TC-005       | R2          | pass         | Wrong password rejected with 401                                                            |
| TC-006       | R2          | pass         | Malformed login rejected with 422                                                           |
| TC-007       | R3          | pass         | Authorized current user returned expected email                                             |
| TC-008       | R3          | pass         | Missing token rejected with 401                                                             |
| TC-009       | R3          | pass         | User update persisted bio/image                                                             |
| TC-010       | R4          | pass         | Article creation succeeded                                                                  |
| TC-011       | R4          | pass         | Malformed article rejected with 422                                                         |
| TC-012       | R5          | pass         | Owner update succeeded                                                                      |
| TC-013       | R5          | pass         | Non-owner update rejected with 403                                                          |
| TC-014       | R5          | fail         | Missing slug delete returned 200 instead of expected 404                                    |
| TC-015       | R6          | pass         | Follow toggle with auth succeeded                                                           |
| TC-016       | R6          | pass         | Missing auth follow rejected                                                                |
| TC-017       | R7          | pass         | `limit=1&offset=0` behaved as expected                                                      |
| TC-018       | R7          | fail         | Invalid bounds were accepted with 200 instead of rejection                                  |
| TC-019       | R8          | pass         | Comment create and owner delete succeeded                                                   |
| TC-020       | R8          | partial fail | Non-owner delete correctly rejected, but missing comment delete returned 200 instead of 404 |

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

...
```

## Experimental Analysis

The evaluation method combines prompt iteration, generated artifact inspection, executable test running, and defect-reveal analysis. Clean-baseline failures are reviewed to separate true defects from spec/implementation mismatches.

### Coverage of EP/BVA

The generated black-box reports explicitly include Equivalence Partitioning (EP) and Boundary Value Analysis (BVA) sections rather than only free-form test lists. This is important because it makes the generated artifacts easier to review, trace back to requirements, and compare across prompt versions and target implementations.

Across the ASP.NET Core feature-slice evaluation, EP/BVA coverage was consistently high:

| Feature                 | Requirement Coverage | EP Coverage  | BVA Coverage | Notes                                                |
| ----------------------- | -------------------- | ------------ | ------------ | ---------------------------------------------------- |
| Auth Login Smoke        | 4/4 = 100%           | 11/11 = 100% | 3/3 = 100%   | Strong login validation and duplicate-input coverage |
| Article Lifecycle       | 16/16 = 100%         | 21/21 = 100% | 5/5 = 100%   | Strong tagList boundary and CRUD lifecycle coverage  |
| Comment Lifecycle       | 9/9 = 100%           | 11/11 = 100% | 3/3 = 100%   | Strong create/delete/authorization edge coverage     |
| Settings Null Fields    | 9/9 = 100%           | 13/13 = 100% | 4/4 = 100%   | Strong null vs empty-string semantics coverage       |
| Authorization Ownership | 14/14 = 100%         | 14/14 = 100% | 4/4 = 100%   | Strong owner vs non-owner boundary coverage          |

These results show that the workflow does not only generate happy-path tests. It systematically covers:

- valid and invalid equivalence classes,
- empty and missing input cases,
- boundary transitions such as empty vs non-empty and null vs empty array,
- authorization boundaries such as authenticated owner vs authenticated non-owner,
- and stateful sequences such as create → update → GET and create → delete → GET.

At the same time, the generated reports also record under-covered or deferred EP/BVA areas when the requirement does not define an expected outcome clearly. Typical deferred items include maximum-length limits, malformed token behavior, concurrency, and whitespace-only edge cases when the specification is ambiguous. This makes the EP/BVA analysis more trustworthy, because uncovered boundaries are reported explicitly instead of being silently guessed.

### Prompt Iteration

#### Summary table

| Prompt version | Requirement coverage | Boundary hit count | Duplicate case rate | Executability |
| -------------- | -------------------- | ------------------ | ------------------- | ------------- |
| v1             | 66.7% (8/12)         | 6                  | 22.2% (4/18)        | 3.0/5         |
| v2             | 91.7% (11/12)        | 12                 | 10.0% (2/20)        | 4.2/5         |
| v3             | 100.0% (12/12)       | 17                 | 4.3% (1/23)         | 4.7/5         |

#### Relative improvements

- v2 vs v1:
  - Coverage: `+25.0%`
  - Boundary hits: `+6`
  - Duplicate rate: `-12.2%`
  - Executability: `+1.2`
- v3 vs v2:
  - Coverage: `+8.3%`
  - Boundary hits: `+5`
  - Duplicate rate: `-5.7%`
  - Executability: `+0.5`
- v3 vs v1:
  - Coverage: `+33.3%`
  - Boundary hits: `+11`
  - Duplicate rate: `-17.9%`
  - Executability: `+1.7`

### Defect-Reveal & Generalizability Analysis

#### Bug Injection

We inject selected defects into the target system to test whether the generated tests can reveal them.

**Example:** `Assignment 01/codebases/realworld/bugs/golang-gin/authorization-ownership.patch`

```diff
diff --git a/articles/routers.go b/articles/routers.go
index 3aea0b5..b1e8513 100644
--- a/articles/routers.go
+++ b/articles/routers.go
@@ -212,7 +212,7 @@ func ArticleCommentDelete(c *gin.Context) {
 		// Comment exists, check authorization
 		myUserModel := c.MustGet("my_user_model").(users.UserModel)
 		articleUserModel := GetArticleUserModel(myUserModel)
-		if commentModel.AuthorID != articleUserModel.ID {
+		if commentModel.Article.AuthorID != articleUserModel.ID {
 			c.JSON(http.StatusForbidden, common.NewError("comment", errors.New("you are not the author")))
 			return
 		}

```

#### Results

The final results are summarized using coverage, executability, baseline failures, and bug reveal rate.

- Total features evaluated: 5
- Total generated tests: 20 (Go) + 86 (C#)
- Failed tests before bug injection: 14 (Due to spec/implementation mismatches and baseline defects)
- Failed tests after bug injection: 78
- Total bugs injected: 20
- Total bugs revealed: 14
- Bug reveal rate: 70%

The skill successfully revealed 70% of the injected bugs across the 5 evaluated features and 2 different implementations, demonstrating its practical effectiveness in generating tests that can detect real defects and its generalizability across different systems.

## Comparison to Traditional Non-AI-Based Technique

### Traditional Baseline Workflow

A traditional non-AI workflow would normally require humans to:

- read the requirements manually
- derive EP/BVA and scenarios manually
- write test-case tables manually
- hand-code API/UI test scripts manually
- debug and maintain these artifacts manually
- manually keep traceability and evaluation tables synchronized

### Comparison Table

- **Speed**: AI-assisted workflow is faster for drafting and bootstrapping.
- **Coverage breadth**: AI-assisted workflow expands branches faster, especially negative and boundary cases.
- **Precision / control**: Traditional workflow is more stable at fine-grained expectation control.
- **Human effort**: AI-assisted workflow reduces manual generation effort but not review effort.
- **Reusability**: AI-assisted prompts/skills are more reusable across similar tasks.
- **Explainability**: Traditional handcrafted tests are often easier to justify line by line, but AI-generated reports improve explainability by adding explicit tables.

### Pros of the AI-Based Technique

- Faster generation from raw requirements
- Better structured artifact completeness
- Easy prompt reuse and iteration
- Strong requirement-to-test traceability after prompt refinement
- Useful for quick benchmark bootstrapping and regression seeding

### Cons of the AI-Based Technique

- Sensitive to ambiguous requirements
- Can hallucinate or over-assume unspecified behavior
- May drift toward implementation inspection without strong constraints
- Requires human adjudication for spec-vs-implementation mismatches
- Execution readiness still depends on good environment setup

### Overall Comparison Conclusion

The traditional approach remains stronger when absolute precision and stable expectations are the highest priority. The AI-assisted approach is much stronger for speed, artifact breadth, and rapid iteration. In practice, the best workflow is not AI-only or human-only, but a hybrid model: AI generates the first structured draft and execution assets, while humans review, calibrate, and decide which failures are true findings.

## Limitations of AI and Tool Improvement During Practice

### Limitation 1

AI may misinterpret ambiguous requirements.

This appeared across the project whenever requirements left room for multiple acceptable status codes or payload shapes. We improved this by introducing requirement-first extraction, explicit requirement IDs, ambiguity sections, and stronger prompt constraints requiring missing information to be reported rather than silently guessed.

### Limitation 2

AI may over-generate redundant or weakly distinct tests.

Prompt v1 had a relatively high duplicate case rate. Through prompt evolution, especially in v3, we added explicit dedup rules and branch-focused coverage instructions. This reduced duplicate rate significantly and improved execution efficiency.

### Limitation 3

AI-generated outputs may not be directly executable.

This happened when the generated tests did not include enough setup information, assumed idealized response shapes, or depended on missing environment information. The project improved this by adding preconditions, run guidance, execution templates, environment validation, and explicit separation between black-box design and execution setup.

### Practical Lessons Learned

The most important practical lesson is that building a useful AI testing tool is not a one-shot prompting task. Stable results required:

- fixed I/O contracts
- iterative prompt refinement
- strict metric definitions
- requirement-to-test traceability
- execution mapping rules
- explicit ambiguity handling
- human review for calibration

## Conclusion

This project demonstrates the potential of LLM-based techniques to enhance black-box testing workflows, especially in terms of speed, coverage breadth, and artifact structure. However, it also highlights the importance of careful prompt design, iterative refinement, and human-in-the-loop review to mitigate limitations such as ambiguity sensitivity and execution readiness. The best practical approach is a hybrid workflow that leverages AI for rapid generation and humans for calibration and judgment.
