# Postprocess — ct-figma-storyboard

Run through this checklist after generation completes.

---

## 1. SwiftLint

```bash
swiftlint lint --config .swiftlint.yml --strict [changed files]
```

Fix any warnings or errors before proceeding.

---

## 2. ViewController Wiring

- Wire ViewModel callbacks in the generated ViewController (e.g., `onConfirm`, `onCancel`, `onDismiss`)
- Connect `@IBOutlet` and `@IBAction` in Xcode's Interface Builder if not already wired via the storyboard XML
- Verify all outlets match the ViewController property names exactly

---

## 3. Dependency Injection

- Register the ViewController in the module's Assembler (e.g., `CRViewControllerAssembler`) if not auto-detected
- Verify Swinject container resolves the ViewController and any injected dependencies correctly

---

## 4. Xcode Project

- Open Xcode and verify all 5 (or 10+) registered files appear in the Project Navigator under the correct group
- Build the target to confirm no compile errors from missing outlets, wrong module names, or missing imports
- Open the `.storyboard` in Interface Builder to confirm it renders without "Failed to unarchive" errors

---

## 5. Build & Smoke Test

```bash
fastlane ios tests
```

- Resolve any import or compile errors
- Present the generated ViewController from a parent screen
- Smoke test: buttons dismiss correctly, callbacks fire, no layout constraint warnings in console
