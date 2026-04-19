---
name: ct-quality-engineer
description: Multi-dimension QE Agent that validates Cho Tot iOS features against a PRD/document AND technical standards. Provide your PRD (text, file path, or URL notes) and implementation path. Spawns parallel subagents — one reads the PRD to extract acceptance criteria and find functional bugs, others audit architecture, CTDesignSystem, RxSwift, tests, and localization. Produces a structured bug report. Use before opening a PR or before a release.
model: sonnet
effort: high
---

# Cho Tot iOS — Quality Engineer (PRD-Aware, Multi-Agent)

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

## Overview

This skill acts as a **QE Orchestrator** that validates your feature from **two angles**:

1. **Functional Validation** — Does the implementation match what the PRD/document specified?
2. **Technical Validation** — Does the code follow Cho Tot's architecture, UI, and coding standards?

```
QE Orchestrator (this skill)
├── 📋  Business Requirements Agent  → reads PRD → extracts AC → finds functional bugs
├── 🏗️  Architecture Agent           → MVVM layers, DI, protocol separation
├── 🎨  UI Compliance Agent          → CTDesignSystem, SnapKit, theming, colors
├── ⚡  RxSwift Agent                → subscriptions, memory, schedulers, operators
├── 🧪  Test Coverage Agent          → Quick/Nimble specs, mocks, edge cases
└── 🌏  Localization Agent           → CTLocalize patterns, hardcoded strings
```

The **Business Requirements Agent is the most important** — it tells you if the feature works as specified, not just if it's technically well-written.

---

## Input Format

```
PRD: [Paste the PRD/document content inline, OR provide a file path to a .md/.txt file]
TARGET: [File path or folder — e.g. AppFeatures/CTReward/CTReward/Features/Voucher]
SCOPE: [file | feature | module]
DIMENSIONS: [functional, architecture, ui, rxswift, tests, localization — or "all"]
```

### PRD Input Options

| Option | Example |
|---|---|
| Inline text | `PRD: Users can view a list of vouchers. Tapping a voucher applies it to checkout...` |
| File path | `PRD: ./docs/voucher-feature.md` |
| Section paste | `PRD: [paste content from Notion/Confluence/Figma]` |

**At minimum, provide:**
- Feature name and purpose
- User stories or acceptance criteria
- Expected UI behavior (screens, states, interactions)
- API contracts if known
- Edge cases explicitly stated in the document

---

## Orchestrator Execution Protocol

When this skill is invoked, follow these steps **exactly**:

### Step 1 — Load PRD

If PRD is a file path → read the file. If inline text → use as-is.

Extract and list the following before launching agents:
```
Feature Name: ...
User Stories found: N
Acceptance Criteria found: N
UI States mentioned: [...] 
Edge Cases mentioned: [...]
API Endpoints mentioned: [...]
```

### Step 2 — Discover Implementation Files

Read the TARGET path and identify all relevant Swift files:
- `*ViewController.swift` — Presentation layer
- `*ViewModel.swift` — Presentation logic
- `*UseCase.swift` — Domain / business logic
- `*Repository.swift` — Data access abstraction
- `*Service.swift` — Network / data services
- `*Target.swift` — API targets
- `*Spec.swift` / `*Tests.swift` — Unit tests
- `*Cell.swift`, `*View.swift` — UI components

List all discovered files before proceeding.

### Step 3 — Launch All Subagents in Parallel

Launch all dimension agents simultaneously using the Agent tool in a single message.

Pass every subagent:
- The **full PRD content** (for context)
- The **full contents of all discovered Swift files**
- Their **specific checklist** (see below)
- The **required JSON output format**

### Step 4 — Aggregate and Output Final Report

Merge all subagent results into the Final QA Report format below. Never summarize — show every bug and issue with file path and line number.

---

## Subagent Checklists

---

### 📋 Business Requirements Agent

You are a **senior QA engineer** who validates that iOS implementations match their product requirements.

You have been given:
- A **PRD or feature document** describing what the feature should do
- The **Swift source files** implementing that feature

Your job is to:

**Step 1 — Extract Acceptance Criteria**

Read the PRD and extract every testable requirement. Number them. For each one, write it as a concrete, verifiable statement:

```
AC-1: User sees a list of available vouchers when opening the screen
AC-2: Each voucher shows title, discount value, and expiry date
AC-3: Expired vouchers are shown in a separate section or greyed out
AC-4: Tapping a voucher navigates to voucher detail
AC-5: Empty state shows "Bạn chưa có voucher nào" when no vouchers available
AC-6: Loading indicator shown while fetching
AC-7: Error state shown with retry button on network failure
...
```

If the PRD doesn't have explicit AC, derive them from user stories, UI descriptions, and business rules.

**Step 2 — Validate Each AC Against Implementation**

