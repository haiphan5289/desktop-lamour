---
description: "End-to-end feature pipeline for Cho Tot iOS — single input (PRD + Figma URL + module) auto-runs 3 phases in sequence: semantic filter PRD → generate 6-layer UseCase → generate ViewController + Storyboard + register pbxproj entries. Supports optional Jira ticket URL to auto-fetch PRD via Atlassian MCP. Use when starting a new feature from PRD to production-ready code."
argument-hint: "FEATURE_NAME: <name> JIRA_TICKET_URL: <optional https://701search.atlassian.net/browse/CRE-XXXX> PRD_CONTENT: <optional prd> REFERENCE_USECASE: <class> FIGMA_URL: <url> MODULE_PATH: <path> COMPONENT_TYPE: <type>"
---

## Prompt Activation

**You are an expert iOS developer orchestrating a full feature pipeline for Chợ Tốt iOS.**

Single-entry orchestrator. Input once → output: filtered PRD + 6-layer UseCase + ViewController + Storyboard + pbxproj entries.

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code.

---

## How to Use This Prompt

Provide your input in this format:

```
FEATURE_NAME: <e.g. "POS Warning Message">
JIRA_TICKET_URL: <optional — e.g. https://701search.atlassian.net/browse/CRE-13492 — if provided, PRD is auto-fetched from Jira>
PRD_CONTENT:                       ← optional if JIRA_TICKET_URL is given
"""
[Paste full PRD here — sensitive data will be filtered in Phase 1]
"""
REFERENCE_USECASE: <e.g. CRStatusVideoAIUseCase>
FIGMA_URL: <e.g. https://www.figma.com/design/GlkeqMpiIEcPpIAoHO6FKL/...?node-id=2703-10882>
MODULE_PATH: <e.g. ChoTot/Features/Job/VerticalizePos/Presentation/Ver2/Pos>
COMPONENT_TYPE: <bottom_sheet | full_screen | modal | view_component | tableview_onesection | tableview_multisection>
```

> **Jira MCP:** Uses the `Atlassian MCP` server (configured in `.vscode/mcp.json`, endpoint: `https://mcp.atlassian.com/v1/mcp`). Key tool: `mcp_atlassian_mcp_getJiraIssue(cloudId, issueIdOrKey, responseContentFormat: "markdown")`.

---

## Execution: 3 Phases in Sequence

### MANDATORY: Todo List Tracking (MUST NOT be skipped)

**The very first action** — before any analysis or output — MUST be initializing all tasks with a todo list. This is non-negotiable. The todo list is the user's primary progress tracker and must be visible throughout the entire pipeline.

**Step 0 (REQUIRED — do this FIRST before anything else):**

Use the todo list tool to create:

```
- Phase 0 — Fetch PRD from Jira (if JIRA_TICKET_URL provided)  [pending]
- Phase 1 — Semantic Filter PRD                                [pending]
- Phase 2 — Generate 6-layer UseCase                           [pending]
- Phase 3a — Generate ViewController + Storyboard              [pending]
- Phase 3b — Register files in project.pbxproj                 [pending]
```

> If `JIRA_TICKET_URL` is not provided, Phase 0 is auto-completed immediately and skipped.

**Lifecycle rules — STRICTLY ENFORCED:**

| Moment | Action |
|--------|--------|
| Before starting a phase | Mark that task → `in_progress` |
| After finishing a phase | Mark that task → `completed` |
| Only one task may be `in_progress` at a time | |

Run each phase completely before starting the next. Show a progress header before each phase:

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
PHASE [N]/3 — [Phase Name]
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

---

### Phase 0 — Fetch PRD from Jira (conditional)

**Condition:** Only run this phase if `JIRA_TICKET_URL` is provided.

**Steps:**

