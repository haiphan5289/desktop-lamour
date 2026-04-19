# Guardrails — ct-figma-storyboard

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

---

## CTDesignSystem Enforcement Rule (MANDATORY for ALL Generated Code)

> ⚠️ **EVERY function you generate — `configureUI()`, `awakeFromNib()`, `setupViews()`, `bindCellModel()`, `configure(with:)`, or any custom setup method — MUST use CTDesignSystem exclusively. No exceptions.**

**Prohibited patterns (never generate these):**

```swift
// ❌ Raw UIKit font/color — NEVER
label.font = UIFont.systemFont(ofSize: 14, weight: .bold)
label.textColor = UIColor.black
label.textColor = .label
button.backgroundColor = UIColor(hex: "#FFD400")
view.layer.borderColor = UIColor.gray.cgColor
```

**Required patterns (always use these):**

```swift
// ✅ CTDesignSystem tokens
label.setStyle(DS.TypoToken.Label.Page(color: theme.text.textPrimary.color))
button.setStyle(DS.Button.primary(size: .medium, themeType: theme.type))
view.layer.borderColor = theme.line.linePrimary.color.cgColor
```

**Token mapping for common styling in cells:**

| What you want to style | Correct CTDesignSystem call |
|---|---|
| Title label (bold 14px) | `DS.TypoToken.Label.Page(color: theme.text.textPrimary.color)` |
| Subtitle / description | `DS.TypoToken.Body.Section(color: theme.text.textSecondary.color)` |
| Badge / price label | `DS.TypoToken.Label.Caption(color: theme.text.textPositive.color)` |
| Border color | `theme.line.linePrimary.color.cgColor` |
| Background tint | `theme.background.backgroundPrimary.color` |
| Warning/orange tint | `theme.background.backgroundWarningLight.color` |

> The reference cell `CRTopupDongtotCell` uses older `DS.T14B` / `CTColor.*` APIs — these are **legacy**. Do NOT copy those patterns. Use `DS.TypoToken.*` and `theme.*` from `CMStaticThemeLoader` instead.

---

## Design Token Mapping (Figma → CTDesignSystem)

| Figma | Swift |
|---|---|
| SemiBold 16px (header) | `DS.TypoToken.Header.Section(color:)` |
| SemiBold 20px (page title) | `DS.TypoToken.Header.Page(color:)` |
| Regular 14px (body) | `DS.TypoToken.Body.Section(color:)` |
| Bold 16px (label) | `DS.TypoToken.Label.Page(color:)` |
| Button fill `#FFD400` | `DS.Button.primary(size:, themeType:)` |
| Button white/bordered | `DS.Button.secondary(size:, themeType:)` |
| `rgba(34,34,34)` | `theme.text.textPrimary.color` |
| `rgba(89,89,89)` | `theme.text.textSecondary.color` |
| `rgba(251,115,40)` (orange warning) | `theme.background.backgroundWarningLight.color` |
| Warning icon | `CTAssetSystemIcon.warningFill24px(tint:)` |
| Close icon | `CTAssetSystemIcon.closeOutline24px()` |
| 1px divider line | `UIView` with `height = 1` constraint |

---

## Design System Tokens — NEVER Invent (MANDATORY)

**Do NOT fabricate DS token names.** Always verify every token against the actual codebase before using it.

- **Before using any `theme.*` property**, grep `CMBorder`, `CMBackground`, `CMText`, etc. in `Libraries/CTDesignSystem/CTDesignSystem/Theme/CMDefaultTheme.swift` to confirm the exact member name.
- **Before using any `DS.TypoToken.*` or `DS.Button.*`**, search the CTDesignSystem source to confirm it exists.
- **Never guess** token names like `borderPrimary`, `textPrimary`, `backgroundPrimary` without verification — they may not exist.

Verified `CMBorder` members (as of last check):
`borderDivider`, `borderThin`, `borderOverlay`, `borderRegular`, `borderBold`, `borderActive`, `borderBlank`, `borderBrand`, `borderInfo`, `borderSuccess`, `borderError`, `borderWarning`, `borderDisabled`, `borderBlack`

If unsure → grep the member before writing. A compile error from a fake token wastes a full build cycle.

---

## String Encoding — Vietnamese UTF-8 (MANDATORY)

Always write Vietnamese strings as direct UTF-8 literals. **Never use `\u{XXXX}` escape sequences** for human-readable text.

