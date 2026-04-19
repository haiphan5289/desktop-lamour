# Prompt — ct-figma-storyboard

> See [GUARDRAILS.md](GUARDRAILS.md) before executing any step.
> Input parameters are defined in [INPUT_SCHEMA.md](INPUT_SCHEMA.md).

---

## Step 0 — Verify Figma MCP Server

Before anything else:

1. Use `ToolSearch` to load all Figma MCP tools (they are deferred — search for `mcp__figma`)
2. Call `mcp__figma__get_screenshot` with the node ID extracted from `FIGMA_URL`
3. If the call **succeeds**, proceed to Step 1
4. If the call **fails**, stop and report:

```
❌ Figma MCP Server is not reachable.
Fix:
  1. Open VS Code Output panel → select "MCP: figma"
  2. Cmd+Shift+P → "MCP: Restart Server" → figma
  3. Verify scripts/figma-mcp-proxy/server.js exists
  4. Re-run this skill
```

> Do NOT generate any code if the MCP server is unreachable — design context will be missing.

---

## Step 1 — Fetch Figma Design Context

Extract `file_key` and `node_id` from `FIGMA_URL`:

- **file_key**: segment after `/design/` (e.g. `GlkeqMpiIEcPpIAoHO6FKL`)
- **node_id**: value of `node-id` query param (e.g. `2703-10882`)

Then call **both in parallel**:

1. `mcp__figma__get_design_context(file_key, node_id, depth=4)` — extracts layout, colors, typography, component tree
2. `mcp__figma__get_screenshot(file_key, node_id)` — visual reference (source of truth for fidelity)

Analyze the result:
- Identify sections: header, body, footer
- Note layout direction, spacing, fills, font sizes/weights
- Identify interactive elements (buttons, close icon)

### Step 1b — Extract Overridden Text Content (MANDATORY)

After getting the design context, inspect the `overrides` array in the raw JSON response.

For **every** override entry where `overriddenFields` contains `"characters"`, the text content of that node has been customised and **will NOT appear** in the parent's `get_design_context` result — you must fetch it separately.

**Algorithm:**
1. Collect all override IDs where `overriddenFields` includes `"characters"`
2. Also collect IDs of button instances where `overriddenFields` includes `"componentProperties"` (button label text is stored as a `TEXT` component property — key pattern: `"↳ Input Text#..."`)
3. Fetch all collected node IDs **in parallel** using `mcp__figma__get_design_context(file_key, node_id, depth=2)`
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

---

If the user answers **yes**, search the codebase for existing components that visually match the Figma design. This prevents duplication and enforces the "Reuse Over Recreation" principle.

**Search strategy (module-first, then expand):**

1. **Within the same module** (`MODULE_PATH`): Search for ViewControllers or Views with the same `COMPONENT_TYPE` (e.g., other bottom sheets, warning dialogs, two-button footers)
2. **Cross-module** (if nothing found): Expand search to sibling AppFeatures modules
3. **Key signals to match:**
   - Same structural pattern: header + body + footer, or icon + title + description + buttons
   - Same interactive element count: single button vs. two-button footer
   - Same icon type: warning icon, info icon, close button
   - Similar TypoToken hierarchy used (e.g., `Header.Section` + `Body.Section`)