1. Load the Atlassian MCP tools: call `tool_search_tool_regex` with pattern `mcp_atlassian` to ensure tools are available
2. If tools fail to load, skip Phase 0 — warn the user and fall back to `PRD_CONTENT`
3. Parse the ticket key from `JIRA_TICKET_URL` (e.g. `https://701search.atlassian.net/browse/CRE-13492` → key: `CRE-13492`, cloudId: `https://701search.atlassian.net`)
4. Call `mcp_atlassian_mcp_getJiraIssue(cloudId: "<host>", issueIdOrKey: "<key>", responseContentFormat: "markdown")` to fetch the ticket's summary, description, acceptance criteria, and linked content
5. Merge fetched content with any provided `PRD_CONTENT` (manual PRD supplements Jira content)
6. Store as `JIRA_PRD` in context — pass to Phase 1 as the primary input

**Output block:**
```
✅ Phase 0 complete — PRD fetched from Jira.
🔗 Ticket: [JIRA_TICKET_URL] — [ticket summary]
📋 PRD source: Jira[+ manual supplement if PRD_CONTENT was also given]
→ Proceeding to Phase 1...
```

**If Jira MCP is unreachable:**
```
⚠️ Jira MCP unavailable — falling back to manual PRD_CONTENT.
Fix: Ensure .vscode/mcp.json has the Atlassian MCP server configured with endpoint https://mcp.atlassian.com/v1/mcp
```

---

### Phase 1 — Semantic Filter (Strip PRD)

**Goal:** Remove sensitive business data from PRD. Output a clean PRD for AI processing.

**Rules to apply:**

1. **Remove** sensitive business data:
   - Revenue/GMV targets, financial projections
   - Employee names, team names, org chart references
   - Competitor benchmarks and internal metrics
   - Dates and release schedules tied to business plans

2. **Preserve** all technical content:
   - API endpoint paths and HTTP methods
   - Request/response schemas and field names
   - User stories and functional requirements
   - UI/UX specifications (layout, spacing, colors, typography)
   - Validation rules and error handling requirements
   - Business logic rules (triggers, conditions, flows)

**Input:** `JIRA_PRD` from Phase 0 (if Jira was fetched), otherwise `PRD_CONTENT` from user input

**Output:** `FILTERED_PRD` — cleaned PRD text (stored in context for Phase 2)

**End of Phase 1:** Print summary:
```
✅ Phase 1 complete — PRD filtered.
🔒 Removed: [list what was stripped]
✅ Preserved: [list what was kept]
→ Proceeding to Phase 2...
```

---

### Phase 2 — UseCase Generation (6-Layer Implementation)

**Goal:** Generate complete 6-layer UseCase implementation based on the filtered PRD.

**Parameters to use:**
- Use `FILTERED_PRD` from Phase 1 as the feature specification
- Extract `USECASE_NAME` from the FEATURE_NAME or PRD summary (e.g. "WarningMessage" → `CRWarningMessageUseCase`)
- `REFERENCE_USECASE` from user input — read this file to replicate its exact patterns
- `MODULE_PATH` from user input
- Auto-detect all other parameters from existing module conventions

**The 6 layers to implement (modify existing files only):**

| Layer | File | Change |
|---|---|---|
| NetworkHelper | `CR*NetworkHelper.swift` | Add `Api.<usecaseName> = "<endpoint_path>"` |
| Target | `CR*Targets.swift` | Add `struct <UseCaseName>Target: Requestable` |
| Service | `CR*Services.swift` | Add `func <usecaseName>(input:) -> Observable<Output?>` |
| Repository | `CR*Repository.swift` | Add pass-through method calling service |
| UseCase | `CR*UseCase.swift` | Add `final class CR<UseCaseName>UseCase: CTActionUseCaseType` |
| ViewModel | `*ViewModel.swift` | Add `func execute<UseCaseName>(input:)` with elements/executing/underlyingError bindings |

**Auto-detection rules:**
- Repository property name in ViewModel (e.g. `checkoutRepo`, `posRepo`, `vehRepo`) → use as `{REPO_PROPERTY_NAME}`
- Wrapper model by module: CorePayment → `CRModelCommon`; VEH → `BaseResponseModel`; POS → no wrapper
- GET → query params; POST → body params

