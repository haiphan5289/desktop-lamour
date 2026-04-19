# Eval — ct-feature-pipeline

Use this checklist to verify pipeline output quality after each run.

---

## Phase 0 — Jira Fetch

- [ ] Ticket key and host correctly parsed from `JIRA_TICKET_URL`
- [ ] `summary` and `description` extracted from Jira response
- [ ] `JIRA_PRD` stored in context and passed to Phase 1
- [ ] Fallback message shown if MCP is unreachable

## Phase 1 — Semantic Filter

- [ ] Sensitive data removed: revenue targets, employee names, competitor benchmarks, internal metrics
- [ ] API endpoint preserved exactly (path, HTTP method)
- [ ] Response schema preserved (field names and types)
- [ ] UI specifications preserved (component type, layout, colors, sizing)
- [ ] User stories preserved
- [ ] `FILTERED_PRD` is clean, parseable, and complete enough for Phase 2

## Phase 2 — UseCase Generation

- [ ] `Api` key added to `NetworkHelper` (lowercase, matches endpoint path)
- [ ] Target struct conforms to `Requestable` with correct `httpMethod`, `endpoint`, `parameters`
- [ ] Service method returns `Observable` and calls the correct Target
- [ ] Repository pass-through method added
- [ ] UseCase class conforms to `CTActionUseCaseType`
- [ ] ViewModel `execute[Name]()` method binds `elements`, `executing`, and `underlyingError`
- [ ] Model struct conforms to `Codable` with correct field names and `CodingKeys`
- [ ] All 6 layers present (NetworkHelper → Target → Service → Repository → UseCase → ViewModel)
- [ ] No new files created unless explicitly approved

## Phase 3 — UI Generation

- [ ] `ViewController.swift` generated at correct path under `MODULE_PATH`
- [ ] `.storyboard` generated at correct path under `MODULE_PATH`
- [ ] Exactly 5 `project.pbxproj` entries added
- [ ] ViewController uses `CTDesignSystem` components (`DSLabel`, `DSButton`, etc.)
- [ ] All layout constraints use `SnapKit` (no `NSLayoutConstraint`)
- [ ] No `\u{XXXX}` escape sequences in Vietnamese string literals

## Overall Pipeline

- [ ] `TodoWrite` called at start and end of every phase
- [ ] Progress header printed before each phase
- [ ] Only one task `in_progress` at a time
- [ ] `FILTERED_PRD` (not raw `PRD_CONTENT`) used as Phase 2 input
- [ ] Final summary printed with all phases accounted for
- [ ] SwiftLint command included in next steps
