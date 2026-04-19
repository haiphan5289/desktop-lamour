---
name: ct-figma-storyboard
description: Translate a Figma design into a production-ready iOS UIKit ViewController + Storyboard for Cho Tot. Use THIS SKILL when the target component needs a .storyboard file (bottom_sheet, full_screen, modal, tableview_onesection, tableview_multisection). Follows the exact ct-ai-figma-to-ios-ui.prompt.md workflow: fetch Figma MCP context → ask clarifying questions → explore existing patterns → generate ViewController.swift + .storyboard → register 5 entries in project.pbxproj. Enforces UIStackView-based storyboard layout, DSLabel/DSButton outlets, CMStaticThemeLoader theming, SnapKit for programmatic constraints, and DSBottomSheetLayout protocol. Different from ct-figma-implement-design (which uses SnapKit-only, no storyboard).
metadata:
  mcp-server: figma
---

# Figma → iOS UIKit Storyboard Implementation

Single-entry skill. Input once → output: ViewController.swift + .storyboard + pbxproj entries.

---

## File Structure

| File | Purpose |
|------|---------|
| [spec/INPUT_SCHEMA.md](spec/INPUT_SCHEMA.md) | Input parameters and format |
| [spec/OUTPUT_SCHEMA.md](spec/OUTPUT_SCHEMA.md) | Expected output format |
| [spec/PROMPT.md](spec/PROMPT.md) | Step-by-step execution workflow (Steps 0–6) |
| [spec/EXAMPLES.md](spec/EXAMPLES.md) | Worked example (JBWarningMessage bottom sheet) |
| [spec/EVAL.md](spec/EVAL.md) | Output quality checklist |
| [spec/GUARDRAILS.md](spec/GUARDRAILS.md) | Anti-hallucination, encoding rules, CTDesignSystem enforcement, common issues |
| [spec/POSTPROCESS.md](spec/POSTPROCESS.md) | Post-generation next steps checklist |
| [CHANGELOG.md](CHANGELOG.md) | Version history |

---

## Execution

Load and execute: **[spec/PROMPT.md](spec/PROMPT.md)**

See [spec/GUARDRAILS.md](spec/GUARDRAILS.md) before running.
