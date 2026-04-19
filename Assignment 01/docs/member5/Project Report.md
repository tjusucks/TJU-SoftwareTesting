# Project Report

## LLM-based Black-box Testing and Execution using User-Invocable Skills

------

# 1. Introduction

Software testing is essential for validating system behavior against requirements. Traditional black-box testing techniques such as Equivalence Partitioning (EP), Boundary Value Analysis (BVA), Decision Table Testing, and manual scenario design are widely used in industry. However, these methods often require substantial human effort to interpret requirement documents, identify test conditions, and implement executable scripts.

In this project, we developed an AI-assisted black-box testing workflow using two Claude user-invocable skills:

- **blackbox-testing**
   Generates black-box test cases directly from natural language requirements or functional specifications.

- blackbox-execution

  Converts generated test cases into executable assets such as:

  - Newman / Postman collections for API systems
  - Playwright scripts for Web UI systems

The system also performs environment validation, execution readiness checks, and generates execution reports.

Our benchmark focused primarily on the **RealWorld** application (golang-gin implementation), with additional design considerations for OpenAPI-based systems and Web UI benchmarks.

------

# 2. Comparison to Traditional Non-AI-Based Techniques

------

## 2.1 Traditional Black-box Testing Workflow

Traditional non-AI approaches usually involve the following steps:

1. Tester reads requirement documents manually.
2. Tester extracts functional behaviors.
3. Tester applies testing techniques such as:
   - Equivalence Partitioning
   - Boundary Value Analysis
   - Error Guessing
   - Decision Table Testing
4. Tester writes test cases manually.
5. Tester converts them into executable scripts manually.
6. Tester executes and maintains tests.

This process is effective but labor-intensive.

------

## 2.2 AI-based Workflow in This Project

Our workflow automates many of these stages:

```
Project code base(Realworld (Golang) )
        ↓
blackbox-testing skill (SKILL.md)
        ↓
Structured Test Cases
(generated-blackbox-output-v1.md)
        ↓
blackbox-execution skill (SKILL2.md)
        ↓
Executable Test Assets
(Postman/Newman Collections, Playwright Scripts)
        ↓
Execution Results + Reports
(test-report.md, api-run.json, execution-summary.md)
```

------

## 2.3 Comparison Table

| Aspect                       | Traditional Technique | AI-based Skill Workflow      |
| ---------------------------- | --------------------- | ---------------------------- |
| Requirement understanding    | Manual                | Automated NLP interpretation |
| Test case generation         | Manual                | Automatic                    |
| Script generation            | Manual                | Automatic                    |
| Speed                        | Slow                  | Fast                         |
| Coverage consistency         | Depends on tester     | High repeatability           |
| Human expertise needed       | High                  | Moderate                     |
| Adaptability to new projects | Medium                | High                         |
| Error handling               | Precise but slow      | Fast but may hallucinate     |

------

## 2.4 Advantages of AI-based Approach

### 1. Significant Reduction in Manual Effort

The model automatically extracts functional behaviors, inputs, and expected outputs.

### 2. Faster Test Bootstrapping

Especially useful for:

- New APIs
- Legacy systems with poor documentation
- Rapid prototyping

### 3. Better Requirement-to-Test Traceability

Generated tests preserve requirement IDs (R1, R2...) and test case IDs (TC-001...).

### 4. Easy Multi-project Adaptation

The same workflow can be reused for:

- REST APIs
- OpenAPI systems
- Web UI applications

------

## 2.5 Disadvantages Compared to Traditional Methods

### 1. AI May Misinterpret Ambiguous Requirements

Natural language requirements often contain missing assumptions.

### 2. Hallucinated Assertions

The model may invent behaviors not explicitly specified.

### 3. Non-deterministic Outputs

Repeated runs may generate different tests.

### 4. Need for Human Review

Critical systems still require tester validation.

------

# 3. Analytical Report: Limitations Encountered and Improvements Made

------

# 3.1 Limitation 1 — Ambiguous Requirements

## Problem

Many requirements were incomplete or informal. Example:

```
User login should reject invalid requests.
```

