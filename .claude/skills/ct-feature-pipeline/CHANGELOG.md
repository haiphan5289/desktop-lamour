# Changelog — ct-feature-pipeline

## [1.1.0] — 2026-04-08

### Changed
- Restructured skill into modular files (Option A — Split & Reference)
  - `SKILL.md` → lightweight entry point with file index
  - `INPUT_SCHEMA.md` → input parameters and format
  - `OUTPUT_SCHEMA.md` → phase output formats and final summary template
  - `PROMPT.md` → phase execution logic (Phases 0–3) + TodoWrite rules + fallback
  - `EXAMPLES.md` → POS Warning Message worked example (both input variants)
  - `EVAL.md` → per-phase output quality checklist
  - `GUARDRAILS.md` → anti-hallucination, encoding rules, phase constraints
  - `POSTPROCESS.md` → post-pipeline next steps (swiftlint, DI, build)
  - `CHANGELOG.md` → this file
  - `examples/pos-warning-message.md` → structured test case
  - `tools/` → placeholder for future helper scripts

---

## [1.0.0] — Initial Release

### Added
- 3-phase pipeline: `ct-semantic-filter` → `ct-generate-usecase` → `ct-figma-storyboard`
- Optional Phase 0: Jira ticket fetch via Atlassian MCP
- Mandatory `TodoWrite` progress tracking with lifecycle enforcement
- Vietnamese UTF-8 encoding guardrails (no `\u{XXXX}` escapes)
- POS Warning Message worked example (Jira + manual PRD variants)
- Fallback instructions for unresolved `@`-references
