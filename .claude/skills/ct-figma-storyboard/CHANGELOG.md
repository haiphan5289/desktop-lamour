# Changelog — ct-figma-storyboard

## [2.0.0] — 2026-04-09

### Changed
- Restructured skill into modular files matching ct-feature-pipeline pattern
  - `SKILL.md` → lightweight entry point with file index
  - `spec/INPUT_SCHEMA.md` → input parameters, COMPONENT_TYPE reference table
  - `spec/OUTPUT_SCHEMA.md` → generated file list and final confirmation template
  - `spec/PROMPT.md` → full step-by-step execution workflow (Steps 0–6), all cell templates (Priority 1/2/3), ViewController + storyboard generation rules, pbxproj registration
  - `spec/EXAMPLES.md` → worked examples: JBWarningMessage (bottom_sheet) + tableview_multisection pattern
  - `spec/EVAL.md` → output quality checklist (verbatim from v1.x)
  - `spec/GUARDRAILS.md` → CTDesignSystem enforcement rule, Design Token Mapping, encoding rules, XIB cell rules, common issues and solutions
  - `spec/POSTPROCESS.md` → post-generation steps (SwiftLint, outlet wiring, DI, build verification)
  - `CHANGELOG.md` → this file
  - `tools/` → placeholder for future helper scripts

---

## [1.0.0] — Initial Release

### Added
- Single SKILL.md covering full workflow: Steps 0–6
- Figma MCP server verification (Step 0)
- Overridden text extraction from Figma `overrides` array (Step 1b)
- Reuse-before-create search strategy (Step 1c)
- Canonical reference storyboard table by COMPONENT_TYPE (Step 3)
- Code pattern extraction from reference Swift file (Step 3a)
- Cell creation priority order: Programmatic > Storyboard prototype > XIB (Step 3a)
- ViewController template with DSBottomSheetLayout, CMStaticThemeLoader, IBOutlets (Step 4)
- Storyboard XML rules: UIStackView-only layout, no layoutMarginsRelativeArrangement (Step 5)
- pbxproj registration rules: 5 entries (bottom_sheet) vs 10+ entries (tableview) (Step 6)
- CTDesignSystem enforcement: DS.TypoToken.* and theme.* only, no UIFont/UIColor
- Design Token Mapping table (Figma → CTDesignSystem)
- Common issues: storyboard unarchive error, XIB bundle error, DSButton/DSLabel customModule
