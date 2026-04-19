# Postprocess — ct-feature-pipeline

Run through this checklist after the pipeline completes.

---

## 1. SwiftLint

```bash
swiftlint lint --config .swiftlint.yml --strict [changed files]
```

Fix any warnings or errors before proceeding.

---

## 2. ViewController Wiring

- Wire ViewModel callbacks in the generated ViewController (e.g., `onConfirm`, `onCancel`, `onDismiss`)
- Bind presenter `BehaviorRelay`/`PublishRelay` outputs to UI elements
- Ensure `disposeBag` is used for all RxSwift subscriptions

---

## 3. Dependency Injection

- Register the new UseCase in the module's `Assembler` (e.g., `CRUsecaseAssembler`)
- Register ViewController and ViewModel in their respective Assemblers if not auto-detected
- Verify Swinject container resolves correctly

---

## 4. Build & Test

```bash
fastlane ios tests
```

- Resolve any import or compile errors
- Verify API integration in staging environment
- Smoke test the happy path and error path
