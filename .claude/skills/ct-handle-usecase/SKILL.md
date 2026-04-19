---
name: ct-handle-usecase
description: Add a UseCase execution method to an existing ViewModel following MVVM + Clean Architecture. Generates the execute{UseCaseName}() method with elements (success), executing (loading), and underlyingError (error) bindings. Use when wiring a new UseCase into an existing ViewModel. Critical: never use .observe(on:) for underlyingError, never use .bind(onNext:).
---

# ViewModel UseCase Execution Guide

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Add a UseCase execution method to an existing ViewModel, following the MVVM + Clean Architecture pattern.

## Input Format

```
USECASE_NAME: <UseCaseName, e.g. "FetchUserProfile">
INPUT_PARAM: <Input type, e.g. "String" or "UserRequest">
VIEWMODEL_CLASS: <ViewModel class name, e.g. "CRCheckoutPageViewModel">
REPO_PROPERTY_NAME: <Repository property in the ViewModel, e.g. "checkoutRepo">
```

## Generated Method Template

```swift
// Add to existing ViewModel class via extension
extension [ViewModelClass] {
    func execute[UseCaseName](input: [InputParam]) {
        let useCase = CR[UseCaseName]UseCase(repository: self.[repoPropertyName])

        // Success — observe on MainScheduler for UI updates
        useCase.action?.elements
            .observe(on: MainScheduler.instance)
            .subscribe(onNext: { [weak self] result in
                // TODO: Handle success result
                // self?.presenter?.data.accept(result)
            })
            .disposed(by: disposeBag)

        // Loading state
        useCase.action?.executing
            .observe(on: MainScheduler.instance)
            .subscribe(onNext: { [weak self] isLoading in
                self?.presenter?.loading.accept(isLoading)
            })
            .disposed(by: disposeBag)

        // Error handling — minimal, NO observe(on:) needed
        useCase.action?.underlyingError
            .subscribe(onNext: { [weak self] error in
                guard let self = self else { return }
                // Minimal error handling
            })
            .disposed(by: disposeBag)

        // Execute
        useCase.action?.execute(input)
    }
}
```

## Common Repository Property Names

| ViewModel | Repository Property |
|-----------|-------------------|
| `CRCheckoutPageViewModel` | `checkoutRepo` |
| `CRTopupDongtotViewModel` | `dongtotRespository` |
| `POSViewModel` | `posRepo` |
| `VEHViewModel` | `vehRepo` |

## Critical Rules

### ❌ NEVER DO THESE:

```swift
// ❌ WRONG: observe(on:) for underlyingError
useCase.action?.underlyingError
    .observe(on: MainScheduler.instance)  // NOT needed
    .subscribe(...)

// ❌ WRONG: bind(onNext:) — always use subscribe(onNext:)
useCase.action?.elements
    .bind(onNext: { result in ... })  // Use subscribe instead

// ❌ WRONG: complex error unwrapping
useCase.action?.underlyingError
    .subscribe(onNext: { [weak self] error in
        if let err = error as? CustomError {  // Keep it minimal
            ...
        }
    })
```

### ✅ CORRECT PATTERNS:

```swift
// ✅ Correct: elements with MainScheduler + subscribe
useCase.action?.elements
    .observe(on: MainScheduler.instance)
    .subscribe(onNext: { [weak self] result in
        self?.presenter?.data.accept(result)
    })
    .disposed(by: disposeBag)

// ✅ Correct: underlyingError — no observe(on:), minimal handling
useCase.action?.underlyingError
    .subscribe(onNext: { [weak self] error in
        guard let self = self else { return }
    })
    .disposed(by: disposeBag)
```

## Architecture Notes

- UseCase is created inside the execution method (not stored as property)
- Repository is injected from the ViewModel's own repository property
- Use `[weak self]` in all closures to prevent retain cycles
- `disposed(by: disposeBag)` on every subscription
- Add method as an `extension` on the ViewModel class for organization
- The UseCase naming convention is `CR{UseCaseName}UseCase`