**Search commands to run:**
```
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

## Step 2 — Clarifying Questions (Ask BEFORE Writing Code)

Ask the user only the minimum required before generating files:

- **Button actions**: What does each button/close icon do? (dismiss, navigate, callback?)
- **File name**: What should the ViewController and storyboard be named? (e.g. `JBWarningMessage`)
- **Subfolder**: Which subfolder within `MODULE_PATH`? (confirm or ask if ambiguous)

Do NOT ask about design tokens, spacing, or colors — extract those from Figma.

---

## Step 3 — Explore Existing Patterns

Before writing code, search the module for:

1. How `DSBottomSheetLayout` is used in sibling ViewControllers
2. How `configureUI()` applies `DS.TypoToken.*` and `DS.Button.*`
3. Which theme is used: `CMStaticThemeLoader.jobTheme`, `.defaultTheme`, `.posTheme`, etc.
4. Whether SnapKit is used for programmatic constraints (always yes)

Then read the **canonical reference** by `COMPONENT_TYPE`:

| COMPONENT_TYPE | Reference files to read |
|---|---|
| `bottom_sheet` | `AppFeatures/CTCorePayment/CTCorePayment/Features/CheckoutPage/NoticeShareAd/CRNoticeShareAd.storyboard` + `CRNoticeShareAdViewController.swift` |
| `tableview_onesection` | `AppFeatures/CTCorePayment/CTCorePayment/Features/UserDashBoard/AdOptimization/AdOptimizationAll/CRAdOptimizationAll.storyboard` + `CRAdOptimizationAll.swift` |
| `tableview_multisection` | `AppFeatures/CTPTY/CTPTY/Features/Subscription/SubscriptionSK/PTSubscriptionSK.storyboard` + `PTSubscriptionSKViewController.swift` |
| `full_screen` / `modal` | Search for a sibling storyboard in the same module folder |
| **cell (xib)** | `AppFeatures/CTCorePayment/CTCorePayment/Features/UserDashBoard/AdOptimization/AdOptimizationAll/Cell/CRAdOptimizationAllCell.swift` + `CRAdOptimizationAllCell.xib` |

> ⚠️ **MANDATORY: Read BOTH the storyboard AND the Swift ViewController of the reference.** The storyboard gives layout structure; the Swift file gives the code patterns you must replicate. Never generate a ViewController without first reading the reference Swift file.

### Step 3a — Extract Code Patterns From Reference Swift File (MANDATORY)

After reading the reference ViewController `.swift`, extract and record **all** of the following before writing any code:

| Pattern | What to extract | Example from `tableview_multisection` reference |
|---|---|---|
| **Data binding** | `UITableViewDataSource` (imperative) vs `RxDataSources` (reactive) | `RxTableViewSectionedReloadDataSource<Section>` |
| **Section model** | The section type used | `typealias Section = SectionModel<String, CellType>` |
| **Data relay** | How the table data is held | `BehaviorRelay<[Section]>` driven via `.drive(tableView.rx.items(dataSource:))` |
| **Cell type enum** | Enum of possible row types | `enum CellType { case header(...), tier, freeText(...) }` |
| **Cell registration** | `register(_:forCellReuseIdentifier:)` vs `register(_:)` helper | matches the module's convention |
| **Selection handling** | `didSelectRowAt` (delegate) vs `tableView.rx.itemSelected` (reactive) | `tableView.rx.itemSelected.subscribe(...)` |
| **Delegate split** | Whether DataSource is separated from Delegate | DataSource owned by RxDataSources, only `UITableViewDelegate` conformance remains |
| **Imports** | Which additional frameworks are imported | `import RxDataSources` |

**Replicate every extracted pattern exactly in the generated ViewController.** Do not fall back to `UITableViewDataSource` if the reference uses RxDataSources.

Replicate the reference's StackView structure exactly before writing the new storyboard.

### Cell Creation Options — Priority Order (MANDATORY)

Cells in this project can be created in **three ways**. **Always apply in priority order — use the first approach that fits:**

| Priority | Method | When to use | Example |
|---|---|---|---|
| **1 — Programmatic** | Separate `.swift` file + SnapKit | Default for `tableview_multisection`; when the reference VC uses `tableView.register(XCell.self)` + programmatic `init(style:reuseIdentifier:)` + SnapKit constraints | `PTHeaderSKCell.swift`, `CTSurveyReasonCell.swift` |
| **2 — Storyboard prototype** | Cell embedded inside the feature's `.storyboard` | When the feature already has a storyboard and cells are simple (1–2 labels, no complex subviews) AND the reference VC uses `tableView.register(XCell.self)` without a separate `.xib` | Prototype cell inside `CRHighValuePackage.storyboard` |
| **3 — XIB** | Separate `.xib` file + `@IBOutlet` | Only when the module already uses XIB-based cells (`register(nib:forCellReuseIdentifier:)`) OR the cell is reused across multiple ViewControllers | `CRTopupDongtotCell.xib` |

> **Decision rule:** Read the reference ViewController's cell registration calls (extracted in Step 3a). If it calls `tableView.register(SomeCell.self)` with no NIB, use **Priority 1 (Programmatic)**. If the storyboard has `<tableViewCell>` prototype nodes, use **Priority 2**. If it calls `register(nib:...)`, use **Priority 3 (XIB)**.

---

#### Priority 1 — Programmatic Cell (`.swift` + SnapKit)

> ⚠️ **UIStackView is MANDATORY as the main layout driver inside the cell** — never chain individual subviews with `leading / centerY / trailing` constraints directly. Use a `UIStackView` pinned inside `contentView` via SnapKit.

**Swift file template (programmatic cell):**

```swift
final class <Name>Cell: UITableViewCell {

