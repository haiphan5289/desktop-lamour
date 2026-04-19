---
name: ct-feature-pipeline
description: End-to-end feature pipeline for Cho Tot iOS — single input (PRD + Figma URL + module) auto-runs 3 phases in sequence: ct-semantic-filter → ct-generate-usecase → ct-figma-storyboard. Supports optional Jira ticket URL to auto-fetch PRD via Atlassian MCP. Use when starting a new feature from PRD to production-ready code.
---

# [CT] Feature Pipeline — PRD → UseCase → UI

Single-entry orchestrator. Input once → output: filtered PRD + 6-layer UseCase + ViewController + Storyboard + pbxproj entries.

---

## File Structure

| File | Purpose |
|------|---------|
| [spec/INPUT_SCHEMA.md](spec/INPUT_SCHEMA.md) | Input parameters and format |
| [spec/OUTPUT_SCHEMA.md](spec/OUTPUT_SCHEMA.md) | Expected output format per phase |
| [spec/PROMPT.md](spec/PROMPT.md) | Phase execution logic (Phases 0–3) |
| [spec/EXAMPLES.md](spec/EXAMPLES.md) | POS Warning Message worked example |
| [spec/EVAL.md](spec/EVAL.md) | Per-phase output quality checklist |
| [spec/GUARDRAILS.md](spec/GUARDRAILS.md) | Anti-hallucination, encoding rules, phase constraints |
| [spec/POSTPROCESS.md](spec/POSTPROCESS.md) | Post-pipeline next steps checklist |
| [CHANGELOG.md](CHANGELOG.md) | Version history |
| [examples/](examples/) | Full test case inputs/expected outputs |

---

## Execution

Load and execute: **[spec/PROMPT.md](spec/PROMPT.md)**

Sub-skills loaded per phase:
- Phase 1: `@.claude/skills/ct-semantic-filter/SKILL.md`
- Phase 2: `@.claude/skills/ct-generate-usecase/SKILL.md`
- Phase 3: `@.claude/skills/ct-figma-storyboard/SKILL.md`

See [spec/GUARDRAILS.md](spec/GUARDRAILS.md) before running.
