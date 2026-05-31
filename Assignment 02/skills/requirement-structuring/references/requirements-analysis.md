---
parent-skill: requirement-structuring
reference-type: structuring-analysis
load-when: Extracting requirement items, constraints, conditions, expected behavior, ambiguity, duplicates, and conflicts for requirement structuring
---

# Requirement Structuring Analysis Reference

**Parent Skill:** requirement-structuring
**When to Use:** Turning raw requirement sources into normalized requirement items.

This reference is the core requirement structuring analysis step. It is intentionally
narrower than general requirements lifecycle analysis: it structures requirements,
but does not own risk scoring, test planning, black-box design, or change management.

---

## Purpose & Context

**What this achieves**: Extract clear, reviewable requirement records from input
sources while preserving constraints, conditions, and expected behavior.

**When to apply**:

- Requirements are embedded in README, docs, issues, API descriptions, CSV rows, or
  user/agent CLI input.
- A source sentence contains multiple behaviors that need splitting.
- Requirements need IDs, types, confidence, and review status.
- Ambiguity or missing information must be surfaced for human review.

**Prerequisites**:

- Requirement input inventory exists or can be created.
- Sources have enough context to cite evidence.
- The expected output schema is the parent skill default or a user-provided variant.

---

## Extraction Rules

Extract a requirement when the source expresses:

- A user-visible capability.
- A system behavior or business rule.
- A data validation rule.
- A constraint on security, performance, reliability, compatibility, or usability.
- A required workflow, state transition, or error behavior.

Do not extract:

- Pure implementation preferences unless stated as constraints.
- General project goals that are not testable.
- Marketing language without observable behavior.
- Duplicate statements unless they add new conditions or constraints.

When uncertain, keep the candidate with `status: needs_review` and add an open
question.

---

## Normalized Requirement Fields

Use these fields while analyzing candidates. When producing the final
schema-compatible machine output, map fields that are not in
`structured-requirements.schema.json`
into `notes`, top-level arrays, or the human-readable review summary.

For every item, identify these fields when available:

| Field              | Purpose                                                                       |
| ------------------ | ----------------------------------------------------------------------------- |
| `id`               | Stable `R*` requirement ID, assigned during specification                     |
| `title`            | Short human-readable summary                                                  |
| `type`             | `functional`, `non_functional`, `constraint`, `business_rule`, or `data_rule` |
| `component`        | Product area such as `auth`, `article`, `comment`, or `profile`               |
| `actor`            | User or system role                                                           |
| `action`           | Main verb or capability                                                       |
| `object`           | Entity acted on                                                               |
| `description`      | Complete normalized statement for the final schema                            |
| `conditions`       | Preconditions, triggers, or contextual rules                                  |
| `constraints`      | Hard limits, policy rules, technology restrictions, standards                 |
| `expected_actions` | Observable system results for the final schema                                |
| `source`           | Evidence record from input inventory                                          |
| `confidence`       | `high`, `medium`, or `low`                                                    |
| `status`           | `candidate`, `accepted`, `needs_review`, or `rejected`                        |
| `open_questions`   | Missing information for human review                                          |

---

## Splitting and Merging

### Split overloaded requirements

Split one source statement into multiple requirements when it contains independent
behaviors:

```text
Users can register, log in, edit profiles, and follow other users.
```

Split into sequential requirement records:

- `R1`: User can register.
- `R2`: User can log in.
- `R3`: User can edit profile.
- `R4`: User can follow another user.

### Merge duplicates

Merge candidates when they express the same actor, action, object, and expected
behavior. Keep all source references if multiple sources support the same requirement.

### Preserve variants

Do not merge if conditions differ:

- Login succeeds with valid credentials.
- Login fails with invalid credentials.

These may become separate acceptance criteria under one requirement, or separate
requirements if the source treats them independently.

---

## Constraints, Conditions, and Expected Behavior

Separate these concepts explicitly:

| Concept           | Meaning                      | Example                                        |
| ----------------- | ---------------------------- | ---------------------------------------------- |
| Condition         | When the requirement applies | "When the user is authenticated"               |
| Constraint        | A hard rule or limitation    | "Password must be at least 8 characters"       |
| Expected behavior | Observable result            | "The article is saved and visible in the feed" |

Avoid burying these in a long prose requirement. Downstream risk and test design depend
on these fields being easy to read.

---

## Requirement Types

Use these types consistently:

- `functional`: User/system capability, workflow, CRUD behavior.
- `non_functional`: Performance, security, reliability, usability, compatibility.
- `constraint`: Hard design, policy, standard, environment, or compliance limit.
- `business_rule`: Domain rule that governs behavior.
- `data_rule`: Validation, format, required field, range, uniqueness.

If a source mixes types, split it or keep the main behavior as `functional` and move
the hard rule into `constraints`.

---

## Priority Handling

Do not invent priority.

Use:

- `Must`, `Should`, `Could`, `Won't` only when source evidence supports it.
- `Unspecified` when no priority is provided.
- `candidate_priority` only when the user asks for a proposed prioritization.

Risk scoring and final test priority belong to downstream workflow steps.

---

## Ambiguity Handling

Mark `status: needs_review` when:

- Actor is unclear.
- Expected behavior is subjective or missing.
- Constraints conflict across sources.
- A term is domain-specific and undefined.
- A source implies behavior from implementation but no requirement states it.

Example:

```json
{
  "id": "R7",
  "description": "Article should be displayed quickly.",
  "input_fields": [],
  "data_ranges": [],
  "conditions": ["An article is requested"],
  "expected_actions": ["The article is displayed quickly."],
  "notes": "Status: needs_review. Confidence: low. Open questions: What response-time target defines 'quickly'? Which page or API endpoint is covered?"
}
```

---

## Analysis Checklist

Before handing off to specification:

- [ ] Each candidate is atomic and testable.
- [ ] Duplicates are merged or intentionally preserved.
- [ ] Conditions, constraints, and expected behavior are separate fields.
- [ ] Functional and non-functional requirements are distinguished.
- [ ] Inferred requirements are marked with lower confidence.
- [ ] Missing details are captured as open questions.
- [ ] No risk score, test case, or execution script is included.

---

## Related Practices

Before analysis:

- Use `requirements-elicitation.md` to collect source inventory.

After analysis:

- Use `requirements-specification.md` to assign IDs and produce structured output.