    static let reuseId = "<Name>Cell"

    private lazy var <stack>: UIStackView = {
        let stack = UIStackView(arrangedSubviews: [<subviews>])
        stack.axis = .horizontal   // or .vertical
        stack.spacing = 12
        stack.alignment = .center
        return stack
    }()

    // lazy var subviews using CTDesignSystem components (DSLabel, DSButton, etc.)

    override init(style: UITableViewCell.CellStyle, reuseIdentifier: String?) {
        super.init(style: style, reuseIdentifier: reuseIdentifier)
        setupLayout()
    }

    required init?(coder: NSCoder) {
        super.init(coder: coder)
        setupLayout()
    }

    private func setupLayout() {
        selectionStyle = .none
        contentView.addSubview(<stack>)
        <stack>.snp.makeConstraints { make in
            make.top.bottom.equalToSuperview().inset(12)
            make.leading.trailing.equalToSuperview().inset(16)
        }
        // Fixed-size subviews get explicit width/height constraints inside SnapKit
    }

    func configure(with item: <ItemType>, theme: CTTheme) {
        // Apply DS.TypoToken.* and theme.* exclusively — never UIFont/UIColor
    }
}
```

**Cell registration in ViewController (programmatic):**
```swift
tableView.register(<Name>Cell.self, forCellReuseIdentifier: <Name>Cell.reuseId)
```

---

#### Priority 2 — Storyboard Prototype Cell

> ⚠️ **UIStackView is MANDATORY as the main layout driver** — the same rule as the main storyboard. Never chain individual subviews. Use a `<stackView>` element pinned inside a `containerView`.

**Storyboard prototype cell XML structure:**
```xml
<!-- Inside <tableView> in the feature's .storyboard -->
<tableViewCell contentMode="scaleToFill" selectionStyle="none"
               id="<CELL-ID>" customClass="<Name>Cell" customModule="ChoTot" customModuleProvider="target">
    <rect key="frame" x="0.0" y="0.0" width="393" height="56"/>
    <tableViewCellContentView key="contentView" id="<CONTENT-ID>">
        <subviews>
            <!-- containerView: optional card border wrapper -->
            <view translatesAutoresizingMaskIntoConstraints="NO" id="<CONTAINER-ID>">
                <subviews>
                    <!-- StackView: MANDATORY main layout driver -->
                    <stackView opaque="NO" contentMode="scaleToFill" alignment="center" spacing="12"
                               translatesAutoresizingMaskIntoConstraints="NO" id="<SV-ID>">
                        <subviews>
                            <!-- DSLabel, DSButton, UIImageView, etc. as arranged subviews -->
                            <label customClass="DSLabel" customModule="CTDesignSystem"
                                   translatesAutoresizingMaskIntoConstraints="NO" id="<LBL-ID>"/>
                        </subviews>
                        <constraints>
                            <!-- Fixed-size items only — let StackView handle spacing -->
                        </constraints>
                    </stackView>
                </subviews>
                <constraints>
                    <constraint firstItem="<SV-ID>" firstAttribute="top" secondItem="<CONTAINER-ID>" secondAttribute="top" constant="14" id="<C1>"/>
                    <constraint firstAttribute="bottom" secondItem="<SV-ID>" secondAttribute="bottom" constant="14" id="<C2>"/>
                    <constraint firstItem="<SV-ID>" firstAttribute="leading" secondItem="<CONTAINER-ID>" secondAttribute="leading" constant="12" id="<C3>"/>
                    <constraint firstAttribute="trailing" secondItem="<SV-ID>" secondAttribute="trailing" constant="12" id="<C4>"/>
                </constraints>
            </view>
        </subviews>
        <constraints>
            <constraint firstItem="<CONTAINER-ID>" firstAttribute="top" secondItem="<CONTENT-ID>" secondAttribute="top" constant="4" id="<C5>"/>
            <constraint firstAttribute="bottom" secondItem="<CONTAINER-ID>" secondAttribute="bottom" constant="4" id="<C6>"/>
            <constraint firstItem="<CONTAINER-ID>" firstAttribute="leading" secondItem="<CONTENT-ID>" secondAttribute="leading" constant="16" id="<C7>"/>
            <constraint firstAttribute="trailing" secondItem="<CONTAINER-ID>" secondAttribute="trailing" constant="16" id="<C8>"/>
        </constraints>
    </tableViewCellContentView>
    <connections>
        <!-- @IBOutlet connections for each subview -->
    </connections>
