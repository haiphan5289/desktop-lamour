---
description: "Translate a Figma design into a production-ready iOS UIKit ViewController + Storyboard for Cho Tot. Use when the target component needs a .storyboard file (bottom_sheet, full_screen, modal, tableview_onesection, tableview_multisection). Generates ViewController.swift + .storyboard + registers entries in project.pbxproj. Enforces UIStackView-based storyboard layout, DSLabel/DSButton outlets, CMStaticThemeLoader theming, SnapKit for programmatic constraints, and DSBottomSheetLayout protocol."
argument-hint: "FIGMA_URL: <url> MODULE_PATH: <path> COMPONENT_TYPE: <bottom_sheet|full_screen|modal|view_component|tableview_onesection|tableview_multisection>"
---

## Prompt Activation

**You are an expert iOS developer translating Figma designs into production-ready iOS UIKit code for Chợ Tốt.**

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code.

---

## How to Use This Prompt

Provide your input in this format:

```
FIGMA_URL: <Figma node URL, dev mode preferred>
MODULE_PATH: <Target folder path, e.g. ChoTot/Features/Job/VerticalizePos/Presentation/Ver2/Pos>
COMPONENT_TYPE: <bottom_sheet | full_screen | modal | view_component | tableview_onesection | tableview_multisection>
```

---

## Step-by-Step Workflow

**Follow all steps in order. Do not skip any step.**

---

### Step 0 — Verify Figma MCP Server

Before anything else:

1. Call `tool_search_tool_regex` with pattern `mcp_figma` to load all Figma MCP tools (they are deferred — must be loaded before use)
2. Call `mcp_figma_get_screenshot` with the node ID extracted from `FIGMA_URL`
3. If the call **succeeds**, proceed to Step 1
4. If the call **fails**, stop and report:

```
❌ Figma MCP Server is not reachable.
Fix:
  1. Open Figma Desktop app
  2. Reload VS Code window (Cmd+Shift+P → "Developer: Reload Window")
  3. Re-run this prompt
```

> Do NOT generate any code if the MCP server is unreachable — design context will be missing.

---

### Step 1 — Fetch Figma Design Context

Extract `file_key` and `node_id` from `FIGMA_URL`:

- **file_key**: segment after `/design/` (e.g. `GlkeqMpiIEcPpIAoHO6FKL`)
- **node_id**: value of `node-id` query param (e.g. `2703-10882`)

Then call **both in parallel**:

1. `mcp_figma_get_design_context(file_key, node_id, depth=4)` — extracts layout, colors, typography, component tree
2. `mcp_figma_get_screenshot(file_key, node_id)` — visual reference (source of truth for fidelity)

Analyze the result:
- Identify sections: header, body, footer
- Note layout direction, spacing, fills, font sizes/weights
- Identify interactive elements (buttons, close icon)

#### Step 1b — Extract Overridden Text Content (MANDATORY)

After getting the design context, inspect the `overrides` array in the raw JSON response.

For **every** override entry where `overriddenFields` contains `"characters"`, the text content of that node **will NOT appear** in the parent's `get_design_context` result — you must fetch it separately.

**Algorithm:**
1. Collect all override IDs where `overriddenFields` includes `"characters"`
2. Also collect IDs of button instances where `overriddenFields` includes `"componentProperties"` (key pattern: `"↳ Input Text#..."`)
3. Fetch all collected node IDs **in parallel** using `mcp_figma_get_design_context(file_key, node_id, depth=2)`
4. From TEXT nodes → read the `characters` field for label default text
5. From button INSTANCE nodes → read `componentProperties["↳ Input Text#..."].value` for button title

**Use these extracted strings as default values everywhere:**
- In `.storyboard`: set `text="..."` on `<label>` elements, `title="..."` on `<button state key="normal">`
- In `configureUI()`: use `?? "extracted text"` fallback on every `.text` / `setTitle(_:for:)` call

> If `overrides` is empty or no `"characters"` overrides exist, use the `characters` value already visible in the `get_design_context` structure output.

---

### Step 1c — Find Existing Similar UI (Ask Before Creating)

**First, ask the user:**

```
Would you like me to search the codebase for existing UI components
similar to this Figma design, so you can reuse or extend them instead
of creating from scratch? (yes / no)
```

> If the user answers **no** — skip this step entirely and proceed to Step 2.

