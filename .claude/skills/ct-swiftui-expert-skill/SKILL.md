---
name: ct-swiftui-expert-skill
description: "Expert guidance for SwiftUI development in ChoTot iOS app — building views with CDS components, typography, MVVM-Combine patterns, and design system compliance. Use when implementing SwiftUI features, creating custom components, optimizing performance, managing state with @Published/@ObservedObject, handling Combine subscriptions, validating design system adherence (CDSButton, CDSTextField, .cdsTextStyle), or debugging view recomputation issues. Requires understanding of MVVM-C architecture, PassthroughRelay input patterns, and CTDesignSystemSwiftUI tokens."
model: sonnet
effort: medium
argument-hint: "[component or token type]"
---

# CT SwiftUI Expert Skill

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

This skill is the definitive guide for SwiftUI development at ChoTot, grounded in the `CTDesignSystemSwiftUI` core package.

## How to Use This Skill

**With arguments:**
```
/ct-swiftui-expert-skill CDSButton
/ct-swiftui-expert-skill state management in SwiftUI views
/ct-swiftui-expert-skill create a custom component
```

**Supported argument patterns:**
- **Component name**: `CDSButton`, `CDSTextField`, `CDSPopup`, etc.
- **Typography tokens**: `displayPage`, `headerSection`, `bodySection`
- **Feature/issue**: `state management`, `compose views`, `handle errors`, `optimize performance`
- **File context**: When selected text is provided via `{selectedText}`, the skill analyzes the code directly

---

**Your question:** $ARGUMENTS

Provide expert guidance specifically for: **$ARGUMENTS** in the context of SwiftUI development at ChoTot.

## Core Mandates

1.  **CDS Components Only**: Never use native `Button`, `TextField`, or `Text` without CDS styling.
2.  **Semantic Typography**: Use `.cdsTextStyle(.bodySection)` instead of `.font(.system(...))`.
3.  **Standardized Popups**: Use `.cdsPopup()` or `.cdsBottomSheet()` instead of native `alert()` or `sheet()`.
4.  **MVVM-Combine**: View state MUST be managed by a `ViewModel` exposed via `AnyViewModel`.

## Key Component Mappings

| Feature | CDS Usage | Modifiers / Notes |
| :--- | :--- | :--- |
| **Typography** | `Text("...").cdsTextStyle(.headerPage)` | See [Typography Reference](#typography) |
| **Buttons** | `Button("...").cdsButtonStyle(.primary)` | `.cdsButtonLoading(true)` for loading |
| **Input** | `CDSTextField(text: $t, placeholder: "...")` | `CDSTextView` for multiline |
| **Popups** | `.cdsPopup(isPresented: $p, title: "...")` | Standard modal dialogs |
| **Bottom Sheet**| `.cdsBottomSheet(isPresented: $p) { ... }` | Sliding panel from bottom |

## Typography System

ChoTot uses a semantic typography system based on `DS.TypoToken`.

```swift
// Common tokens:
.cdsTextStyle(.displayPage)      // 32 Bold
.cdsTextStyle(.headerSection)    // 16 SemiBold
.cdsTextStyle(.labelPage)       // 16 SemiBold
.cdsTextStyle(.bodySection)      // 14 Regular
.cdsTextStyle(.noteSection)      // 12 Regular Italic
```

## ViewModel Pattern (PassthroughRelay)

Always use `PassthroughRelay` for inputs to ensure a reactive, unidirectional flow.

```swift
// In ViewModel
private let fetchDataStream = PassthroughRelay<Void>()

func trigger(_ input: Input) {
    switch input {
    case .fetchData: fetchDataStream.accept()
    }
}
```

## String Substitutions Supported

The skill can automatically detect and use:
- `{selectedText}` — Code snippet you've selected for review or refactoring
- `{fileName}` — Current file name for context-aware suggestions
- `{filePath}` — Full file path for architecture recommendations

## Resources

- [references/architecture.md](references/architecture.md): MVVM + Combine deep dive.
- [references/cds_components.md](references/cds_components.md): Comprehensive list of CDS SwiftUI components.
- [references/components_api.md](references/components_api.md): Detailed API for BottomSheets, Popups, and Inputs.