</tableViewCell>
```

**Swift file (storyboard prototype cell — uses `@IBOutlet`, NOT `lazy var` + SnapKit):**
```swift
final class <Name>Cell: UITableViewCell {
    @IBOutlet private weak var titleLabel: DSLabel!
    // other @IBOutlet subviews

    override func awakeFromNib() {
        super.awakeFromNib()
        setupUI()
    }

    private func setupUI() {
        selectionStyle = .none
        // DS.TypoToken.* and theme.* only
    }

    func configure(with item: <ItemType>, theme: CTTheme) { }
}
```

**Cell registration (storyboard prototype — class-only, no NIB needed):**
```swift
tableView.register(<Name>Cell.self)
```

---

#### Priority 3 — XIB Cell

**Canonical cell (`.xib`) structure (`CRHighValuePackagesCell` — primary reference):**

> ⚠️ **StackView is MANDATORY as the main layout driver inside the containerView** — same rule as storyboards. Never chain individual subviews with leading/centerY/trailing constraints directly. Use a horizontal `UIStackView` pinned inside the container.

Cell layout hierarchy:
```
contentView
└── containerView (UIView, rounded card border, top+4 bottom-4 leading+16 trailing-16)
    └── horizontalStackView (UIStackView, alignment=center, spacing=12, top+14 bottom-14 leading+12 trailing-12)
        ├── DSRadioButton (fixed 24x24)
        └── DSLabel (numberOfLines=0, fills remaining width via hugging priority)