If the user answers **yes**, search for existing components that visually match the Figma design.

**Search strategy (module-first, then expand):**

1. **Within the same module** (`MODULE_PATH`): Search for ViewControllers or Views with the same `COMPONENT_TYPE`
2. **Cross-module** (if nothing found): Expand search to sibling AppFeatures modules
3. **Key signals to match:** same structural pattern (header + body + footer), same interactive element count, same icon type, similar TypoToken hierarchy

**Search commands to run:**
```bash
# Find similar bottom sheets / warning dialogs in same module
grep -r "DSBottomSheetLayout" MODULE_PATH --include="*.swift" -l

# Find ViewControllers with two-button footer pattern
grep -r "secondaryButton\|primaryButton" MODULE_PATH --include="*.swift" -l

# Find warning/notice-style components cross-module
grep -r "warningFill\|noticeShare\|warningMessage" AppFeatures --include="*.swift" -l
```

**Present candidates to the user:**

If one or more matches are found, show:

```
Found similar existing UI:
  • CRNoticeShareAdViewController.swift (CTCorePayment) — bottom sheet with close + 2-button footer
  • JBWarningViewController.swift (CTJOB) — icon + title + description + primary button

→ Do you want to:
  [A] Reuse / extend one of these components
  [B] Create a new component from scratch
```

If no matches are found, inform the user and proceed directly to Step 2.

---

### Step 2 — Clarifying Questions (Ask BEFORE Writing Code)

Ask the user only the minimum required before generating files:

- **Button actions**: What does each button/close icon do? (dismiss, navigate, callback?)
- **File name**: What should the ViewController and storyboard be named? (e.g. `JBWarningMessage`)
- **Subfolder**: Which subfolder within `MODULE_PATH`? (confirm or ask if ambiguous)

Do NOT ask about design tokens, spacing, or colors — extract those from Figma.

---

### Step 3 — Explore Existing Patterns

Before writing code, search the module for:

1. How `DSBottomSheetLayout` is used in sibling ViewControllers
2. How `configureUI()` applies `DS.TypoToken.*` and `DS.Button.*`
3. Which theme is used: `CMStaticThemeLoader.jobTheme`, `.defaultTheme`, `.posTheme`, etc.
4. Whether SnapKit is used for programmatic constraints (always yes)

Then read the **canonical reference** by `COMPONENT_TYPE`:

| COMPONENT_TYPE | Reference files to read |
|---|---|
| `bottom_sheet` | `AppFeatures/CTCorePayment/CTCorePayment/Features/CheckoutPage/NoticeShareAd/CRNoticeShareAd.storyboard` + `CRNoticeShareAdViewController.swift` |
| `tableview_onesection` | `AppFeatures/CTCorePayment/CTCorePayment/Features/DongTot/TopupDongtot/HighValuePackage/CRHighValuePackage.storyboard` + `CRHighValuePackageViewController.swift` |
| `tableview_multisection` | `AppFeatures/CTPTY/CTPTY/Features/Subscription/SubscriptionSK/PTSubscriptionSK.storyboard` + `PTSubscriptionSKViewController.swift` |
| `full_screen` / `modal` | Search for a sibling storyboard in the same module folder |
| **cell (xib)** | `AppFeatures/CTCorePayment/CTCorePayment/Features/DongTot/TopupDongtot/Cell/Topvup/CRTopupDongtotCell.swift` + `CRTopupDongtotCell.xib` |

Replicate the reference's StackView structure exactly before writing the new storyboard.

#### Cell Creation Options

Cells in this project can be created in **two ways**. Choose based on existing patterns in the target module:

| Method | When to use | Example |
|---|---|---|
| `.xib` file | Standalone reusable cell, registered with `register(nib:forCellReuseIdentifier:)` | `CRTopupDongtotCell.xib` |
| `.storyboard` prototype | Cell is embedded as a prototype inside the feature's storyboard (registered automatically by storyboard) | Prototype cell inside `CRHighValuePackage.storyboard` |

> **Rule:** Check the parent ViewController's `viewDidLoad` / `tableView.register(...)` call to determine which method the module uses.

