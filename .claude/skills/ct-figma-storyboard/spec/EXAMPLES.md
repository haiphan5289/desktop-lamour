# Examples — ct-figma-storyboard

---

## Example 1 — bottom_sheet (JBWarningMessage)

### Input

```
FIGMA_URL: https://www.figma.com/design/GlkeqMpiIEcPpIAoHO6FKL/Revenue-Handoff-2026?node-id=2703-10882&m=dev
MODULE_PATH: ChoTot/Features/Job/VerticalizePos/Presentation/Ver2/Pos
COMPONENT_TYPE: bottom_sheet
```

### Expected Output

1. `JBWarningMessageViewController.swift` — ViewController with `DSBottomSheetLayout`, `@IBOutlet` properties, `configureUI()`, `@IBAction` handlers
2. `JBWarningMessage.storyboard` — UIStackView-based layout: drawer header (DSLabel + close UIButton) + body container (UIImageView + DSLabel + DSLabel) + footer (DSButton x2 in horizontal StackView)
3. 5 pbxproj entries in `ChoTot.xcodeproj/project.pbxproj`

### ViewController structure

```swift
final class JBWarningMessageViewController: UIViewController, DSBottomSheetLayout {
    @IBOutlet private weak var titleLabel: DSLabel!
    @IBOutlet private weak var bodyTitleLabel: DSLabel!
    @IBOutlet private weak var descriptionLabel: DSLabel!
    @IBOutlet private weak var secondaryButton: DSButton!
    @IBOutlet private weak var primaryButton: DSButton!

    var onCancel: (() -> Void)?
    var onConfirm: (() -> Void)?
    private let theme = CMStaticThemeLoader.jobTheme

    override func viewDidLoad() {
        super.viewDidLoad()
        configureUI()
    }

    deinit { Logger.print("\(self) deallocated.") }

    private func configureUI() {
        titleLabel.setStyle(DS.TypoToken.Header.Section(color: theme.text.textPrimary.color))
        titleLabel.text = "Cảnh báo"
        bodyTitleLabel.setStyle(DS.TypoToken.Header.Page(color: theme.text.textPrimary.color))
        descriptionLabel.setStyle(DS.TypoToken.Body.Section(color: theme.text.textSecondary.color))
        secondaryButton.setStyle(DS.Button.secondary(size: .medium, themeType: theme.type))
        primaryButton.setStyle(DS.Button.primary(size: .medium, themeType: theme.type))
    }

    @IBAction private func didTapSecondaryButton(_ sender: Any) {
        dismiss(animated: true) { [weak self] in self?.onCancel?() }
    }

    @IBAction private func didTapPrimaryButton(_ sender: Any) {
        dismiss(animated: true) { [weak self] in self?.onConfirm?() }
    }

    @IBAction private func didTapClose(_ sender: Any) {
        dismiss(animated: true)
    }
}
```

### pbxproj entries (5 total)

```
/* PBXBuildFile */
A1B2C3D4E5F6A7B8C9D0E1F2 /* JBWarningMessageViewController.swift in Sources */
A2B3C4D5E6F7A8B9C0D1E2F3 /* JBWarningMessage.storyboard in Resources */

/* PBXFileReference */
A3B4C5D6E7F8A9B0C1D2E3F4 /* JBWarningMessageViewController.swift */
A4B5C6D7E8F9A0B1C2D3E4F5 /* JBWarningMessage.storyboard */

/* PBXGroup — Pos folder */
(both file refs added as children)
```

---

## Example 2 — tableview_multisection (PTSubscriptionSK pattern)

### Input

```
FIGMA_URL: <Figma node URL for subscription screen>
MODULE_PATH: AppFeatures/CTPTY/CTPTY/Features/Subscription/SubscriptionSK
COMPONENT_TYPE: tableview_multisection
```

### Expected Output

1. `<Name>ViewController.swift` — RxDataSources-driven TableView, `BehaviorRelay<[Section]>`, RxTableViewSectionedReloadDataSource, cell registration for each cell type
2. `<Name>ViewModel.swift` — business logic, data relay
3. `<Name>.storyboard` — TableView embedded in UIStackView layout
4. `Cell/<Name>Cell.swift` — programmatic cell (UIStackView + SnapKit, NO XIB)
5. 10+ pbxproj entries

### Code patterns extracted from reference (Step 3a)

| Pattern | Value |
|---|---|
| Data binding | `RxDataSources` — `RxTableViewSectionedReloadDataSource<Section>` |
| Section model | `typealias Section = SectionModel<String, CellType>` |
| Data relay | `BehaviorRelay<[Section]>` → `.drive(tableView.rx.items(dataSource:))` |
| Cell registration | `tableView.register(XCell.self, forCellReuseIdentifier: XCell.reuseId)` |
| Selection | `tableView.rx.itemSelected.subscribe(...)` |
| Imports | `import RxDataSources` |
