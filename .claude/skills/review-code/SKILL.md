---
name: review-code
description: "SwiftUI code review for Chợ Tốt iOS — CT Design System compliance, MVVM patterns, state management, color/typography/spacing tokens, and SwiftLint rules. Use when asked to review SwiftUI code."
argument-hint: "[file path or code to review] [focus area: DS Components | Color Tokens | Typography | Spacing Tokens | State Management | MVVM | SwiftLint All | Full Review]"
---

# SwiftUI Code Review Skill

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Full code review for SwiftUI files in the **Chợ Tốt iOS** app.

**Last synced:** 2026-03-25

---

## When to Use

Invoke this skill when asked to:
- Review a SwiftUI file or directory
- Check CT Design System compliance
- Audit MVVM architecture in SwiftUI
- Verify SwiftLint compliance

---

## Read Guide

| Task | File |
|------|------|
| Full review template, Few-Shot examples, SwiftLint rules | [references/review-code-swiftUI.md](./references/review-code-swiftUI.md) |

> Always read `references/review-code-swiftUI.md` before performing any review.

---

## Focus Areas

| Area | What Is Checked |
|------|----------------|
| `DS Components` | `.cdsButtonStyle()`, `CDSTextField`, `.cdsTextStyle()` vs raw SwiftUI |
| `Color Tokens` | `theme.*.*` sub-protocol access, no raw `Color.*` |
| `Typography` | `.cdsTextStyle()`, no `Font.system()` |
| `Spacing Tokens` | `DS.Gap.*`, `DS.Padding.*`, `DS.BorderRadius.*`, no hardcoded values |
| `State Management` | `@State`, `@StateObject`, `@ObservedObject`, `@EnvironmentObject` correctness |
| `MVVM` | No business logic in View body, proper ViewModel separation |
| `Memory Management` | `[weak self]`, retain cycles in Combine/closures |
| `SwiftLint All` | All rules from `.swiftlint.yml` |
| `Full Review` | All of the above combined |

---

## Key Rules (ALWAYS APPLY)

| ❌ Forbidden | ✅ Required |
|-------------|-------------|
| `Color.blue` | `theme.text.textBrand` |
| `Color(hex: "...")` | `theme.*.*` |
| `.padding(16)` | `.padding(DS.Padding.paddingMedium)` |
| `Font.system(size:)` | `.cdsTextStyle(...)` |
| `theme.textPrimary` | `theme.text.textPrimary` (sub-protocol) |
| ViewModel created in `body` | `@StateObject` with `init(flow:)` |
| `@Environment(\.presentationMode)` | `@Environment(\.dismiss)` |
| `as!`, `try!`, `!` | `as?` + guard, do/catch, guard let |
| `Button(action: {}) { }` | `Button(action: {}, label: { })` |

## Anti-Hallucination Rule

> **NEVER suggest a `CDS*` component unless it is verified in [references/review-code-swiftUI.md](./references/review-code-swiftUI.md).**
> If unsure — use raw SwiftUI with CT tokens instead.

**Verified components:** `CDSTextField`, `CDSCard`, `CDSBottomSheet`, `CDSBadge`, `CDSChip`, `CDSTag`, `CDSAvatar`, `CDSAsyncImage`, `CDSPopupView`, `CDSEmptyState`, `CDSSkeleton`, `CDSToast`, `CDSSnackBarView`

**Do NOT exist:** `CDSDivider`, `CDSLabel`, `CDSText`, `CDSImage`, `CDSStack`, `CDSButton`

---

## SwiftLint Source

**File:** `/Users/hai.phan/Desktop/haiphan/ct-ios-app--v3/.swiftlint.yml`
- `force_cast`, `force_try` → **error** (CI fails)
- `opening_brace`, `multiple_closures_with_trailing_closure` → default rules (always active)
- See full rules in [references/review-code-swiftUI.md](./references/review-code-swiftUI.md)