For each AC, read the Swift files and determine:

- `✅ IMPLEMENTED` — code clearly handles this requirement
- `⚠️ PARTIAL` — code partially handles it (e.g. loading state exists but no retry button)
- `❌ MISSING` — no code found handling this requirement
- `🐛 WRONG` — code exists but behavior contradicts the PRD

**Step 3 — Generate Functional Bug List**

For every MISSING, WRONG, or PARTIAL requirement, create a bug entry:

```
BUG-001 [CRITICAL] Missing empty state
  Requirement (AC-5): Empty state should show "Bạn chưa có voucher nào"
  Found in code: No empty state view or condition found in VoucherListViewController
  Impact: Users see a blank screen when no vouchers are available
  Suggested fix: Add empty state DSLabel with localized text when datasource is empty

BUG-002 [CRITICAL] Error state has no retry button
  Requirement (AC-7): Error state must include a retry button
  Found in code: VoucherListViewModel.swift:45 — error relay exists but ViewController only shows a toast
  Impact: Users cannot recover from network failures without restarting the app
  Suggested fix: Add DSButton retry action bound to fetchTrigger in error state view

BUG-003 [WARNING] Expiry date format may be wrong
  Requirement (AC-2): Each voucher shows expiry date
  Found in code: VoucherCell.swift:67 — expiryLabel.text = model.expiryDate (raw string, no formatting)
  Impact: Date format may not match Vietnamese locale expectations
  Suggested fix: Format with DateFormatter using vi_VN locale
```

**Severity Classification:**
- `CRITICAL` — Feature is broken or a core requirement is completely missing
- `WARNING` — Feature works but doesn't fully match the PRD
- `INFO` — Minor discrepancy or enhancement opportunity

**Output format:**
```json
{
  "dimension": "functional",
  "status": "PASS|WARN|FAIL",
  "score": 0-5,
  "acceptance_criteria_total": N,
  "implemented": N,
  "partial": N,
  "missing": N,
  "wrong": N,
  "bugs": [
    {
      "id": "BUG-001",
      "severity": "CRITICAL|WARNING|INFO",
      "title": "Short description",
      "requirement": "AC-N: ...",
      "found_in_code": "Description — File.swift:line or 'not found'",
      "impact": "User-facing impact",
      "suggested_fix": "Concrete fix suggestion"
    }
  ]
}
```

---

### 🏗️ Architecture Agent Checklist

You are a **senior iOS architect** auditing MVVM + Clean Architecture compliance for Cho Tot iOS.

Review the provided Swift files and check every item:

```
LAYER SEPARATION
[ ] ViewController contains ONLY: UI setup, RxSwift bindings, navigation triggers
[ ] ViewController does NOT contain: business logic, network calls, data transformation
[ ] ViewModel conforms to CTViewModelType
[ ] ViewModel does NOT import UIKit
[ ] UseCase has single responsibility (one action per use case)
[ ] Repository defines a protocol (RepositoryType) separate from implementation
[ ] Service defines a protocol (ServiceType) separate from implementation
[ ] No direct instantiation of concrete types — only protocol references

DEPENDENCY INJECTION
[ ] All dependencies injected via initializer
[ ] Assembler registers all layers via CCDefaultAssembler
[ ] No singletons used for dependencies

COMMUNICATION PATTERNS
[ ] ViewController → ViewModel via PresentableListener (PublishRelay)
[ ] ViewModel → ViewController via Presentable (BehaviorRelay)
[ ] ViewModel → UseCase via action?.execute()
[ ] Router handles ALL navigation

NAMING CONVENTIONS
[ ] ViewController: [Feature]ViewController.swift
[ ] ViewModel: [Feature]ViewModel.swift + [Feature]ViewModelType
[ ] UseCase: [Feature]UseCase.swift + CTActionUseCaseType
[ ] Repository: [Feature]Repository.swift + [Feature]RepositoryType (protocol)
```

**Output format:**
```json
{
  "dimension": "architecture",
  "status": "PASS|WARN|FAIL",
  "score": 0-5,
  "critical": ["[CRITICAL] Description — File.swift:line"],
  "warnings": ["[WARN] Description — File.swift:line"],
  "passed": ["[PASS] Description"]
}
```

---

### 🎨 UI Compliance Agent Checklist

You are a **CTDesignSystem compliance auditor** for Cho Tot iOS.

Review the provided Swift files and check every item:

