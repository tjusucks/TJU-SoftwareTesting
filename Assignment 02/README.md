# AutoTestDesign Tool

AI-driven test design tool built as a set of Claude Code skills. Covers requirement structuring, risk analysis, black-box test design, white-box modeling, and end-to-end orchestration with interactive review.

## Installation

This project is packaged as a Claude Code plugin named `test-design`, listed in the marketplace `software-testing-skills`.

```bash
claude plugin marketplace add https://github.com/tjusucks/TJU-SoftwareTesting.git
claude plugin install test-design@software-testing-skills
```

Or in Claude Code TUI:

```text
/plugin marketplace add https://github.com/tjusucks/TJU-SoftwareTesting.git
/plugin install test-design@software-testing-skills
/reload-plugins
```

### Update

```bash
claude plugin marketplace update software-testing-skills
```

### Remove

```bash
claude plugin remove test-design@software-testing-skills
```

## Skills

| Skill                   | Slash Command                          | Description                                                           |
| ----------------------- | -------------------------------------- | --------------------------------------------------------------------- |
| Orchestrator            | `/test-design:orchestrator`            | End-to-end pipeline coordinator with interactive review at each stage |
| Requirement Structuring | `/test-design:requirement-structuring` | Extract and structure requirements from various sources               |
| Risk Analysis           | `/test-design:risk-analysis`           | Assign risk scores and test priorities to requirements                |
| Black-Box Design        | `/test-design:black-box-design`        | Generate test cases using EP, BVA, and Decision Table techniques      |
| White-Box Modeling      | `/test-design:white-box-modeling`      | Model internal behavior using state transitions or control-flow paths |

## Usage

### Full Pipeline (Recommended)

Run the orchestrator to execute all stages with interactive review:

```text
/test-design:orchestrator
Analyze the RealWorld article-lifecycle feature.

Feature spec: Assignment 01/codebases/realworld/specification/features/article-lifecycle.md
Codebase: Assignment 01/codebases/realworld/implementations/golang-gin/
```

The orchestrator will guide you through each stage, pausing for review after each one.

### Individual Skills

Each skill can also be invoked standalone:

```text
/test-design:black-box-design
Run black-box test design for the article-lifecycle feature.

Inputs:
- structured requirements: outputs/structured-requirements/article-lifecycle.json
- risk analysis: outputs/risk-analysis/article-lifecycle/risk-analysis.json
- feature spec: Assignment 01/codebases/realworld/specification/features/article-lifecycle.md

Write output to: outputs/black-box-design/runs/article-lifecycle-run1/
```

## Output Structure

```
outputs/
├── structured-requirements/{feature_id}/
├── risk-analysis/{feature_id}/
├── black-box-design/runs/{run_id}/
├── white-box-modeling/{feature_id}/
└── pipeline/{feature_id}/
    ├── pipeline-summary.md
    └── pipeline-status.md
```