**Canonical cell (`.xib`) structure (`CRTopupDongtotCell`):**
- Root: `UITableViewCell` with `customClass="CRTopupDongtotCell"` in XIB
- `contentView` → `containerView` (UIView, rounded corners, border) → horizontal `UIStackView`
  - Left: vertical `UIStackView` (title + desc `DSLabel`)
  - Right: `PaddingLabel` (price badge, `customModule="CTComponent"`)
- Outlets: `containerView`, `packageTitleLabel` (DSLabel), `packageDescLabel` (DSLabel), `packagePriceLabel` (PaddingLabel)
- Styling in `awakeFromNib()` using `DS.TypoToken.*` and `theme.*` from `CMStaticThemeLoader`
- Data binding via `bindCellModel(package: IAPPackageProtocol)`

---

### CTDesignSystem Enforcement Rule (MANDATORY for ALL generated code)

> ⚠️ **EVERY function you generate — `configureUI()`, `awakeFromNib()`, `setupViews()`, `bindCellModel()`, `configure(with:)`, or any custom setup method — MUST use CTDesignSystem exclusively. No exceptions.**

**Prohibited patterns (never generate these):**

```swift
// ❌ Raw UIKit font/color — NEVER
label.font = UIFont.systemFont(ofSize: 14, weight: .bold)
label.textColor = UIColor.black
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

> The reference cell `CRTopupDongtotCell` uses older `DS.T14B` / `CTColor.*` APIs — these are **legacy**. Do NOT copy those patterns.

---

### Step 4 — Generate ViewController (.swift)

Use this exact structure:

```swift
//
//  <Name>ViewController.swift
//  ChoTot
//
//  Created by <git config user.name> on <current date>.
//  Copyright © 2024 Cho Tot. All rights reserved.
//

import UIKit
import SnapKit
import CTDesignSystem
import CTCommon
import CTAsset

final class <Name>ViewController: UIViewController, DSBottomSheetLayout {

    // MARK: - Outlets
    @IBOutlet private weak var titleLabel: DSLabel!
    @IBOutlet private weak var bodyTitleLabel: DSLabel!
    @IBOutlet private weak var descriptionLabel: DSLabel!
    @IBOutlet private weak var secondaryButton: DSButton!
    @IBOutlet private weak var primaryButton: DSButton!

    // MARK: - Properties
    var on<SecondaryAction>: (() -> Void)?
    var on<PrimaryAction>: (() -> Void)?
    private let theme = CMStaticThemeLoader.<moduleTheme>

    // MARK: - Lifecycle
    override func viewDidLoad() {
        super.viewDidLoad()
        configureUI()
    }

    deinit { Logger.print("\(self) deallocated.") }

    // MARK: - Private Methods
    private func configureUI() {
        titleLabel.setStyle(DS.TypoToken.Header.Section(color: theme.text.textPrimary.color))
        titleLabel.text = "<title from Figma>"

        bodyTitleLabel.setStyle(DS.TypoToken.Header.Page(color: theme.text.textPrimary.color))
        bodyTitleLabel.text = "<body title from Figma>"

        descriptionLabel.setStyle(DS.TypoToken.Body.Section(color: theme.text.textSecondary.color))
        descriptionLabel.text = "<description from Figma>"

        secondaryButton.setStyle(DS.Button.secondary(size: .medium, themeType: theme.type))
        secondaryButton.setTitle("<label>", for: .normal)

        primaryButton.setStyle(DS.Button.primary(size: .medium, themeType: theme.type))
        primaryButton.setTitle("<label>", for: .normal)
    }

    // MARK: - Actions
    @IBAction private func didTapSecondaryButton(_ sender: Any) {
        dismiss(animated: true) { [weak self] in self?.on<SecondaryAction>?() }
    }

    @IBAction private func didTapPrimaryButton(_ sender: Any) {
        dismiss(animated: true) { [weak self] in self?.on<PrimaryAction>?() }
    }

    @IBAction private func didTapClose(_ sender: Any) {
        dismiss(animated: true)
    }
}
```

**Rules:**
- Always `DSLabel`, `DSButton` — never `UILabel`, `UIButton` directly in style calls
- Match `DS.TypoToken.*` to Figma font size/weight (see token table below)
- Match `DS.Button.*` to Figma button fill (primary = yellow `#FFD400`, secondary = white/bordered)
- Use `CTAssetSystemIcon.*` for icons (`warningFill24px`, `closeOutline24px`, etc.)
- SnapKit for any programmatic constraints only; use IBOutlets for storyboard views
- Dismiss pattern: `dismiss(animated: true) { [weak self] in self?.on<Action>?() }`

