# Examples — ct-feature-pipeline

> Full test case with input/output: see [examples/pos-warning-message.md](examples/pos-warning-message.md)

---

## Example 1 — Input with Jira Ticket

```
FEATURE_NAME: POS Warning Message
JIRA_TICKET_URL: https://701search.atlassian.net/browse/CRE-13492
REFERENCE_USECASE: CRStatusVideoAIUseCase
FIGMA_URL: https://www.figma.com/design/GlkeqMpiIEcPpIAoHO6FKL/Revenue-Handoff-2026?node-id=2703-10882&m=dev
MODULE_PATH: ChoTot/Features/Job/VerticalizePos/Presentation/Ver2/Pos
COMPONENT_TYPE: bottom_sheet
```

**Phase 0 output:**
```
✅ Phase 0 complete — PRD fetched from Jira.
🔗 Ticket: https://701search.atlassian.net/browse/CRE-13492 — POS improvement for JOB: Remove wording, update thumbnail, dynamic testimonial
📋 PRD source: Jira
→ Proceeding to Phase 1...
```

---

## Example 2 — Input with Manual PRD (no Jira)

```
FEATURE_NAME: POS Warning Message
PRD_CONTENT:
"""
# POS Warning Message — Q3 Revenue Initiative

## Business Context
Revenue team (Nguyen Van A, Tran Thi B) reports that 45% of POS sellers abandon
the checkout flow due to unclear warning prompts. Target: increase conversion from
$1.2M to $2M GMV by Q3 by reducing warning-related drop-off by 30%.
Competitor analysis: Shopee POS achieves 78% completion rate with bottom-sheet warnings.

## Technical Requirements
- Display a bottom-sheet warning when seller tries to submit an incomplete POS order
- API endpoint: POST /api-uni-rev/private/pos/warning_check (returns WarningCheckModel)
- Response includes: title (String), description (String), primaryAction (String), secondaryAction (String)
- User can dismiss (close icon), confirm (primary button), or cancel (secondary button)
- Warning type: orange fill illustration + warning icon

## UI Requirements
- Bottom sheet with drawer header (title + close button)
- Illustration: circular orange background + warning fill icon (40x40)
- Body: title (SemiBold 20px) + description (Regular 14px), centered
- Footer: two buttons side-by-side — secondary (white/bordered) + primary (yellow #FFD400)
"""
REFERENCE_USECASE: CRStatusVideoAIUseCase
FIGMA_URL: https://www.figma.com/design/GlkeqMpiIEcPpIAoHO6FKL/Revenue-Handoff-2026?node-id=2703-10882&m=dev
MODULE_PATH: ChoTot/Features/Job/VerticalizePos/Presentation/Ver2/Pos
COMPONENT_TYPE: bottom_sheet
```

---

## Phase 1 Output

```
✅ FILTERED PRD — POS Warning Message

## Summary
Display a bottom-sheet warning when a seller submits an incomplete POS order,
with options to confirm or cancel.

## User Stories
- As a seller, I want to see a warning when my order is incomplete
- As a seller, I want to choose to continue or cancel from the warning

## Functional Requirements
- Trigger: seller submits incomplete POS order
- API: POST /api-uni-rev/private/pos/warning_check
- Response: title, description, primaryAction, secondaryAction (all String)
- Dismiss: close icon, secondary button (cancel), primary button (confirm)

## UI/UX Specifications
- Component: bottom_sheet
- Illustration: circular orange background + warning-fill icon (40x40)
- Body: title (SemiBold 20px, centered) + description (Regular 14px, centered)
- Footer: secondary button (white/bordered) + primary button (#FFD400), equal width

## API Specifications
- Endpoint: POST /api-uni-rev/private/pos/warning_check
- Response model: WarningCheckModel { title, description, primaryAction, secondaryAction }

---
🔒 Removed: Revenue targets ($1.2M→$2M), employee names, competitor benchmark (Shopee 78%)
✅ Preserved: API endpoint, response schema, UI specs, button behaviors, warning type
```

---

## Phase 2 Output

| Layer | File | Change |
|-------|------|--------|
| NetworkHelper | `CRNetworkHelper.swift` | `Api.warningCheck = "api-uni-rev/private/pos/warning_check"` |
| Target | `CRPosPfTargets.swift` | `struct WarningCheckTarget: Requestable` |
| Service | `CRPosPfServices.swift` | `func warningCheck(input: String) -> Observable<WarningCheckModel?>` |
| Repository | `CRPosRepositories.swift` | Pass-through method |
| UseCase | `CRPosPfUseCase.swift` | `final class CRWarningCheckUseCase: CTActionUseCaseType` |
| ViewModel | `POSViewModel.swift` | `func executeWarningCheck(input: String)` |
| Model | `PosModel.swift` | `public struct WarningCheckModel: Codable` |

```
✅ Phase 2 complete — UseCase generated.
📁 Files modified: 7
→ Proceeding to Phase 3...
```

---

## Phase 3 Output

```
✅ Phase 3 complete — UI generated.
• JBWarningMessageViewController.swift → ChoTot/Features/Job/VerticalizePos/Presentation/Ver2/Pos/ViewControllers/
• JBWarningMessage.storyboard → ChoTot/Features/Job/VerticalizePos/Presentation/Ver2/Pos/
• project.pbxproj — 5 entries added
```

---

## Final Summary

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ PIPELINE COMPLETE — POS Warning Message
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Phase 1 — Semantic Filter
  🔒 Removed: Revenue targets, employee names, competitor data
  ✅ Preserved: API endpoint, response schema, UI specs, button behaviors

Phase 2 — UseCase: CRWarningCheckUseCase
  Modified 7 files:
  • CRNetworkHelper.swift
  • CRPosPfTargets.swift
  • CRPosPfServices.swift
  • CRPosRepositories.swift
  • CRPosPfUseCase.swift
  • POSViewModel.swift
  • PosModel.swift

Phase 3 — UI: JBWarningMessageViewController
  • JBWarningMessageViewController.swift
  • JBWarningMessage.storyboard
  • project.pbxproj — 5 entries added

Next steps:
  1. Run: swiftlint lint --config .swiftlint.yml --strict [changed files]
  2. Wire ViewModel callbacks in ViewController (onConfirm, onCancel)
  3. Register CRWarningCheckUseCase in CRUsecaseAssembler
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```
