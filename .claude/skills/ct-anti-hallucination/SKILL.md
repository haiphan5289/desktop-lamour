---
name: ct-anti-hallucination
description: Anti-hallucination guardrails for all Cho Tot iOS code generation. Enforces verify-before-use for every symbol, token, path, and identifier. Referenced by all other ct-* skills. Invoke directly when you suspect generated code references non-existent APIs, wrong module names, invented DS tokens, or stale file paths.
---

# Anti-Hallucination Rules for Cho Tot iOS

> These rules apply to **every code generation task** in this project.  
> Before writing a single line of code, complete the verification checklist below.

---

## The Core Rule

**Never reference any symbol, path, token, or identifier you have not verified exists in the current codebase.**

A memory, a prior conversation, or a reference example file is NOT proof that something exists now. Code is the only source of truth.

---

## Pre-Generation Verification Checklist

Complete every applicable item before generating code.

### 1. File Paths
- [ ] Use `Glob` to confirm every target file path exists before reading or referencing it
- [ ] If a path does not exist, ask the user — do NOT invent an alternative path
- [ ] Never assume a subfolder exists because a sibling folder exists

### 2. Class / Protocol / Struct Names
- [ ] Use `Grep` to find the exact declaration (`class Foo`, `protocol FooType`, `struct Foo`) before using it
- [ ] Check the module it belongs to — the same name can exist in multiple modules with different behaviour
- [ ] Never assume a class name based on a naming pattern (e.g. `CRFooRepository`) without verifying

### 3. Method / Property Signatures
- [ ] Read the actual file containing the class/protocol before calling any method on it
- [ ] Verify parameter labels, types, and return types exactly — do not guess from the method name
- [ ] If a method has changed signature since a reference example, use the current signature

### 4. CTDesignSystem Tokens
- [ ] Verify `DS.TypoToken.*` paths by reading `Libraries/CTDesignSystem` before use
- [ ] Verify `DS.Button.*` style names exist (e.g. `.primary`, `.secondary`, `.ghost`)
- [ ] Verify `theme.*` property chains (`theme.text.textPrimary`, `theme.line.linePrimary`, etc.) in `CTTheme.swift`
- [ ] **NEVER use**: `DS.T14B`, `DS.T10R`, `DS.T12B`, `CTColor.*` — these are legacy APIs
- [ ] **NEVER use**: raw `UIColor`, `UIFont.systemFont`, `.label`, `.secondaryLabel`

### 5. Import Statements
- [ ] Only add `import X` for a framework that is actually used in the generated code
- [ ] Verify the framework is available in the target module's podspec/Package.swift
- [ ] Do not copy import blocks from reference files blindly — a different module may not have the same dependencies

### 6. Storyboard / XIB Identifiers
- [ ] Read a sibling storyboard in the same module to get the correct `customModule` value (never guess — it differs between `ChoTot`, `CTCorePayment`, `CTJOB`, etc.)
- [ ] `storyboardIdentifier` must match the ViewController class name exactly
- [ ] `customClass` for `DSLabel`/`DSButton` always uses `customModule="CTDesignSystem"`
- [ ] `PaddingLabel` uses `customModule="CTComponent"`

### 7. project.pbxproj UUIDs
- [ ] **Always generate fresh UUIDs** with `uuidgen | tr -d '-' | cut -c1-24` — never reuse UUIDs from examples or memory
- [ ] Verify the parent group UUID by searching `project.pbxproj` for a sibling file in the same folder
- [ ] Verify the Sources and Resources build phase UUIDs by reading the pbxproj around an existing entry in the same target

### 8. API Endpoints and NetworkHelper Keys
- [ ] Read `NetworkHelper.swift` (or `Api.swift`) to confirm the key does not already exist before adding
- [ ] Verify the endpoint path format matches existing entries (trailing slash, prefix pattern)
- [ ] Never invent an endpoint path — use the one provided in the task

### 9. Localization Keys
- [ ] Do not use `ctLocalize(for:tableName:)` — use typed accessors like `JBLocalize.foo()` from CTLocalize
- [ ] Verify the localization key exists by grepping the `.strings` file before using it
- [ ] Never invent a localization key — ask the user if it is missing

### 10. Reference Files Are Patterns, Not Copy-Paste Sources
- [ ] Reference files (e.g. `CRNoticeShareAdViewController.swift`) show **structural patterns only**
- [ ] Every symbol copied from a reference must be individually verified in the current codebase
- [ ] Legacy patterns in reference files (e.g. `CTColor.*`, `DS.T14B`) must be replaced with modern equivalents

---

## Hallucination Red Flags — Stop and Verify

If you find yourself doing any of the following, **stop and verify** before continuing:

| Red flag | What to do instead |
|---|---|
| Writing `DS.TypoToken.Header.Foo` without checking | Grep `TypoToken` in CTDesignSystem |
| Using `theme.text.textFoo` without checking | Read `CTTheme.swift` |
| Writing `import SomeFramework` from memory | Check podspec or grep existing files in the module |
| Referencing `CRFooRepository` because it "should" exist | Glob for the file first |
| Pasting a UUID from an example | Run `uuidgen` |
| Adding `Api.foo = "/v2/foo"` from memory | Read `NetworkHelper.swift` first |
| Using `.posTheme` for a non-POS module | Read sibling VCs to find the correct theme |
| Assuming a method signature from its name | Read the protocol declaration |

---

## When Verification Fails

If a required symbol, path, or token cannot be found in the codebase:

1. **Do not invent a substitute** — report what is missing
2. **Ask the user** before proceeding: _"I could not find `X` in the codebase. Can you point me to the correct name/path?"_
3. If the user confirms it does not exist yet: create it following existing patterns, and flag it clearly as a **new addition**

---

## Quick Verification Commands

```bash
# Verify a class/protocol exists
Grep: pattern="class FooViewModel|protocol FooViewModelType"

# Verify a DS token path
Grep: pattern="Header\.Section|Label\.Page" path="Libraries/CTDesignSystem"

# Verify a theme property chain
Grep: pattern="textPrimary|linePrimary|backgroundWarning" path="Libraries/CTDesignSystem"

# Verify an API key
Grep: pattern="Api\." path="<module>/NetworkHelper.swift"

# Verify a file path
Glob: pattern="**/FooViewController.swift"

# Verify storyboard customModule
Read: a sibling .storyboard in the same module folder
```