```

**XIB file structure:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<document type="com.apple.InterfaceBuilder3.CocoaTouch.XIB" version="3.0" toolsVersion="23504" targetRuntime="iOS.CocoaTouch" propertyAccessControl="none" useAutolayout="YES" useTraitCollections="YES" useSafeAreas="YES" colorMatched="YES">
    <device id="retina6_12" orientation="portrait" appearance="light"/>
    <dependencies>
        <deployment identifier="iOS"/>
        <plugIn identifier="com.apple.InterfaceBuilder.IBCocoaTouchPlugin" version="23506"/>
        <capability name="Safe area layout guides" minToolsVersion="9.0"/>
        <capability name="System colors in document resources" minToolsVersion="11.0"/>
        <capability name="documents saved in the Xcode 8 format" minToolsVersion="8.0"/>
    </dependencies>
    <objects>
        <placeholder placeholderIdentifier="IBFilesOwner" id="-1" userLabel="File's Owner"/>
        <placeholder placeholderIdentifier="IBFirstResponder" id="-2" customClass="UIResponder"/>
        <tableViewCell contentMode="scaleToFill" selectionStyle="none" indentationWidth="10"
                       id="<CELL-ID>" customClass="<Name>Cell" customModule="<TargetModule>" customModuleProvider="target">
            <rect key="frame" x="0.0" y="0.0" width="375" height="64"/>
            <autoresizingMask key="autoresizingMask" flexibleMaxX="YES" flexibleMaxY="YES"/>
            <tableViewCellContentView key="contentView" opaque="NO" clipsSubviews="YES"
                                      multipleTouchEnabled="YES" contentMode="center"
                                      tableViewCell="<CELL-ID>" id="<CONTENT-ID>">
                <rect key="frame" x="0.0" y="0.0" width="375" height="64"/>
                <autoresizingMask key="autoresizingMask"/>
                <subviews>
                    <!-- containerView: rounded card, top+4 bottom-4 leading+16 trailing-16 -->
                    <view contentMode="scaleToFill" translatesAutoresizingMaskIntoConstraints="NO" id="<CONTAINER-ID>">
                        <rect key="frame" x="16" y="4" width="343" height="56"/>
                        <subviews>
                            <!-- horizontal StackView: main layout driver, pinned top+14 bottom-14 leading+12 trailing-12 -->
                            <stackView opaque="NO" contentMode="scaleToFill" alignment="center" spacing="12"
                                       translatesAutoresizingMaskIntoConstraints="NO" id="<SV-ID>">
                                <rect key="frame" x="12" y="14" width="319" height="28"/>
                                <subviews>
                                    <!-- DSRadioButton: fixed 24x24 inside StackView -->
                                    <view contentMode="scaleToFill" translatesAutoresizingMaskIntoConstraints="NO"
                                          id="<RADIO-ID>" customClass="DSRadioButton" customModule="CTDesignSystem">
                                        <rect key="frame" x="0.0" y="2" width="24" height="24"/>
                                        <color key="backgroundColor" red="0" green="0" blue="0" alpha="0"
                                               colorSpace="custom" customColorSpace="sRGB"/>
                                        <constraints>
                                            <constraint firstAttribute="width" constant="24" id="<UUID>"/>
                                            <constraint firstAttribute="height" constant="24" id="<UUID>"/>
                                        </constraints>
                                    </view>
                                    <!-- DSLabel: fills remaining width, numberOfLines=0 -->
                                    <label opaque="NO" userInteractionEnabled="NO" contentMode="left"
                                           horizontalHuggingPriority="251" verticalHuggingPriority="251"
                                           text="Label" textAlignment="natural" lineBreakMode="tailTruncation"
                                           numberOfLines="0" baselineAdjustment="alignBaselines"
                                           adjustsFontSizeToFit="NO" translatesAutoresizingMaskIntoConstraints="NO"
                                           id="<LABEL-ID>" customClass="DSLabel" customModule="CTDesignSystem">
                                        <rect key="frame" x="36" y="4" width="283" height="20"/>
                                        <fontDescription key="fontDescription" type="system" pointSize="17"/>
                                        <nil key="textColor"/>
                                        <nil key="highlightedColor"/>
                                    </label>
                                </subviews>
                            </stackView>
                        </subviews>
                        <color key="backgroundColor" systemColor="systemBackgroundColor"/>
                        <!-- Only 4 constraints needed: StackView pinned inside containerView -->
                        <constraints>
                            <constraint firstItem="<SV-ID>" firstAttribute="top" secondItem="<CONTAINER-ID>" secondAttribute="top" constant="14" id="<UUID>"/>
                            <constraint firstAttribute="bottom" secondItem="<SV-ID>" secondAttribute="bottom" constant="14" id="<UUID>"/>
                            <constraint firstItem="<SV-ID>" firstAttribute="leading" secondItem="<CONTAINER-ID>" secondAttribute="leading" constant="12" id="<UUID>"/>
                            <constraint firstAttribute="trailing" secondItem="<SV-ID>" secondAttribute="trailing" constant="12" id="<UUID>"/>
                            <constraint firstAttribute="height" relation="greaterThanOrEqual" constant="48" id="<UUID>"/>
                        </constraints>
                    </view>
                </subviews>
                <constraints>
                    <constraint firstItem="<CONTAINER-ID>" firstAttribute="top" secondItem="<CONTENT-ID>" secondAttribute="top" constant="4" id="<UUID>"/>
                    <constraint firstAttribute="bottom" secondItem="<CONTAINER-ID>" secondAttribute="bottom" constant="4" id="<UUID>"/>
                    <constraint firstItem="<CONTAINER-ID>" firstAttribute="leading" secondItem="<CONTENT-ID>" secondAttribute="leading" constant="16" id="<UUID>"/>
                    <constraint firstAttribute="trailing" secondItem="<CONTAINER-ID>" secondAttribute="trailing" constant="16" id="<UUID>"/>
                </constraints>
            </tableViewCellContentView>
            <viewLayoutGuide key="safeArea" id="<SA-ID>"/>
            <connections>
                <outlet property="containerView" destination="<CONTAINER-ID>" id="<UUID>"/>
                <outlet property="radioButton" destination="<RADIO-ID>" id="<UUID>"/>
                <outlet property="titleLabel" destination="<LABEL-ID>" id="<UUID>"/>
            </connections>
        </tableViewCell>
    </objects>
    <resources>
        <systemColor name="systemBackgroundColor">
            <color white="1" alpha="1" colorSpace="custom" customColorSpace="genericGamma22GrayColorSpace"/>
        </systemColor>
    </resources>
</document>
```

