# Input Schema — ct-feature-pipeline

## Input Format

```
FEATURE_NAME: <e.g. "POS Warning Message">
JIRA_TICKET_URL: <optional — e.g. https://701search.atlassian.net/browse/CRE-13492 — if provided, PRD is auto-fetched from Jira via Atlassian MCP>
PRD_CONTENT:                       ← optional if JIRA_TICKET_URL is given
"""
[Paste full PRD here — sensitive data will be filtered in Phase 1]
"""
REFERENCE_USECASE: <e.g. CRStatusVideoAIUseCase>
FIGMA_URL: <e.g. https://www.figma.com/design/GlkeqMpiIEcPpIAoHO6FKL/...?node-id=2703-10882>
MODULE_PATH: <e.g. ChoTot/Features/Job/VerticalizePos/Presentation/Ver2/Pos>
COMPONENT_TYPE: <bottom_sheet | full_screen | modal | view_component | tableview_onesection | tableview_multisection>
```

---

## Parameter Reference

| Parameter | Required | Description |
|-----------|----------|-------------|
| `FEATURE_NAME` | Yes | Short name used to derive UseCase class name and ViewController name |
| `JIRA_TICKET_URL` | No | If provided, PRD is auto-fetched from Jira. Skips need for `PRD_CONTENT` |
| `PRD_CONTENT` | Conditional | Required if `JIRA_TICKET_URL` is not provided. Supplements Jira content if both given |
| `REFERENCE_USECASE` | No | Existing UseCase class to use as a naming/pattern reference in Phase 2. If omitted, Phase 2 asks clarifying questions |
| `FIGMA_URL` | No | Figma design URL for Phase 3 UI generation. If omitted, Phase 3 is skipped |
| `MODULE_PATH` | Yes | Target module path for generated files (relative to project root) |
| `COMPONENT_TYPE` | Yes | UI component type determining storyboard template used in Phase 3 |

---

## Notes

- At least one of `JIRA_TICKET_URL` or `PRD_CONTENT` must be provided
- If both are provided, Jira content is primary and `PRD_CONTENT` is a manual supplement
- `COMPONENT_TYPE` must be one of the supported values listed above