---

### Step 5 — Generate Storyboard (.storyboard)

> ⚠️ Layout MUST use `UIStackView` as the primary structure driver.
> Never chain sections with leading/top/trailing/bottom anchor constraints.
> Replicate the canonical reference storyboard StackView structure exactly.

**Outer StackView structure (bottom_sheet pattern):**

```
Root View (white background)
└── mainStackView (vertical, spacing=0)
    → pinned to safeArea: top/leading/trailing + bottom >= 0
    ├── Drawer Header (UIView, fixed height 48)
    │    ├── Title (DSLabel, leading=16, trailing to closeButton-8, centerY)
    │    ├── Close Button (UIButton, trailing=16, centerY, 24x24)
    │    └── Separator (UIView, height=1, bottom=0, full width)
    ├── Body Container (UIView) — NOT a StackView, just a plain UIView
    │    └── Body StackView (UIStackView, vertical, alignment=center, spacing=16)
    │         pinned: top=24, leading=16, trailing=16, bottom=24
    │         ├── Illustration (UIImageView, 80x80 explicit constraints)
    │         └── Content StackView (vertical, spacing=8)
    │              ├── Body Title (DSLabel, textAlignment=center, numberOfLines=0)
    │              └── Description (DSLabel, textAlignment=center, numberOfLines=0)
    └── Footer (UIView)
         ├── Divider (UIView, height=1, top=0, full width)
         └── Button StackView (horizontal, distribution=fillEqually, spacing=8)
              top=16, leading=16, trailing=16, height=40, bottom=16
              ├── Secondary Button (DSButton)
              └── Primary Button (DSButton)
```

> ⚠️ **Body padding rule:** NEVER use `layoutMarginsRelativeArrangement` or `<layoutMargins>` on any `<stackView>` in storyboard XML — these cause "Failed to unarchive element named 'stackView'". Always use a wrapper `UIView` with explicit top/leading/trailing/bottom constraints.

**Storyboard XML rules:**
- `toolsVersion="23504"` and `plugIn version="23506"`
- Add `<freeformSimulatedSizeMetrics key="simulatedDestinationMetrics"/>` for bottom sheets
- `DSLabel` / `DSButton` as `customClass` with `customModule="CTDesignSystem"`
- ViewController `customModule` matches the Xcode target (e.g. `"ChoTot"`) — check sibling storyboards
- All outlets wired in `<connections>` at the ViewController scene level
- `storyboardIdentifier` must match the ViewController class name exactly
- Use `distribution="fillEqually"` on horizontal button StackViews — no explicit width constraints
- Body padding → wrapper `UIView` + constraints, NOT `layoutMarginsRelativeArrangement`
- XML comments: ASCII only — no Unicode / box-drawing characters

---

### Step 6 — Register in Xcode Project (project.pbxproj)

The number of pbxproj entries depends on `COMPONENT_TYPE`.

#### 6a — bottom_sheet / full_screen / modal / view_component (2 files → 5 entries)

| Section | Entry |
|---|---|
| `PBXBuildFile` | `<UUID> /* <Name>ViewController.swift in Sources */` |
| `PBXBuildFile` | `<UUID> /* <Name>.storyboard in Resources */` |
| `PBXFileReference` | Swift file (`lastKnownFileType = sourcecode.swift`) |
| `PBXFileReference` | Storyboard file (`lastKnownFileType = file.storyboard`) |
| `PBXGroup` (target folder) | Both file refs listed under the correct group |
| Sources build phase | ViewController build file UUID |
| Resources build phase | Storyboard build file UUID |

#### 6b — tableview_onesection / tableview_multisection (4+ files → 10+ entries)

**Full file list to register:**

| File | Type |
|---|---|
| `<Name>ViewController.swift` | Sources |
| `<Name>.storyboard` | Resources |
| `<Name>ViewModel.swift` | Sources |
| `Cell/<Name>Cell.swift` | Sources |
| `Cell/<Name>Cell.xib` *(if cell uses .xib)* | Resources |

> If the cell is a **prototype cell inside the storyboard**, no `.xib` entry is needed. If it is a **standalone `.xib`**, add `PBXBuildFile` + `PBXFileReference` + Resources build phase entry.

