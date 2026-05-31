---
parent-skill: requirement-structuring
reference-type: specification-output
load-when: Producing structured requirement specification, R1/R2-style requirement IDs, acceptance criteria, JSON/CSV/Markdown output, and handoff artifacts
---

# Requirement Specification Output Reference

**Parent Skill:** requirement-structuring
**When to Use:** Finalizing structured requirement outputs.

This reference defines the output contract for the requirement structuring skill.

---

## Purpose & Context

**What this achieves**: Convert analyzed requirement candidates into stable,
machine-readable and human-readable requirement specification artifacts.

**When to apply**:

- Requirement candidates have been extracted from sources.
- Orchestration needs stable intermediate data.
- Downstream workflows will consume requirements for risk analysis or test design.
- Requirement IDs must be assigned or normalized to `R1`, `R2`, `R3` style.

**Prerequisites**:

- Requirement input inventory exists.
- Requirement candidates include source evidence.
- Ambiguities and assumptions have been captured.

---

## Required Deliverables

Produce these artifacts:

1. **Requirement input inventory**
   - Sources considered, used, skipped, and why.

2. **Structured requirement specification**
   - JSON is preferred for tool handoff.
   - Markdown table is preferred for human review.
   - CSV is optional when spreadsheet import is needed.

3. **Requirement ID registry**
   - Sequential `R*` IDs and the next assigned number.

4. **Review notes**
   - Assumptions, inferred requirements, unresolved questions.

Do not include risk scores, test cases, coverage items, or execution results in the
requirement structuring output.

---

## JSON Output Schema

Use this downstream-compatible top-level structure:

```json
{
  "schema_version": "1.0",
  "project_name": "RealWorld",
  "feature_id": "authentication",
  "feature_name": "Authentication",
  "source_path": "README.md",
  "actors": ["registered user"],
  "preconditions": ["The user has an existing account"],
  "business_rules": [],
  "input_constraints": ["Email and password are required"],
  "error_conditions": ["Credentials are invalid"],
  "requirement_items": []
}
```

Each requirement should follow this schema:

```json
{
  "id": "R1",
  "description": "A registered user can log in with email and password.",
  "input_fields": ["email", "password"],
  "data_ranges": [],
  "conditions": ["The account exists", "The credentials are valid"],
  "expected_actions": ["The system creates an authenticated session."],
  "notes": "Source: SRC-DOC-001. Type: functional. Confidence: high."
}
```

Use `notes` for requirement-level metadata that is useful to humans but not modeled
as separate fields in the structured requirements schema, such as source references,
confidence, requirement type, assumptions, constraints, and open questions.

Allowed ID pattern:

- `id`: `R1`, `R2`, `R3`, continuing sequentially without gaps unless preserving a
  previously reviewed baseline.

---

## Requirement ID Rules

Use stable sequential IDs:

```text
R{N}
```

Examples:

- `R1`
- `R2`
- `R3`
- `R10`

Rules:

- Start at `R1` for the feature requirement set unless continuing a prior baseline.
- Increment by one for each accepted or needs-review requirement.
- Never reuse an ID in the same run after deletion or rejection.
- Do not encode component names into the ID; keep component information in
  `feature_id`, `feature_name`, `description`, or `notes`.
- If an ID is deleted during review, record the deletion in review notes and assign
  the next new requirement the next unused number.

---

## Acceptance Criteria Rules

Acceptance criteria are allowed because they make requirements testable, but they must
remain requirement-level output, not full test cases.

Use Given-When-Then style when possible:

```text
Given [condition], when [action], then [expected behavior].
```

Good criteria:

- Are observable and testable.
- Reflect the source requirement.
- Include negative/error behavior only when source evidence supports it or it is a
  necessary direct counterpart.

Avoid:

- Detailed test data tables.
- Test case IDs.
- Browser automation steps.
- Coverage matrix fields.
- Risk-based priority labels.

---

## Markdown Review Format

For human review, include a compact table:

| ID  | Title           | Type       | Source      | Requirement                 | Conditions     | Constraints | Expected Behavior   | Status    |
| --- | --------------- | ---------- | ----------- | --------------------------- | -------------- | ----------- | ------------------- | --------- |
| R1  | User can log in | functional | SRC-DOC-001 | Registered user can log in. | Account exists | None        | Session is created. | candidate |

After the table, list:

- Assumptions.
- Open questions.
- Inferred requirements.
- Excluded sources.

---

## CSV Columns

If CSV is requested, use these columns:

```text
id,description,input_fields,data_ranges,conditions,expected_actions,notes
```

Represent list fields with semicolon-separated values.

---

## Quality Checklist

Specification is ready for handoff when:

- [ ] Every requirement has a stable sequential `R*` ID.
- [ ] Every requirement has at least one source reference in `notes` or the review
      summary.
- [ ] Conditions, constraints, and expected behavior are separated.
- [ ] Requirements are atomic and testable.
- [ ] Inferred requirements are marked with confidence below `high` in `notes`.
- [ ] Ambiguous items are marked as `needs_review` in `notes`.
- [ ] Priority is omitted from the machine schema unless the source evidence or user
      request requires candidate priority in `notes`.
- [ ] Output does not include risk scores, black-box test cases, white-box paths, or execution results.

---

## Example Handoff Summary

```markdown
## Requirement Structuring Summary

- Sources used: 3
- Sources skipped: 2
- Requirements extracted: 18
- Accepted candidates: 12
- Needs review: 6
- Inferred from code: 2
- Last assigned requirement ID: R18

Main open questions:

- Should social login be in scope?
- What exact response-time target applies to feed loading?
- Are anonymous users allowed to view article comments?
```

---

## Related Practices

Before specification:

- Use `requirements-elicitation.md` to build the input inventory.
- Use `requirements-analysis.md` to normalize requirement candidates.

After specification:

- Hand off JSON/Markdown results to the orchestrator or downstream workflow.
- Risk analysis may use the requirements for scoring.
- Test design may derive black-box coverage and test cases.