**Swift file — XIB-based cell (use `@IBOutlet` + `awakeFromNib`, NOT `lazy var` + SnapKit):**

```swift
final class <Name>Cell: UITableViewCell {

    static let reuseIdentifier = "<Name>Cell"
    private let theme = CMStaticThemeLoader.defaultTheme

    var enableHandler: ((Bool) -> Void)?

    @IBOutlet private weak var containerView: UIView!
    @IBOutlet private weak var radioButton: DSRadioButton!
    @IBOutlet private weak var titleLabel: DSLabel!

    override func awakeFromNib() {
        super.awakeFromNib()
        setupUI()
    }

    override func setSelected(_ selected: Bool, animated: Bool) {
        super.setSelected(selected, animated: animated)
        radioButton.isSelected = selected
        containerView.backgroundColor = selected
            ? theme.background.backgroundSecondary.color
            : theme.background.backgroundPrimary.color
        containerView.layer.borderColor = selected
            ? CTColor.orange300.cgColor
            : theme.border.borderThin.color.cgColor
        if selected { enableHandler?(true) }
    }

    private func setupUI() {
        selectionStyle = .none
        backgroundColor = .clear
        contentView.backgroundColor = .clear
        titleLabel.numberOfLines = 0
        titleLabel.setStyle(DS.TypoToken.Body.Section(color: theme.text.textPrimary.color))
        radioButton.textStyle = .icon
        radioButton.iconStyle = .radio
        radioButton.tintColor = CTColor.orange300
        radioButton.isUserInteractionEnabled = false
        containerView.layer.cornerRadius = 8
        containerView.layer.borderWidth = 1
        containerView.clipsToBounds = true
        containerView.backgroundColor = theme.background.backgroundPrimary.color
        containerView.layer.borderColor = theme.border.borderThin.color.cgColor
    }

    func configure(with model: <ModelType>) {
        titleLabel.text = model.<textProperty>
    }
}
```

**ViewController registration — two patterns, choose based on cell type:**

| Cell type | Registration | Why |
|---|---|---|
| Prototype cell in `.storyboard` | `tableView.register(<Name>Cell.self)` | Storyboard handles nib loading automatically |
| Standalone `.xib` file | `UINib(nibName:bundle:)` (see below) | Must explicitly load the nib from the correct bundle |

**Always split tableView setup into its own `setupTableView()` function** — never inline inside `configureUI()`.

