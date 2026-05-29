---
parent-skill: requirement-structuring
reference-type: input-intake
load-when: Reading local project files, requirement documents, CSV-like tables, README/API docs, issue text, or agent CLI/user input for requirement structuring
---

# Requirement Input Intake Reference

**Parent Skill:** requirement-structuring
**When to Use:** Collecting and recording requirement sources before extraction.

This reference explains how to gather requirement input from local files or
user/agent CLI text without turning the task into full project orchestration.

---

## Purpose & Context

**What this achieves**: Build a trustworthy inventory of sources that can support a
structured requirement specification.

**When to apply**:

- The user provides a local project path.
- The user provides requirement text, a CSV/table, issue export, or CLI transcript.
- The target app has README, docs, API specs, stories, or existing requirement files.
- Downstream workflows need evidence-backed requirements.

**Prerequisites**:

- Target app or input text is available.
- The task scope is limited to requirement input and structuring.
- The output schema is known or can use the parent skill default.

---

## Input Modes

### Local Project Files

Prioritize files likely to contain requirements:

1. `README*`, `docs/**`, `requirements*`, `spec*`, `user-stories*`.
2. API specs such as OpenAPI/Swagger, Postman collections, GraphQL schemas.
3. Issue exports, backlog CSV files, product notes, examples, or fixtures.
4. Configuration and route files only when docs are missing.
5. Source code only for inferred requirements, marked as `inferred`.

Use `rg --files` to list candidate files. Use targeted reads rather than loading the
whole project. Skip generated directories such as `node_modules`, `dist`, `build`,
`.git`, coverage output, binary assets, and lockfiles unless the user specifically
asks for them.

### Requirement Documents

For Markdown, text, or Word/PDF-derived text:

- Preserve headings as context.
- Treat bullet lists and numbered scenarios as requirement candidates.
- Record section names or line numbers when available.
- Keep quoted evidence short and source-specific.

### CSV or Table-Like Input

Map common columns to the output contract:

| Input column                 | Output field                                                            |
| ---------------------------- | ----------------------------------------------------------------------- |
| ID / Req ID                  | `id`; normalize to `R1`, `R2`, `R3` style                               |
| Feature / Module / Component | `feature_id`, `feature_name`, or `notes`                                |
| Requirement / Description    | `description`                                                           |
| Actor / Role                 | top-level `actors` or requirement `notes`                               |
| Condition / Precondition     | `conditions`                                                            |
| Constraint / Rule            | top-level `input_constraints`, `business_rules`, or requirement `notes` |
| Expected Result              | `expected_actions`                                                      |
| Priority                     | `notes`                                                                 |
| Source                       | `source_path`, input inventory, or requirement `notes`                  |

If columns are missing, keep values as `Unspecified` and add an open question instead
of inventing details.

### Agent CLI or User Input

Treat the user's message, agent CLI transcript, or pasted prompt as a source record:

```json
{
  "source_id": "SRC-CLI-001",
  "kind": "agent_cli_input",
  "locator": "user message",
  "summary": "Free-form description of desired behavior",
  "trust_level": "user_supplied"
}
```

Extract explicit requirements first. If the CLI input contains implementation wishes
or design suggestions, separate them from requirements unless they express a hard
constraint.

---

## Requirement Input Inventory

Always produce an inventory before or alongside extracted requirements:

```json
{
  "input_inventory": [
    {
      "source_id": "SRC-DOC-001",
      "kind": "local_file",
      "path": "README.md",
      "locator": "Authentication section",
      "used": true,
      "reason": "Contains user-facing login and registration requirements"
    },
    {
      "source_id": "SRC-CODE-001",
      "kind": "local_file",
      "path": "src/routes/auth.ts",
      "used": false,
      "reason": "Implementation file skipped because README already states behavior"
    }
  ]
}
```

Source IDs should be stable inside one run:

- `SRC-DOC-NNN` for documents.
- `SRC-CSV-NNN` for CSV/table inputs.
- `SRC-CLI-NNN` for user or agent CLI input.
- `SRC-CODE-NNN` for implementation-derived evidence.

---

## Source Evidence Rules

Use evidence to make the requirement auditable:

- Prefer exact source path plus heading, row number, or line reference.
- Keep evidence snippets short.
- Mark `source.kind` accurately: `local_file`, `csv`, `requirement_doc`,
  `agent_cli_input`, `user_input`, or `inferred_from_code`.
- Use `confidence: high` for explicit requirement statements.
- Use `confidence: medium` or `low` for inferred behavior.
- Add `open_questions` when evidence is incomplete.

Do not:

- Treat every code branch as a user requirement.
- Turn implementation details into requirements unless the source says they are
  constraints.
- Hide assumptions in the requirement text.
- Skip source recording because "the behavior seems obvious."

---

## Local Project Intake Checklist

Before extraction, verify:

- [ ] Candidate requirement sources were listed.
- [ ] Generated, dependency, and binary files were excluded.
- [ ] Each used source has `source_id`, type, path/text locator, and reason.
- [ ] User/CLI input is recorded as a first-class source when present.
- [ ] Implementation-derived requirements are marked as inferred.
- [ ] Skipped sources are listed when their exclusion matters.

---

## Related Practices

After input intake:

- Use `requirements-analysis.md` to extract and normalize requirement items.
- Use `requirements-specification.md` to produce the final structured specification.
