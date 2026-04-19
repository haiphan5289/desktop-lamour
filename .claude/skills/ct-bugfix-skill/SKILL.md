---
name: ct-bugfix-skill
description: Debug and fix iOS bugs in Cho Tot with precision. Use WHENEVER you encounter crashes, memory leaks, state not updating, UI styling mismatches, threading errors, RxSwift disposal issues, or view not rendering. This skill identifies root causes by verifying MVVM-C data flow, checking RxSwift subscriptions and retain cycles, validating CTDesignSystem component usage, and confirming scheduler correctness. Essential for CTInsertAd, CTJOB, CTVEH, CTChat, and CTAuthentication. Use even if you're just suspicious about state management, subscribe logic, or component styling.
model: sonnet
effort: high
---

# iOS Bug Fix Skill

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

## Overview

This skill provides a structured debugging workflow for identifying and fixing iOS bugs in the Cho Tot project. It covers both general iOS debugging patterns (RxSwift, memory, state management, threading) and Cho Tot-specific issues (MVVM-C architecture, CTDesignSystem, module patterns, ECS enums).

## When to Use This Skill

**Use this skill when:**
- Debugging crashes, memory leaks, or unexpected behavior
- Investigating UI glitches or state synchronization issues
- Fixing test failures or race conditions
- Troubleshooting RxSwift subscription issues
- Verifying MVVM-C data flow in features
- Validating CTDesignSystem component usage
- Debugging ECS enum issues or state consistency problems

## Core Debugging Workflow

### Step 1: Limit Scope (Read 3-4 Files Max)

Only read the specific files the user mentions. Never explore broadly.

**Good scope:**
- ViewController that exhibits the issue
- Associated ViewModel
- Related UseCase or Repository

**Avoid exploring:**
- Entire modules
- All dependencies transitively
- "Nearby" files unless directly relevant

### Step 2: Identify Root Cause

State the root cause clearly and concisely. Ask yourself:

**For UI bugs:**
- Is the state binding correct? (BehaviorRelay → Observable → bind(to:))
- Are UI components using CTDesignSystem? (not raw UIKit)
- Is SnapKit layout correct? (no NSLayoutConstraint, XIB, or frame-based)
- Is there a threading issue? (UI updates on MainScheduler?)
- Is the view lifecycle correct? (viewDidLoad → viewWillAppear → viewDidAppear)?

**For state/data bugs:**
- Is the MVVM-C data flow correct? (ViewController → ViewModel → UseCase → Repository)
- Are RxSwift subscriptions properly disposed? (DisposeBag in use?)
- Is there a retain cycle? (weak references where needed?)
- Are relays/subjects properly typed? (BehaviorRelay vs PublishSubject?)
- Is the presenter relaying the right data?

**For RxSwift bugs:**
- Is the observable on the correct scheduler? (background work on `subscribeOn`, UI updates on `observeOn(MainScheduler.instance)`)
- Are operations chained correctly? (flatMap vs map vs compactMap?)
- Is the subscription disposed in the correct DisposeBag?
- Are error cases handled? (catchError, onError, trackError?)

**For memory bugs:**
- Are closures capturing `self` weakly? (`[weak self]` in Observable handlers)
- Are listeners/delegates being deallocated? (weak reference storage?)
- Is there a circular dependency in DI? (Assembler cycle?)
- Are timers/subscriptions cancelled on dealloc?

**For CTDesignSystem bugs:**
- Is the component from CTDesignSystem? (DSLabel, DSButton, not UILabel, UIButton)
- Is the styling applied correctly? (setStyle(DS.TypoToken...) with correct theme?)
- Are colors using theme tokens? (not UIColor directly, not hardcoded hex)
- Is layout using SnapKit? (not Auto Layout, Interface Builder, or frame-based)

**For Cho Tot module bugs:**
- Are protocol dependencies injected? (not force-unwrapped, not singleton)
- Is the module's Assembler/DI setup correct?
- Are ECS enums properly generated? (run `python bin/gen_ecs_enum.py`)
- Is the feature following MVVM-C structure? (separate Presentation/Domain/Data layers)

### Step 3: Apply Minimal Fix

Only fix the root cause. Don't refactor surrounding code.

**Good fixes:**
- Add missing `.disposed(by: disposeBag)`
- Fix scheduler with `.observe(on: MainScheduler.instance)`
- Change `UILabel` to `DSLabel` with correct styling
- Add `[weak self]` to closure
- Wire presenter relay correctly

**Avoid:**
- Rewriting the entire ViewModel
- Restructuring directories
- Changing architectural patterns unnecessarily
- "Cleanup" of surrounding code

### Step 4: Verify the Full Path

Trace the fix end-to-end:

1. **User interaction** → ViewController trigger (via listener)
2. **ViewController** → ViewModel input relay
3. **ViewModel** → UseCase execution
4. **UseCase** → Repository call
5. **Repository** → Service/API
6. **Response** → UseCase output
7. **ViewModel** → Presenter relay (BehaviorRelay.accept)
8. **Presenter** → ViewController binding
9. **ViewController** → UI update

Each arrow should be verified: is data flowing correctly? Are schedulers correct? Are errors handled?

### Step 5: Run SwiftLint

```bash
swiftlint lint --path <changed-files> --config .swiftlint.yml --strict
```

Fix any violations in the changed code. Don't modify unchanged files.

### Step 6: Verify the Fix

Run the specific scenario that triggers the bug:
- Open the affected screen
- Perform the action
- Verify the expected behavior
- Check for crashes, memory leaks, or state issues
- Run relevant unit tests

