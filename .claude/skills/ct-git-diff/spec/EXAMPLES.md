# Examples — ct-git-diff

---

## Example 1 — Summary mode, auto-detected base

### Input

```
/ct-git-diff
```

### What Claude confirms before running

```
Comparing: revenue/skill-claude-code → main
(your branch has 3 commits not yet in main)
```

> **Reading the arrow:** `your-branch → base` means "what did I add on top of base?"
> Base branch was auto-detected (main → dev → master, whichever exists remotely).

### Output

```
📊 Diff Summary
─────────────────────────────────────────
Branch:  revenue/skill-claude-code ← main
Commits: 3 commits ahead
Files:   12 changed (+847 insertions, -23 deletions)

Presentation  │ 5 files  (3M 2A)
Domain        │ 2 files  (1M 1A)
Data          │ 3 files  (2M 1A)
Tests         │ 0 files  ⚠️ no test changes
Config/Other  │ 2 files

Presentation
  M  AppFeatures/CTPos/Presentation/ViewModels/PosOrderViewModel.swift
  M  AppFeatures/CTPos/Presentation/ViewControllers/PosOrderViewController.swift
  A  AppFeatures/CTPos/Presentation/Views/PosOrderSummaryView.swift
  ...

[Layer 2 — Structured Analysis]

Presentation Layer
  ❌ PosOrderViewController.swift:42 — UILabel used instead of DSLabel
  ✅ disposed(by: disposeBag) present in all subscription chains
  ✅ SnapKit used for all constraints

Domain Layer
  ✅ PosUpdateOrderUseCase delegates to repository only
  ✅ PosOrderModel is an immutable struct

Data Layer
  ✅ PosUpdateOrderTarget conforms to Requestable
  ❌ PosOrderService.swift — missing observe(on: MainScheduler.instance) before UI-bound observable

Tests
  ⚠️ 5 changed non-test files with no corresponding test changes

[Layer 3 — Review Checklist]

Architecture
  ✅ MVVM layers respected
  ✅ UseCases used for business logic
  ✅ Repositories abstract data access

CT Design System
  ❌ UILabel in PosOrderViewController.swift:42 — use DSLabel
  ✅ DS.TypoToken.* used throughout
  ✅ SnapKit constraints only

RxSwift
  ✅ DisposeBag declared and used
  ✅ [weak self] in all closures
  ❌ Missing observe(on: MainScheduler.instance) in PosOrderService.swift
  ✅ No nested subscriptions

Code Quality
  ✅ No force unwrap
  ✅ Logger.print() used
  ✅ CTLocalize pattern used
  ✅ DRY principle followed

[Layer 4 — Narrative]

This branch adds the POS order update flow, wiring a new PosUpdateOrderUseCase
through the 6-layer pattern. The ViewModel correctly delegates to the UseCase and
the data flow follows the Clean Architecture contract. Two issues need attention:
PosOrderViewController.swift uses raw UILabel at line 42 (CT Design System
violation), and PosOrderService.swift is missing the MainScheduler observation
before a UI-bound observable (potential threading issue). No unit tests were
added for the new UseCase or ViewModel, which is a gap given the business-critical
nature of order updates.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ ct-git-diff COMPLETE — revenue/skill-claude-code
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Compared: revenue/skill-claude-code ← main
Files:    12 changed | +847 -23

💡 Suggested Next Steps:
  1. /review-code AppFeatures/CTPos/Presentation/ViewControllers/PosOrderViewController.swift
  2. /ct-unittest PosUpdateOrderViewModel
  3. /ct-bugfix-skill — fix threading issue in PosOrderService.swift
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

---

## Example 2 — Narrowed scope with flags

### Input

```
/ct-git-diff main --path AppFeatures/CTChat --focus ViewModels --limit 20
```

### Resolution

```
Target: main (explicit)
Path filter: AppFeatures/CTChat
Focus filter: ViewModels
Limit: 20 files
```

### Behaviour

- Runs: `git diff --name-only main...HEAD -- AppFeatures/CTChat | grep -i "ViewModels" | head -20`
- Only files under `AppFeatures/CTChat` matching `ViewModels` in their path are analyzed
- All four output layers scoped to those files only
- If no matching files found: `ℹ️ No files matching ViewModels under AppFeatures/CTChat in the diff.`
