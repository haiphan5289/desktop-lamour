# Example: POS Warning Message

Structured test case — input + expected output for each phase.

---

## Input

```
FEATURE_NAME: POS Warning Message
PRD_CONTENT:
"""
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

## Expected: Phase 1 Output

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
🔒 Removed: (none in this input — no sensitive data present)
✅ Preserved: API endpoint, response schema, UI specs, button behaviors, warning type
```

---

## Expected: Phase 2 Files Modified

| Layer | File | Change |
|-------|------|--------|
| NetworkHelper | `CRNetworkHelper.swift` | `Api.warningCheck = "api-uni-rev/private/pos/warning_check"` |
| Target | `CRPosPfTargets.swift` | `struct WarningCheckTarget: Requestable` |
| Service | `CRPosPfServices.swift` | `func warningCheck(input: String) -> Observable<WarningCheckModel?>` |
| Repository | `CRPosRepositories.swift` | Pass-through method |
| UseCase | `CRPosPfUseCase.swift` | `final class CRWarningCheckUseCase: CTActionUseCaseType` |
| ViewModel | `POSViewModel.swift` | `func executeWarningCheck(input: String)` |
| Model | `PosModel.swift` | `public struct WarningCheckModel: Codable` |

---

## Expected: Phase 3 Artifacts

```
• JBWarningMessageViewController.swift
  → ChoTot/Features/Job/VerticalizePos/Presentation/Ver2/Pos/ViewControllers/

• JBWarningMessage.storyboard
  → ChoTot/Features/Job/VerticalizePos/Presentation/Ver2/Pos/

• project.pbxproj — 5 entries added
```

---

## Eval Checklist

- [ ] Phase 1: No sensitive data leaked into FILTERED_PRD
- [ ] Phase 1: API endpoint `POST /api-uni-rev/private/pos/warning_check` preserved
- [ ] Phase 2: All 7 files modified (6 layers + model)
- [ ] Phase 2: `Api.warningCheck` key is lowercase
- [ ] Phase 2: `WarningCheckModel` fields match PRD: `title`, `description`, `primaryAction`, `secondaryAction`
- [ ] Phase 3: Component type `bottom_sheet` applied to storyboard template
- [ ] Phase 3: ViewController uses `DSLabel` and `DSButton` from CTDesignSystem
- [ ] Phase 3: No `\u{XXXX}` escape sequences in generated Swift
- [ ] Overall: `TodoWrite` called at each phase transition