**UseCase execution method pattern in ViewModel:**
```swift
func execute<UseCaseName>(input: <InputType>) {
    <usecaseName>UseCase.action?.elements
        .observe(on: MainScheduler.instance)
        .subscribeNext { [weak self] result in
            self?.presenter?.<relay>.accept(result)
        }.disposed(by: disposeBag)

    <usecaseName>UseCase.action?.executing
        .observe(on: MainScheduler.instance)
        .subscribeNext { [weak self] isLoading in
            self?.presenter?.isLoading.accept(isLoading)
        }.disposed(by: disposeBag)

    <usecaseName>UseCase.action?.underlyingError
        .subscribeNext { [weak self] error in
            self?.presenter?.error.accept(error)
        }.disposed(by: disposeBag)

    <usecaseName>UseCase.action?.execute(input)
}
```

> Never use `.observe(on:)` for `underlyingError`. Never use `.bind(onNext:)`.

**Output:** Modified/created files across 6 layers + confirmation table of all generated code

**End of Phase 2:** Print summary:
```
✅ Phase 2 complete — UseCase generated.
📁 Files modified: [list of 6-11 files]
→ Proceeding to Phase 3...
```

---

### Phase 3 — UI Generation (ViewController + Storyboard)

**Goal:** Generate production-ready ViewController + Storyboard from the Figma design.

**Parameters to use:**
- `FIGMA_URL` from user input
- `MODULE_PATH` from user input
- `COMPONENT_TYPE` from user input
- ViewModel class name from Phase 2 output (for IBAction/callback wiring hints)

**Sub-steps:**

#### 3a — Verify Figma MCP Server
1. Call `tool_search_tool_regex` with pattern `mcp_figma` to load Figma MCP tools
2. Call `mcp_figma_get_screenshot` to verify server is reachable
3. If unreachable, stop and report fix instructions

#### 3b — Fetch Figma Design Context
1. Call `mcp_figma_get_design_context(file_key, node_id, depth=4)` and `mcp_figma_get_screenshot` in parallel
2. Extract overridden text content (see note below)
3. Map Figma tokens to CTDesignSystem tokens

**Extract overridden text content (MANDATORY):**
- Inspect `overrides` array — for every entry with `"characters"` in `overriddenFields`, fetch that node separately
- For button instances with `"componentProperties"`, read `componentProperties["↳ Input Text#..."].value`

#### 3c — Generate ViewController (.swift)

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
        titleLabel.setStyle(DS.TypoToken.Header.Page(color: theme.text.textPrimary.color))
        descriptionLabel.setStyle(DS.TypoToken.Body.Section(color: theme.text.textSecondary.color))
        secondaryButton.setStyle(DS.Button.secondary(size: .medium, themeType: theme.type))
        primaryButton.setStyle(DS.Button.primary(size: .medium, themeType: theme.type))
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

#### 3d — Generate Storyboard (.storyboard)

**UIStackView-based layout (bottom_sheet pattern):**
```
Root View
└── mainStackView (vertical, spacing=0, pinned to safeArea)
    ├── Drawer Header (UIView, height=48)
    │    ├── Title (DSLabel, customModule="CTDesignSystem")
    │    ├── Close Button (UIButton, 24x24)
    │    └── Separator (UIView, height=1)
    ├── Body Container (UIView — NOT a StackView)
    │    └── Body StackView (vertical, alignment=center, spacing=16)
    │         pinned: top=24, leading=16, trailing=16, bottom=24
    └── Footer (UIView)
         └── Button StackView (horizontal, distribution=fillEqually, spacing=8)
              ├── Secondary Button (DSButton, customModule="CTDesignSystem")
              └── Primary Button (DSButton, customModule="CTDesignSystem")
```

**Storyboard XML rules:**
- `toolsVersion="23504"` and `plugIn version="23506"`
- `DSLabel` / `DSButton` with `customClass` and `customModule="CTDesignSystem"`
- `storyboardIdentifier` matches the ViewController class name exactly
- `distribution="fillEqually"` on horizontal button StackViews
- Body padding → wrapper `UIView` + explicit constraints, NEVER `layoutMarginsRelativeArrangement`
- XML comments: ASCII only — no Unicode or box-drawing characters

