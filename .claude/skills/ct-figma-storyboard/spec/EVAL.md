# Eval — ct-figma-storyboard

Use this checklist to verify output quality before finishing.

---

## Completion Checklist

- [ ] ViewController IBOutlets match storyboard outlet connections exactly
- [ ] All `@IBAction` selectors match storyboard action connections
- [ ] `storyboardIdentifier` matches the ViewController class name
- [ ] SnapKit used for any programmatic constraints (no `NSLayoutConstraint`)
- [ ] Storyboard root section is a vertical UIStackView (not anchor-chained views)
- [ ] Header/body/footer are StackView children — not manually top/bottom chained
- [ ] Button rows use `distribution=fillEqually` StackView (not equal-width constraints)
- [ ] Body padding done via a wrapper `UIView` with explicit top/leading/trailing/bottom constraints — **NEVER** `layoutMarginsRelativeArrangement` or `<layoutMargins>` in XML
- [ ] No XML comments with non-ASCII characters inside the storyboard XML
- [ ] pbxproj entries added: 5 for `bottom_sheet/full_screen/modal`; 10+ for `tableview_onesection/tableview_multisection`; +2 per standalone cell XIB (see Step 6b in PROMPT.md)
- [ ] TableView setup is extracted into a dedicated `setupTableView()` function — called from `configureUI()`, not inlined
- [ ] Cell registration: storyboard prototype → `register(ClassName.self)`; standalone XIB → `UINib(nibName:, bundle: Bundle(for: ClassName.self))` with explicit framework bundle
- [ ] Cell Swift uses `awakeFromNib()` + `@IBOutlet` (XIB/prototype) — NOT `lazy var` + SnapKit + `init(style:reuseIdentifier:)` (programmatic only)
- [ ] Cell XIB: `DSRadioButton` uses `<view customClass="DSRadioButton">`, `DSLabel` uses `<label customClass="DSLabel">`, both with `customModule="CTDesignSystem"`
- [ ] Cell XIB layout: horizontal `UIStackView` is the main layout driver inside `containerView` — subviews are StackView children, NOT individually constrained with leading/centerY/trailing anchors
- [ ] File header has correct `git config user.name` and current date from `mcp__time__get_current_time`
- [ ] `deinit { Logger.print("\(self) deallocated.") }` present
- [ ] `[weak self]` used in dismiss closures
- [ ] No `DS.T14B` or `CTColor.*` APIs used (legacy — use `DS.TypoToken.*` and `theme.*` only)
- [ ] No `\u{XXXX}` escape sequences in Vietnamese string literals
- [ ] All DS tokens verified against codebase (no fabricated token names)