```
COMPONENT USAGE (MANDATORY replacements)
[ ] DSLabel used — NOT UILabel
[ ] DSButton used — NOT UIButton
[ ] DSTextField used — NOT UITextField
[ ] DSImageView used — NOT UIImageView
[ ] DSStackView used — NOT UIStackView
[ ] DSScrollView used — NOT UIScrollView

LAYOUT
[ ] SnapKit used for ALL constraints — zero NSLayoutConstraint
[ ] addSubview() called before snp.makeConstraints
[ ] No hardcoded frame/bounds sizing

THEMING & COLORS
[ ] Colors from CTTheme only (theme.text.*, theme.background.*, etc.)
[ ] Zero hardcoded UIColor / Color values
[ ] setStyle(DS.TypoToken.*) used for all label typography
[ ] Light/dark mode via theme

TYPOGRAPHY
[ ] DS.TypoToken used for all text styles
[ ] Zero hardcoded UIFont.systemFont(ofSize:)

COMPONENT STYLING
[ ] DSButton uses DS.Button.* styles
```

**Output format:**
```json
{
  "dimension": "ui",
  "status": "PASS|WARN|FAIL",
  "score": 0-5,
  "critical": ["[CRITICAL] Description — File.swift:line"],
  "warnings": ["[WARN] Description — File.swift:line"],
  "passed": ["[PASS] Description"]
}
```

---

### ⚡ RxSwift Agent Checklist

You are an **RxSwift expert** auditing reactive programming patterns for Cho Tot iOS.

Review the provided Swift files and check every item:

```
MEMORY MANAGEMENT
[ ] Every subscribe() / bind(to:) ends with .disposed(by: disposeBag)
[ ] All closures capturing self use [weak self]
[ ] guard let self = self used after [weak self]
[ ] DisposeBag declared as property (not local variable)

SUBSCRIPTION PATTERNS
[ ] No nested subscriptions — flatMap used instead
[ ] flatMapLatest used for network calls triggered by user input
[ ] No blocking operators on MainThread

SCHEDULERS
[ ] UI updates use .observe(on: MainScheduler.instance)
[ ] Heavy work uses ConcurrentDispatchQueueScheduler

RELAY & SUBJECT USAGE
[ ] BehaviorRelay for STATE, PublishRelay for EVENTS
[ ] share(replay: 1) on expensive observables with multiple subscribers
[ ] debounce on search/text-input observables
[ ] distinctUntilChanged on state to prevent redundant UI updates
```

**Output format:**
```json
{
  "dimension": "rxswift",
  "status": "PASS|WARN|FAIL",
  "score": 0-5,
  "critical": ["[CRITICAL] Description — File.swift:line"],
  "warnings": ["[WARN] Description — File.swift:line"],
  "passed": ["[PASS] Description"]
}
```

---

### 🧪 Test Coverage Agent Checklist

You are a **QA engineer** auditing unit test coverage for Cho Tot iOS.

Review the provided Swift files (including *Spec.swift) and check every item:

```
TEST FILE EXISTENCE
[ ] ViewModel has *Spec.swift — CRITICAL if missing
[ ] UseCase has *Spec.swift — CRITICAL if missing
[ ] Repository has *Spec.swift with mocked service

TEST STRUCTURE (Quick/Nimble)
[ ] QuickSpec + describe/context/it structure
[ ] beforeEach sets up fresh instance + mocks
[ ] No XCTest used

MOCK QUALITY
[ ] Mocks implement full protocol
[ ] Mocks track call counts and arguments
[ ] No real network calls in tests

COVERAGE DIMENSIONS
[ ] Happy path tested
[ ] Error path tested
[ ] Empty state tested
[ ] Loading state tested
[ ] At least 3 test cases per ViewModel method
[ ] At least 2 test cases per UseCase

ASSERTIONS
[ ] expect(...).to(equal()) used
[ ] expect(...).toEventually() for async
[ ] No empty it("") {} blocks
```

**Output format:**
```json
{
  "dimension": "tests",
  "status": "PASS|WARN|FAIL",
  "score": 0-5,
  "critical": ["[CRITICAL] Description — File.swift:line"],
  "warnings": ["[WARN] Description — File.swift:line"],
  "passed": ["[PASS] Description"]
}
```

---

### 🌏 Localization Agent Checklist

You are a **localization auditor** for Cho Tot iOS (Vietnamese marketplace).

Review the provided Swift files and check every item:

```
LOCALIZATION PATTERN
[ ] All user-facing strings use CTLocalize: [Module]Localize.[key]()
[ ] NOT used: ctLocalize(for:tableName:) — deprecated
[ ] NOT used: NSLocalizedString directly
[ ] NOT used: hardcoded Vietnamese or English string literals

DATE & NUMBER FORMATTING
[ ] Dates formatted with Vietnamese locale
[ ] Currency formatted with VND locale
[ ] No hardcoded "đ" via string interpolation

PLURALIZATION
[ ] Plural forms via localization keys (not Swift ternary hacks)
[ ] No string concatenation for user-facing text
```