#### 3e — Register in project.pbxproj

**For bottom_sheet / full_screen / modal (5 entries):**
- `PBXBuildFile` for ViewController.swift in Sources
- `PBXBuildFile` for .storyboard in Resources
- `PBXFileReference` for each file
- `PBXGroup` with both file refs
- Sources + Resources build phase entries

**For tableview_onesection / tableview_multisection (10+ entries):**
- All of the above + ViewModel.swift + Cell.swift (+ Cell.xib if standalone)
- Add `Cell/` subfolder PBXGroup

**UUID generation:**
```bash
uuidgen | tr -d '-' | cut -c1-24
```

**Output:**
- `<Name>ViewController.swift`
- `<Name>.storyboard`
- 5 (or 10+) `project.pbxproj` entries

**End of Phase 3:** Print summary:
```
✅ Phase 3 complete — UI generated.
```

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
| `rgba(251,115,40)` (orange) | `theme.background.backgroundWarningLight.color` |
| Warning icon | `CTAssetSystemIcon.warningFill24px(tint:)` |
| Close icon | `CTAssetSystemIcon.closeOutline24px()` |

---

## Final Summary (after all 3 phases)

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ PIPELINE COMPLETE — [FEATURE_NAME]
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Phase 0 — Jira PRD Fetch
  🔗 Ticket: [JIRA_TICKET_ID] — https://701search.atlassian.net/browse/[JIRA_TICKET_ID]
  📋 Source: [Jira / manual / merged]

Phase 1 — Semantic Filter
  🔒 Removed: [summary]
  ✅ Preserved: [summary]

Phase 2 — UseCase: [UseCaseName]
  Modified [N] files:
  • [file 1]
  • [file 2]
  • ...

Phase 3 — UI: [ViewControllerName]
  • [Name]ViewController.swift → [MODULE_PATH]/ViewControllers/
  • [Name].storyboard → [MODULE_PATH]/
  • project.pbxproj — 5 entries added

Next steps:
  1. Run: swiftlint lint --config .swiftlint.yml --strict [changed files]
  2. Wire ViewModel callbacks in ViewController
  3. Register DI in Assembler if not auto-detected
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

---

## Example

### Input

```
FEATURE_NAME: POS Warning Message
JIRA_TICKET_ID: CT-1234                        ← auto-fetches PRD from Jira
PRD_CONTENT:                                   ← optional supplement
"""
## Technical Requirements
- Display a bottom-sheet warning when seller tries to submit an incomplete POS order
- API endpoint: POST /api-uni-rev/private/pos/warning_check
- Response: title (String), description (String), primaryAction (String), secondaryAction (String)
- User can dismiss (close icon), confirm (primary button), or cancel (secondary button)

## UI Requirements
- Bottom sheet with drawer header (title + close button)
- Illustration: circular orange background + warning fill icon (40x40)
- Body: title (SemiBold 20px) + description (Regular 14px), centered
- Footer: secondary button (white/bordered) + primary button (#FFD400)
"""
REFERENCE_USECASE: CRStatusVideoAIUseCase
FIGMA_URL: https://www.figma.com/design/GlkeqMpiIEcPpIAoHO6FKL/Revenue-Handoff-2026?node-id=2703-10882&m=dev
MODULE_PATH: ChoTot/Features/Job/VerticalizePos/Presentation/Ver2/Pos
COMPONENT_TYPE: bottom_sheet
```

### Expected Output

| Phase | Deliverable |
|---|---|
| Phase 1 | Filtered PRD with sensitive data removed |
| Phase 2 | 6 files modified: NetworkHelper, Target, Service, Repository, UseCase, ViewModel |
| Phase 3 | `JBWarningMessageViewController.swift` + `JBWarningMessage.storyboard` + 5 pbxproj entries |
