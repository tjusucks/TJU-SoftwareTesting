# RequirementInputV1 Template

Use this template before sending text into prompt.

```yaml
project_name: "<project-name>"
feature_name: "<feature-name>"
actors:
  - "<actor-1>"
preconditions:
  - "<system precondition>"
business_rules:
  - "<rule-1>"
input_constraints:
  - "<input or boundary constraint>"
error_conditions:
  - "<error case>"
requirement_items:
  - id: "R1"
    text: "<requirement sentence>"
    priority: "high"
  - id: "R2"
    text: "<requirement sentence>"
    priority: "medium"
```

## Authoring checklist

- keep each requirement atomic (one behavior per entry)
- include explicit numeric boundaries whenever available
- include invalid/exception behavior explicitly
- avoid mixing UI and API behavior in one requirement ID
