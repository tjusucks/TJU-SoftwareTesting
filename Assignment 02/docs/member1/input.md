# Module Demo

```plain
/test-design:white-box-modeling
Continue from the existing article-lifecycle white-box modeling results.

Existing artifacts: Assignment 02/outputs/white-box-modeling/article-lifecycle/artifacts.json

Now generate executable tests from the WB-PATH-* paths.
Use Playwright for API-level tests against the RealWorld Conduit backend.
Write tests to: Assignment 02/outputs/white-box-modeling/article-lifecycle/tests/
Each test must reference the corresponding WB-PATH-* ID in comments.
```

```plain
/test-design:orchestrator
Continue the article-lifecycle pipeline.

Existing outputs (skip these stages, present for reference only):
- structured requirements: Assignment 02/outputs/structured-requirements/article-lifecycle.json
- risk analysis: Assignment 02/outputs/risk-analysis/article-lifecycle/risk-analysis.json
- black-box design: Assignment 02/outputs/black-box-design/final/article-lifecycle/

Start from Stage 4 (white-box modeling).
Generate executable tests using Playwright after modeling is confirmed.
Write to: Assignment 02/outputs/white-box-modeling/article-lifecycle/
```
