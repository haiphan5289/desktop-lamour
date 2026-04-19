# Output Schema — ct-feature-pipeline

## Phase Progress Header (all phases)

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🔵 PHASE [N]/3 — [Phase Name]
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

---

## Phase 0 — Jira Fetch (conditional)

**Success:**
```
✅ Phase 0 complete — PRD fetched from Jira.
🔗 Ticket: [JIRA_TICKET_URL] — [ticket summary]
📋 PRD source: Jira[+ manual supplement if PRD_CONTENT was also given]
→ Proceeding to Phase 1...
```

**Failure / MCP unreachable:**
```
⚠️ Jira MCP unavailable — falling back to manual PRD_CONTENT.
Fix: Ensure .vscode/mcp.json has Atlassian MCP configured at https://mcp.atlassian.com/v1/mcp
```

---

## Phase 1 — Semantic Filter

```
✅ Phase 1 complete — PRD filtered.
🔒 Removed: [list of removed sensitive items]
✅ Preserved: [list of preserved technical items]
→ Proceeding to Phase 2...
```

---

## Phase 2 — UseCase Generation

```
✅ Phase 2 complete — UseCase generated.
📁 Files modified: [list of 6–11 files]
→ Proceeding to Phase 3...
```

Inline confirmation table:

| Layer | File | Change |
|-------|------|--------|
| NetworkHelper | `CRNetworkHelper.swift` | `Api.[key] = "[endpoint]"` |
| Target | `[Name]Targets.swift` | `struct [Name]Target: Requestable` |
| Service | `[Name]Services.swift` | `func [name](input:) -> Observable<...>` |
| Repository | `[Name]Repositories.swift` | Pass-through method |
| UseCase | `[Name]UseCase.swift` | `final class CR[Name]UseCase: CTActionUseCaseType` |
| ViewModel | `[Name]ViewModel.swift` | `func execute[Name](input:)` |
| Model | `[Name]Model.swift` | `public struct [Name]Model: Codable` |

---

## Phase 3 — UI Generation

```
✅ Phase 3 complete — UI generated.
• [Name]ViewController.swift → [MODULE_PATH]/ViewControllers/
• [Name].storyboard → [MODULE_PATH]/
• project.pbxproj — 5 entries added
```

---

## Final Summary

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ PIPELINE COMPLETE — [FEATURE_NAME]
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Phase 1 — Semantic Filter
  🔒 Removed: [summary]
  ✅ Preserved: [summary]

Phase 2 — UseCase: [UseCaseName]
  Modified [N] files:
  • [file 1]
  • [file 2]
  • ...

Phase 3 — UI: [ViewControllerName]
  • [Name]ViewController.swift → [MODULE_PATH]/ViewControllers/
  • [Name].storyboard → [MODULE_PATH]/
  • project.pbxproj — 5 entries added

Next steps:
  1. Run: swiftlint lint --config .swiftlint.yml --strict [changed files]
  2. Wire ViewModel callbacks in ViewController
  3. Register DI in Assembler if not auto-detected
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```
