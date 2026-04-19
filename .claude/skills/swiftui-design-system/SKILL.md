---
name: swiftui-design-system
description: "CT Design System tokens and components for SwiftUI. Use for colors, typography, spacing, buttons, inputs, cards — always use CT tokens, never raw values."
argument-hint: "[component or token type]"
---

# CT Design System Skill

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Complete reference for CTDesignSystem tokens and components (v3.0+).

**Last synced:** 2026-01-26

---

## Setup

- `DS.registerFonts()` in App init (REQUIRED for custom fonts)
- `import CTDesignSystemSwiftUI` in every SwiftUI file
- `@Environment(\.colorTheme) var theme` in every View
- `.environment(\.colorTheme, .pty)` at app root

## Themes

`.chotot` (Yellow) | `.job` (Blue) | `.pty` (Orange-red) | `.veh` (Yellow)

## Read Guide

| Task | File |
|------|------|
| Color tokens, theme colors | [references/colors.md](./references/colors.md) |
| Text styles, fonts | [references/typography.md](./references/typography.md) |
| Spacing, padding, radius, borders | [references/spacing.md](./references/spacing.md) |
| UI components (buttons, inputs...) | [references/components.md](./references/components.md) |
| Hex→token conversion (Figma) | [references/color-mapping.yaml](./references/color-mapping.yaml) |
| SwiftUI code review (Few-Shot) | [../review-code/references/review-code-swiftUI.md](../review-code/references/review-code-swiftUI.md) |

## Forbidden (ALWAYS APPLY)

| ❌ Forbidden | ✅ Required |
|-------------|-------------|
| `Color.blue` | `theme.text.textBrand` |
| `Color(hex: "...")` | `theme.*.*` |
| `.padding(16)` | `.padding(DS.Padding.paddingMedium)` |
| `Font.system(size:)` | `.cdsTextStyle(...)` |
| `theme.textPrimary` | `theme.text.textPrimary` |

## Gotchas

- `DS.StrokeLine.strokeDivider` (struct member) vs `.strokeDivide` (CGFloat extension) — different names!
- `DS.BorderRadius.radiusCard.value()` — needs `.value()` call for CGFloat
- Color sub-protocol access: `theme.text.textPrimary` NOT `theme.textPrimary`
- Dark mode NOT available yet. All themes light mode only.

## Source

> ⚠️ Path contains a DerivedData build hash — if DerivedData is cleared, regenerate via `pod install` or build once.

**Package root:** `/Users/hai.phan/Library/Developer/Xcode/DerivedData/ChoTot-emrqzdagaqgtgleygywbvfeauazo/SourcePackages/checkouts/ct-ios-design-system-swiftui`

| Resource | Path |
|----------|------|
| README (overview + component status) | `…/README.md` |
| API Reference (authoritative component list) | `…/docs/API_REFERENCE.md` |
| Component Examples (copy-paste) | `…/docs/COMPONENT_EXAMPLES.md` |
| Best Practices | `…/docs/BEST_PRACTICES.md` |
| Integration Guide | `…/docs/INTEGRATION_GUIDE.md` |
| Troubleshooting | `…/docs/TROUBLESHOOTING.md` |
| Source code | `…/Sources/CTDesignSystemSwiftUI/Sources` |
| Demo App | `…/CTDesignSystemSwiftUIApp/` |