```swift
// ❌ BAD — \u{1eedi} is invalid: 'i' is not a hex digit
submitButton.setTitle("G\u{1eedi} th\u{00f4}ng tin", for: .normal)

// ✅ GOOD — direct UTF-8 literal, no escaping needed
submitButton.setTitle("Gửi thông tin", for: .normal)
```

`\u{XXXX}` is only acceptable for invisible/control characters (e.g., `\u{200B}` zero-width space) where the literal form is not legible.

---

## XIB Cell — Key Rules

- Use `awakeFromNib()` for setup, NOT `override init(style:reuseIdentifier:)` / `required init?(coder:)`
- All subviews are `@IBOutlet` — remove `lazy var` + SnapKit entirely (no SnapKit import needed)
- `customModule` in XIB must match the Xcode target name (e.g. `CTCorePayment`, not `ChoTot`)
- `DSRadioButton` → use `<view customClass="DSRadioButton" customModule="CTDesignSystem">` (not `<button>`)
- `DSLabel` → use `<label customClass="DSLabel" customModule="CTDesignSystem">`
- **NEVER** use `tableView.register(<Name>Cell.self)` for XIB-based cells — that form only works for storyboard prototype cells. For XIB cells it attempts to load a nib by class name from the main bundle and crashes.
- **NIB bundle error:** Using `bundle: nil` or `bundle: Bundle.main` for a cell in a framework (e.g. `CTCorePayment`) will crash at runtime: `"Could not load NIB in bundle: '...ChoTot.app' with name '<Name>Cell'"`. Always use `Bundle(for: <Name>Cell.self)` so the XIB is loaded from the framework bundle, not the app bundle.

> `theme.border.borderThin` is a verified `CMBorder` member. Never fabricate tokens like `borderPrimary`. Grep `CMDefaultTheme.swift` to verify any border/background/text token before use.

---

## Common Issues and Solutions

### ❌ Issue: "Failed to unarchive element named 'stackView'" when opening storyboard in Xcode

**Root cause 1 — `layoutMarginsRelativeArrangement` + `<layoutMargins>` XML element:**
This attribute/element combination is **NOT valid** in this project's storyboard XML format. Zero existing storyboards in the project use it. Xcode fails to parse the `<stackView>` element and throws the unarchive error.

**Fix:** Never use `layoutMarginsRelativeArrangement="YES"` or `<layoutMargins key="layoutMargins" .../>` in generated storyboards.
Instead, wrap the content in a plain `UIView` with explicit `top/leading/trailing/bottom` constraints to achieve padding:
```xml
<view id="body-container">          <!-- UIView fills StackView width -->
    <subviews>
        <stackView id="body-stack"> <!-- pinned: top=24, leading=16, trailing=16, bottom=24 -->
            ...
        </stackView>
    </subviews>
    <constraints>
        <constraint firstItem="body-stack" firstAttribute="top"      secondItem="body-container" secondAttribute="top"      constant="24"/>
        <constraint firstItem="body-stack" firstAttribute="leading"  secondItem="body-container" secondAttribute="leading"  constant="16"/>
        <constraint firstItem="body-container" firstAttribute="trailing" secondItem="body-stack" secondAttribute="trailing" constant="16"/>
        <constraint firstItem="body-container" firstAttribute="bottom"   secondItem="body-stack" secondAttribute="bottom"   constant="24"/>
    </constraints>
</view>
```

**Root cause 2 — XML comments with non-ASCII characters:**
Comments like `<!-- ── Header ── -->` (using box-drawing chars U+2500) or any non-ASCII inside XML comments can cause storyboard parse failures.

**Fix:** Use only plain ASCII in XML comments, or remove comments entirely from generated storyboards.

---

### Issue: Figma node not found
**Cause:** Node ID uses `-` separator in URL but Figma API uses `:`.
**Solution:** The proxy auto-converts — pass the raw URL; `mcp__figma__get_design_context` handles it.

---

### Issue: Storyboard file is too large to write
**Cause:** Complex multi-section layout with many nested views.
**Solution:** Write the storyboard in sections — header StackView first, then body, then footer. Verify XML validity with a quick sanity check (matching open/close tags).

---

### Issue: pbxproj group UUID not found
**Cause:** The module folder doesn't have a sibling file in pbxproj yet.
**Solution:** Search for the parent folder's group instead (e.g. `Ver2/Pos` → search `Ver2`) and add a new subgroup.

---

### Issue: DSButton / DSLabel not found as customClass in storyboard
**Cause:** `customModule` set to wrong module name.
**Solution:** Use the module name from the `.xcodeproj` target (e.g. `ChoTot`, not `CTCorePayment`) — check sibling storyboards for the correct value.