```swift
private func configureUI() {
    titleLabel.setStyle(DS.TypoToken.Header.Section(color: theme.text.textPrimary.color))
    confirmButton.setStyle(DS.Button.primary(size: .large))
    confirmButton.isEnabled = false
    setupTableView()   // <-- always call from configureUI
}

private func setupTableView() {
    // registration + tableView config all go here
}
```

**Storyboard prototype cell (simple — class only):**
```swift
private func setupTableView() {
    // Cell is defined as a prototype inside the feature's .storyboard
    tableView.register(CRAdOptimizationAllCell.self)
    tableView.separatorStyle = .none
    tableView.rowHeight = UITableView.automaticDimension
    tableView.delegate = self
}
```

**Standalone XIB cell (must use UINib with explicit bundle):**
```swift
private func setupTableView() {
    // Cell has its own .xib file — MUST specify Bundle(for:) not nil
    let nib = UINib(nibName: <Name>Cell.reuseIdentifier, bundle: Bundle(for: <Name>Cell.self))
    tableView.register(nib, forCellReuseIdentifier: <Name>Cell.reuseIdentifier)
    tableView.separatorStyle = .none
    tableView.rowHeight = UITableView.automaticDimension
    tableView.estimatedRowHeight = 56
    tableView.delegate = self
}
```

> See [GUARDRAILS.md](GUARDRAILS.md) for the CTDesignSystem enforcement rule that applies to ALL generated cell and ViewController code.

---

## Step 4 — Generate ViewController (.swift)

Use this exact structure:

```swift
//
//  <Name>ViewController.swift
//  ChoTot
//
//  Created by <git config user.name> on <current date from mcp__time__get_current_time>.
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
- Match `DS.TypoToken.*` to Figma font size/weight (see Design Token Mapping in [GUARDRAILS.md](GUARDRAILS.md))
- Match `DS.Button.*` to Figma button fill (primary = yellow `#FFD400`, secondary = white/bordered)
- Use `CTAssetSystemIcon.*` for icons (`warningFill24px`, `closeOutline24px`, etc.)
- SnapKit for any programmatic constraints only; use IBOutlets for storyboard views
- Dismiss pattern: `dismiss(animated: true) { [weak self] in self?.on<Action>?() }`

---

## Step 5 — Generate Storyboard (.storyboard)

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
    ├── Body Container (UIView) ← NOT a StackView — just a plain UIView
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

> ⚠️ **Body padding rule:** NEVER use `layoutMarginsRelativeArrangement` or `<layoutMargins>` on any `<stackView>` in storyboard XML — these are not valid in this project and cause "Failed to unarchive element named 'stackView'". Always use a wrapper `UIView` with explicit top/leading/trailing/bottom constraints.

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

## Step 6 — Register in Xcode Project (project.pbxproj)

The number of pbxproj entries depends on `COMPONENT_TYPE`. The storyboard covers **all UI for the feature** — register every file that belongs to the feature, not just the ViewController.

---

### 6a — bottom_sheet / full_screen / modal / view_component (2 files → 5 entries)

| Section | Entry |
|---|---|
| `PBXBuildFile` | `<UUID> /* <Name>ViewController.swift in Sources */` |
| `PBXBuildFile` | `<UUID> /* <Name>.storyboard in Resources */` |
| `PBXFileReference` | Swift file (`lastKnownFileType = sourcecode.swift`) |
| `PBXFileReference` | Storyboard file (`lastKnownFileType = file.storyboard`) |
| `PBXGroup` (target folder) | Both file refs listed under the correct group |
| Sources build phase | ViewController build file UUID |
| Resources build phase | Storyboard build file UUID |

---

### 6b — tableview_onesection / tableview_multisection (4+ files → 10+ entries)

TableView-based features include a **ViewModel** and one or more **Cell** files in a `Cell/` subfolder. The storyboard covers all UI in the feature — all files must be registered.

**Full file list to register:**

| File | Type |
|---|---|
| `<Name>ViewController.swift` | Sources |
| `<Name>.storyboard` | Resources |
| `<Name>ViewModel.swift` | Sources |
| `Cell/<Name>Cell.swift` | Sources |
| `Cell/<Name>Cell.xib` *(if cell uses .xib)* | Resources |