This leaves open questions:

- wrong password = 401?
- empty email = 422?
- malformed JSON = 400?

The model sometimes guessed inconsistent expectations.

------

## Improvement

We refined prompts to force structured extraction:

```
For each requirement identify:
- valid behavior
- invalid inputs
- missing fields
- authorization errors
- expected status codes if inferable
```

This significantly improved consistency.

------

# 3.2 Limitation 2 — Over-generation of Redundant Tests

## Problem

Initial outputs contained many repetitive test cases:

- duplicate empty input cases
- multiple equivalent negative cases

This reduced efficiency.

------

## Improvement

We added generation constraints:

```
Generate minimal but representative tests using:
- Equivalence Partitioning
- Boundary Values
- Unique failure classes only
```

This reduced duplicate cases and improved usefulness.

------

# 3.3 Limitation 3 — Weak Stateful API Flows

## Problem

Some APIs require chained operations:

```
register → login → token → create article → update article
```

Initial generations produced isolated tests without setup steps.

------

## Improvement

We introduced explicit precondition fields:

```
Preconditions:
- authenticated user exists
- article already created
```

Then blackbox-execution translated these into setup requests.

This greatly improved executability.

------

# 3.4 Limitation 4 — Environment Dependency

## Problem

Generated tests failed when:

- backend not running
- wrong API base URL
- missing Newman
- missing browser binaries

------

## Improvement

blackbox-execution added environment checks:

| Check             | Example        |
| ----------------- | -------------- |
| node installed    | node -v        |
| newman installed  | newman -v      |
| backend reachable | localhost:8080 |
| APIURL defined    | env var        |

This improved reliability.

------

# 3.5 Limitation: Poor Requirement-to-Test Traceability

## Problem

Traditional LLM outputs often generate many test cases but lose mapping to requirements.

Example:

- TC001 login test
- TC002 invalid login

But no clear relation to:

- R1 Authentication
- R2 Validation
- R3 Authorization

This weakens evaluation and reporting.

------

## Improvement in `blackbox-testing`

We redesigned the output schema so that every test case contains:

- Test Case ID
- Requirement Reference
- Preconditions
- Steps
- Expected Result

The skill also performs a final **Coverage Review** checking:

- every requirement covered
- normal case present
- negative case present
- boundary case present when applicable

This greatly improved report quality and grading readiness.





# 4. Experimental Findings

Using the RealWorld benchmark:

- 20 requirement-traceable API test cases
- 24 Newman executable requests
- Multiple passed behaviors
- Several requirement-level failures discovered

Examples:

- duplicate username accepted unexpectedly
- deleting missing resource returned success
- invalid pagination bounds accepted

This demonstrates that the workflow is not only generative, but practically useful for defect discovery.

## Overall Evaluation

#### Strengths

- Rapid test generation from requirements
- Executable outputs rather than theoretical cases
- Works across API / Web systems
- Good scalability
- Useful for regression bootstrapping

------

#### Weaknesses

- Depends on prompt quality
- Sensitive to vague requirements
- Requires human review for critical systems
- May miss deep business logic combinations

------

# 5. Summary

This project demonstrates that user-invocable LLM skills can significantly improve black-box testing workflows.

Instead of manually converting requirements into tests and then into executable scripts, our two-skill pipeline automates the process:

```
Requirements
→ blackbox-testing
→ test cases
→ blackbox-execution
→ Newman / Playwright assets
→ execution reports
```

Compared with traditional non-AI approaches, the workflow offers major gains in speed, scalability, and automation.

During development, we encountered several realistic AI limitations, including ambiguity handling, redundant outputs, unstable assertions, and environment sensitivity. Through prompt refinement, structured schemas, state-aware generation, and environment validation, the system became substantially more reliable.

Our benchmark results on RealWorld show that AI-generated black-box tests can successfully execute on real systems and detect requirement-level defects.

Therefore, we conclude that LLM-assisted testing is a promising practical enhancement to traditional software quality assurance workflows, especially for rapid regression testing, API validation, and requirement-driven automated testing.