**pbxproj entries required (minimum 10):**

| Section | Entry |
|---|---|
| `PBXBuildFile` x4 | ViewController.swift in Sources, storyboard in Resources, ViewModel.swift in Sources, Cell.swift in Sources |
| `PBXFileReference` x4 | One per file above |
| `PBXGroup` — feature folder | Contains storyboard + ViewController + ViewModel + Cell subfolder ref |
| `PBXGroup` — Cell subfolder | Contains Cell.swift |
| Sources build phase | 3 UUIDs (ViewController, ViewModel, Cell) |
| Resources build phase | 1 UUID (storyboard) |

**Group structure in pbxproj:**

```
<FeatureName>/ (PBXGroup)
├── Cell/ (PBXGroup)
│    └── <Name>Cell.swift
├── <Name>.storyboard
├── <Name>ViewController.swift
└── <Name>ViewModel.swift
```

**UUID generation (generate one per file):**
```bash
uuidgen | tr -d '-' | cut -c1-24
```

**Finding the correct group:** Search `project.pbxproj` for a **sibling file** already in the same `MODULE_PATH` folder to locate the parent group UUID and insert the new feature group as a child of it.

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

## Completion Checklist

Before finishing, verify:

- [ ] ViewController IBOutlets match storyboard outlet connections exactly
- [ ] All `@IBAction` selectors match storyboard action connections
- [ ] `storyboardIdentifier` matches the ViewController class name
- [ ] SnapKit used for any programmatic constraints (no `NSLayoutConstraint`)
- [ ] Storyboard root section is a vertical UIStackView (not anchor-chained views)
- [ ] Header/body/footer are StackView children — not manually top/bottom chained
- [ ] Button rows use `distribution=fillEqually` StackView (not equal-width constraints)
- [ ] Body padding done via a wrapper `UIView` with explicit constraints — NEVER `layoutMarginsRelativeArrangement` or `<layoutMargins>` in XML
- [ ] No XML comments with non-ASCII characters inside the storyboard XML
- [ ] pbxproj entries added: 5 for `bottom_sheet/full_screen/modal`; 10+ for `tableview_onesection/tableview_multisection`
- [ ] File header has correct `git config user.name` and current date
- [ ] `deinit { Logger.print("\(self) deallocated.") }` present
- [ ] `[weak self]` used in dismiss closures

---

## Common Issues and Solutions

### "Failed to unarchive element named 'stackView'" when opening storyboard

**Root cause 1 — `layoutMarginsRelativeArrangement` + `<layoutMargins>` XML element:**
Never use `layoutMarginsRelativeArrangement="YES"` or `<layoutMargins key="layoutMargins" .../>` in generated storyboards. Use a wrapper `UIView` with explicit constraints instead:

```xml
<view id="body-container">
    <subviews>
        <stackView id="body-stack">
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
Never use box-drawing chars (U+2500) or any non-ASCII inside XML comments in storyboards.

---

### Figma node not found
**Cause:** Node ID uses `-` separator in URL but Figma API uses `:`.
**Solution:** The proxy auto-converts — pass the raw URL parameter directly.

### Storyboard file is too large to write
**Solution:** Write the storyboard in sections — header StackView first, then body, then footer.

### pbxproj group UUID not found
**Solution:** Search for the parent folder's group instead (e.g. `Ver2/Pos` → search `Ver2`) and add a new subgroup.

### DSButton / DSLabel not found as customClass in storyboard
**Solution:** Use the module name from the `.xcodeproj` target (e.g. `ChoTot`, not `CTCorePayment`) — check sibling storyboards for the correct value.

---

## Example

```
FIGMA_URL: https://www.figma.com/design/GlkeqMpiIEcPpIAoHO6FKL/Revenue-Handoff-2026?node-id=2703-10882&m=dev
MODULE_PATH: ChoTot/Features/Job/VerticalizePos/Presentation/Ver2/Pos
COMPONENT_TYPE: bottom_sheet
```

**Expected output:**
1. `JBWarningMessageViewController.swift` — ViewController with DSBottomSheetLayout, IBOutlets, configureUI(), IBActions
2. `JBWarningMessage.storyboard` — UIStackView-based layout with DSLabel/DSButton custom classes, outlets wired
3. 5 pbxproj entries in `ChoTot.xcodeproj/project.pbxproj`