### Step 7: Summarize Changes

Explain:
- **What was broken:** The root cause
- **Why it was broken:** The mechanism that caused the issue
- **How it's fixed:** The minimal change applied
- **How to verify:** Steps to confirm the fix works

## Common Patterns & Solutions

### Pattern 1: Missing or Incorrect RxSwift Disposal

**Symptom:** Memory leak, deinit not called, subscription continues after viewDidDisappear

**Check:**
```swift
// Good
let disposeBag = DisposeBag()
observable.subscribe { /* ... */ }.disposed(by: disposeBag)

// Bad
observable.subscribe { /* ... */ } // No disposal!
```

**Fix:** Add `DisposeBag` property and `.disposed(by: disposeBag)` to all subscriptions

### Pattern 2: Threading Issue (UI Update on Background Thread)

**Symptom:** UILabel not updating, crash with "UIKitCore... main thread"

**Check:**
```swift
// Good
useCase.action?.elements
    .observe(on: MainScheduler.instance) // ← Required before UI binding
    .bind(to: presenter.datasource)
    .disposed(by: disposeBag)

// Bad
useCase.action?.elements
    .bind(to: presenter.datasource) // Still on background scheduler!
    .disposed(by: disposeBag)
```

**Fix:** Add `.observe(on: MainScheduler.instance)` before any UI binding

### Pattern 3: Incorrect View Lifecycle Binding

**Symptom:** State not updating, ViewModel not receiving user input

**Check:**
```swift
// Good (in viewDidLoad)
override func viewDidLoad() {
    super.viewDidLoad()
    configurePresenter()
    configureViewModel()
}

private func configurePresenter() {
    presenter?.listener = self // Set listener
}

private func configureViewModel() {
    // Subscribe to presenter relays
    presenter?.datasource
        .subscribe(onNext: { [weak self] data in
            self?.updateUI(data)
        })
        .disposed(by: disposeBag)
}

// Bad (missing listener setup)
override func viewDidLoad() {
    super.viewDidLoad()
    configureViewModel() // But presenter.listener = nil!
}
```

**Fix:** Ensure presenter.listener is set, all relays are subscribed in correct order

### Pattern 4: CTDesignSystem Component Missing

**Symptom:** Inconsistent styling, colors not matching design system, layout issues

**Check:**
```swift
// Good
let label = DSLabel()
label.setStyle(DS.TypoToken.Label.Caption(color: theme.text.textPrimary.color))

// Bad
let label = UILabel() // Wrong component!
label.textColor = UIColor(hex: 0xFF5733) // Hardcoded color!
```

**Fix:** Replace UIKit components with CTDesignSystem equivalents (DSLabel, DSButton, DSTextField, etc.)

### Pattern 5: Layout Constraint Issues

**Symptom:** View disappears, overlaps, or has unexpected size

**Check:**
```swift
// Good (SnapKit)
label.snp.makeConstraints { make in
    make.top.equalTo(containerView.snp.top).offset(16)
    make.leading.trailing.equalTo(containerView).inset(20)
}

// Bad (NSLayoutConstraint, Interface Builder, or frame-based)
label.translatesAutoresizingMaskIntoConstraints = false
NSLayoutConstraint(...).isActive = true // Manual constraints!
```

**Fix:** Use SnapKit for all Auto Layout. Never use NSLayoutConstraint or Interface Builder.

### Pattern 6: Retain Cycle (Weak Self Missing)

**Symptom:** ViewController not deallocating, memory leak

**Check:**
```swift
// Good
observable.subscribe(onNext: { [weak self] value in
    self?.handleValue(value) // Safe unwrap after weak capture
}).disposed(by: disposeBag)

// Bad
observable.subscribe(onNext: { [self] value in
    self.handleValue(value) // Strong capture! Retain cycle!
}).disposed(by: disposeBag)
```

**Fix:** Use `[weak self]` in closures unless you intentionally want strong capture

### Pattern 7: ECS Enum Not Generated

**Symptom:** Type mismatch, ECS property not recognized, build error

**Check:** Has `gen_ecs_enum.py` been run recently?
```bash
python bin/gen_ecs_enum.py
```

**Fix:** Run the generator, commit the changes, rebuild

## Module-Specific Debugging Tips

### CTInsertAd, CTJOB, CTVEH

- Verify `MarketplaceECSHelper` usage for ECS enum interactions
- Check component builder patterns (getComponents(for:) not self.components)
- Verify module DI: check Assembler for correct registration
- Check post vs put lifecycle in edit flows

### CTChat, CTAIChat

- Verify WebSocket state machine (connection → sending → receiving)
- Check message queuing and retry logic
- Verify notification subscriptions (listener setup in viewDidLoad)

### CTAuthentication, CTLogin

- Check token storage and refresh logic
- Verify redirect after login (router navigation)
- Check session invalidation on logout

## Debugging Checklist

Before marking a fix complete:

- [ ] Read only 3-4 relevant files
- [ ] Root cause stated clearly (one sentence)
- [ ] Fix is minimal (only the root cause addressed)
- [ ] Full path traced (trigger → logic → UI)
- [ ] SwiftLint passes on changed files
- [ ] Fix verified in app
- [ ] Unit tests pass (if applicable)
- [ ] No retain cycles introduced
- [ ] No new threading issues
- [ ] CTDesignSystem components used (not UIKit)
- [ ] SnapKit layout used (not NSLayoutConstraint)