> If the cell is a **prototype cell inside the storyboard**, no `.xib` entry is needed — it is already covered by the storyboard's Resources entry. If the cell is a **standalone `.xib`**, add one additional `PBXBuildFile` + `PBXFileReference` + Resources build phase entry for it.

**pbxproj entries required:**

**Without cell XIB (minimum 10 entries):**

| Section | Entry |
|---|---|
| `PBXBuildFile` x4 | ViewController.swift in Sources, storyboard in Resources, ViewModel.swift in Sources, Cell.swift in Sources |
| `PBXFileReference` x4 | One per file above |
| `PBXGroup` — feature folder | Contains storyboard + ViewController + ViewModel + Cell subfolder ref |
| `PBXGroup` — Cell subfolder | Contains Cell.swift |
| Sources build phase | 3 UUIDs (ViewController, ViewModel, Cell.swift) |
| Resources build phase | 1 UUID (storyboard) |

**With standalone cell XIB (minimum 12 entries — REQUIRED when cell is not a storyboard prototype):**

| Section | Entry |
|---|---|
| `PBXBuildFile` x5 | ViewController.swift in Sources, storyboard in Resources, ViewModel.swift in Sources, Cell.swift in Sources, **Cell.xib in Resources** |
| `PBXFileReference` x5 | One per file above (xib uses `lastKnownFileType = file.xib`) |
| `PBXGroup` — feature folder | Contains storyboard + ViewController + ViewModel + Cell subfolder ref |
| `PBXGroup` — Cell subfolder | Contains **both** Cell.swift + Cell.xib |
| Sources build phase | 3 UUIDs (ViewController, ViewModel, Cell.swift) |
| Resources build phase | 2 UUIDs (storyboard **and** Cell.xib) |

**Example pbxproj snippet for cell XIB (use this as template):**

```
/* PBXBuildFile */
<UUID> /* <Name>Cell.swift in Sources */ = {isa = PBXBuildFile; fileRef = <FILEREF-SWIFT-UUID>; };
<UUID> /* <Name>Cell.xib in Resources */ = {isa = PBXBuildFile; fileRef = <FILEREF-XIB-UUID>; };

/* PBXFileReference */
<FILEREF-SWIFT-UUID> /* <Name>Cell.swift */ = {isa = PBXFileReference; lastKnownFileType = sourcecode.swift; path = <Name>Cell.swift; sourceTree = "<group>"; };
<FILEREF-XIB-UUID> /* <Name>Cell.xib */ = {isa = PBXFileReference; lastKnownFileType = file.xib; path = <Name>Cell.xib; sourceTree = "<group>"; };

/* PBXGroup — Cell subfolder */
<GROUP-UUID> /* Cell */ = {
    isa = PBXGroup;
    children = (
        <FILEREF-SWIFT-UUID> /* <Name>Cell.swift */,
        <FILEREF-XIB-UUID> /* <Name>Cell.xib */,
    );
    path = Cell;
    sourceTree = "<group>";
};

/* Resources build phase — add BOTH storyboard AND xib */
<UUID> /* <Name>Cell.xib in Resources */,
```

**Group structure in pbxproj (with XIB):**

```
<FeatureName>/ (PBXGroup)
├── Cell/ (PBXGroup)
│    ├── <Name>Cell.swift
│    └── <Name>Cell.xib      ← always paired with .swift in same group
├── <Name>.storyboard
├── <Name>ViewController.swift
└── <Name>ViewModel.swift
```

> If multiple Cell files exist (e.g. 2 cell variants), add one `PBXBuildFile` + one `PBXFileReference` per additional Cell file (and `.xib` if applicable). All go inside the `Cell/` PBXGroup.

---

**UUID generation (generate one per file):**
```bash
uuidgen | tr -d '-' | cut -c1-24
```

**Finding the correct group:** Search `project.pbxproj` for a **sibling file** already in the same `MODULE_PATH` folder to locate the parent group UUID and insert the new feature group as a child of it.