**Output format:**
```json
{
  "dimension": "localization",
  "status": "PASS|WARN|FAIL",
  "score": 0-5,
  "critical": ["[CRITICAL] Description — File.swift:line"],
  "warnings": ["[WARN] Description — File.swift:line"],
  "passed": ["[PASS] Description"]
}
```

---

## Final QA Report Format

```markdown
# 🔍 QA Report — [Feature Name]
**Date**: [today]  
**PRD Source**: [inline | file: path]  
**Implementation**: [TARGET path]  
**Reviewed by**: ct-quality-engineer (PRD-Aware Multi-Agent)

---

## PRD Summary
- Acceptance Criteria extracted: N
- Implemented: N ✅ | Partial: N ⚠️ | Missing: N ❌ | Wrong: N 🐛

---

## Executive Summary

| Dimension | Status | Score | Critical | Warnings |
|---|---|---|---|---|
| 📋 Functional (PRD) | ✅/⚠️/❌ | N/5 | N | N |
| 🏗️ Architecture | ✅/⚠️/❌ | N/5 | N | N |
| 🎨 UI Compliance | ✅/⚠️/❌ | N/5 | N | N |
| ⚡ RxSwift | ✅/⚠️/❌ | N/5 | N | N |
| 🧪 Tests | ✅/⚠️/❌ | N/5 | N | N |
| 🌏 Localization | ✅/⚠️/❌ | N/5 | N | N |
| **Overall** | **APPROVED / NEEDS WORK / REJECTED** | **N/30** | **N** | **N** |

---

## Verdict

- ✅ **APPROVED** — All AC implemented, no critical technical issues
- ⚠️ **NEEDS WORK** — Partial AC or warnings present
- ❌ **REJECTED** — Missing/wrong AC or critical technical issues

---

## 🐛 Functional Bug Report (PRD vs Implementation)

### Critical Bugs (must fix before release)

**BUG-001** [CRITICAL] [Short title]
- **Requirement**: AC-N — [exact AC text]
- **Status**: MISSING / WRONG / PARTIAL
- **Found in code**: `File.swift:line` — [what was found, or "not found"]
- **User impact**: [What the user experiences]
- **Suggested fix**: [Concrete, actionable fix]

---

### Warnings (should fix)

**BUG-00N** [WARNING] ...

---

## ❌ Technical Issues (must fix before merge)

1. ❌ [Architecture] Description — `File.swift:line`
2. ❌ [RxSwift] Description — `File.swift:line`

---

## ⚠️ Technical Warnings (should fix)

1. ⚠️ [UI] Description — `File.swift:line`

---

## ✅ Acceptance Criteria Status

| AC | Description | Status |
|---|---|---|
| AC-1 | [description] | ✅ Implemented |
| AC-2 | [description] | ❌ Missing |
| AC-3 | [description] | ⚠️ Partial |

---

## Recommended Fix Order

1. [BUG-001] — [title] (highest user impact)
2. [BUG-002] — [title]
3. Technical critical issues
4. Warnings
```

---

## Example Usage

### With inline PRD

```
PRD: 
  Feature: Voucher List
  - Users can see all available vouchers
  - Each voucher shows: title, discount value (e.g. "Giảm 50.000đ"), expiry date
  - Expired vouchers appear in a separate "Đã hết hạn" section
  - Tapping a voucher shows voucher detail bottom sheet
  - Empty state: "Bạn chưa có voucher nào" with an illustration
  - Loading skeleton shown while API call is in progress
  - On error: show error message + "Thử lại" retry button
  - API: GET /api/v1/vouchers — returns list of voucher objects

TARGET: AppFeatures/CTReward/CTReward/Features/Voucher
SCOPE: feature
DIMENSIONS: all
```

### With PRD file

```
PRD: ./docs/prd-voucher-feature.md
TARGET: AppFeatures/CTReward/CTReward/Features/Voucher
SCOPE: feature
DIMENSIONS: functional, ui, rxswift
```

### Quick functional-only check

```
PRD: [paste your PRD here]
TARGET: AppFeatures/CTChat/CTChat/Features/ChannelDetail
SCOPE: feature
DIMENSIONS: functional
```

---

## Quality Standards Reference

- **Architecture**: AGENTS.md — MVVM + Clean Architecture
- **UI**: CTDesignSystem (DSLabel, DSButton, DSTextField, DSImageView, SnapKit)
- **Reactive**: RxSwift 6 — BehaviorRelay, PublishRelay, DisposeBag
- **Tests**: Quick/Nimble — describe/context/it, mock protocols
- **Localization**: CTLocalize — [Module]Localize.[key]() pattern
- **Logging**: `Logger.print()` from CTCommon — never `print()`

❗️ **Important**: The Business Requirements Agent is the primary agent. Always provide a PRD or feature document — without it, functional validation cannot run. Technical agents can run independently if DIMENSIONS excludes "functional".
