---

name: cocoapods-to-spm
description: "Migrate a CocoaPods dependency to SPM in the ChoTot iOS monorepo — centralized Package.swift wrapper pattern, Podfile helpers, sync script mapping, and common pitfalls (transitive @objc protocol leaks, missing target mappings)."
argument-hint: "[package1] [package2] ... — names only, Claude reads Podfile.lock for versions and resolves GitHub URLs automatically"

---

# CocoaPods → SPM Migration Skill

Migrate a third-party dependency from CocoaPods to the centralized SPM wrapper system used in the **Chợ Tốt iOS** monorepo.

**Last synced:** 2026-03-31

---

## Examples

```bash
# Single package — auto resolve version + URL
/cocoapods-to-spm DGCharts

# Multiple packages — run sequentially, one pod install at the end
/cocoapods-to-spm DGCharts IQKeyboardManagerSwift FSPagerView

# Explicit URL + version
/cocoapods-to-spm DGCharts https://github.com/danielgindi/Charts 5.1.0
```

---

## When to Use

* Convert CocoaPods → SPM
* Add new SPM package
* Fix `module 'X' not found`
* Debug `@import X` in ObjC header

---

# 🔒 Read Guide (MANDATORY)

| Task                                  | File                                |
| ------------------------------------- | ----------------------------------- |
| Full migration checklist & edge cases | `references/spm-migration-guide.md` |
| Migration status & blocked packages   | `references/project_spm_migration.md` |
| Reverting SPM → CocoaPods (dual-link crash, sub-module cleanup) | `references/revert-spm-to-cocoapods.md` |

## ❗ Rules

* MUST read guide before ANY change
* MUST treat guide as source of truth
* If conflict → follow guide

## 🚫 Failure Handling

If guide:

* Missing / unreadable / unclear
  → **STOP and ask user**

## 🧠 Behavior Rule

* Do NOT assume knowledge from memory
* Do NOT skip reading
* Always align with guide

---

# 🧠 Execution Flow (FOR CLAUDE)

## 1. Parse Input

* Extract package names
* Detect optional URL/version

## 2. Read Source of Truth

* Read `Podfile.lock`

  * version
  * source

## 3. Resolve Package Info

* If URL missing → resolve GitHub repo
* Normalize version → `from:`

---

## 4. For EACH package

### 4.1 Update `Package.swift`

```swift
.package(
    url: "https://github.com/xxx/xxx.git",
    from: "1.2.3"
),
```

```swift
.library(
    name: "<Name>",
    type: .dynamic,
    targets: ["<Name>"]
),
```

```swift
.target(
    name: "<Name>",
    dependencies: [
        .product(name: "<Name>", package: "<Package>")
    ]
)
```

---

### 4.2 Create Wrapper

Path:

```
PackageDependencies/<Name>Package/<Name>Package.swift
```

```swift
@_exported import <Name>
```

---

### 4.3 Update Sync Script

File: `bin/sync_spm_packages.rb`

* Add to `SPM_PACKAGES`
* Add to `SPM_METHOD_ALIASES` (if needed)

Ensure target mapping:

```ruby
case target_name
when 'TargetName'
  ['path/to/project.xcodeproj', nil]
end
```

---

### 4.4 Update Podfile

```ruby
def spm_<snake_name>
  [:spm, '<Name>Package']
end
```

* Add to ALL relevant targets
* Remove:

```ruby
pod '<Name>'
```

---

## 5. Final Step (RUN ONCE)

Run `pod install` automatically using the Bash tool — do not ask the user to run it manually:

```bash
pod install
```

---

# 🔁 Idempotency Rules

* Do NOT duplicate:

  * `.package`
  * `SPM_PACKAGES`
  * Podfile helper
* Safe to re-run multiple times
* Only add missing parts

---

# ⚠️ Common Pitfalls (WITH DETECTION)

---

## 1. `module 'X' not found`

### 🔍 Detection

* Build error after migration
* Appears in unrelated targets

### 🧠 Root Cause

* Leaked via `-Swift.h`
* Often from `@objc public protocol`

### ✅ Fix

**Preferred**

* Private delegate proxy (remove from public API)

**Fallback**

* Add `spm_x` to target
* Update sync script

---

## 2. Sync Script Skips Target

### 🔍 Detection

* `pod install` OK
* Package not linked in Xcode

### 🧠 Root Cause

* Missing `case target_name`

### ✅ Fix

```ruby
when 'YourTarget'
  ['path/to/project.xcodeproj', nil]
```

---

## 3. ObjC Header Leak

### 🔍 Detection

```objc
@import PackageName;
```

### 🧠 Root Cause

* Public API exposes third-party types

### ✅ Fix

* Use private wrapper / proxy

---

## 4. SKPhotoBrowser Migration Note (REAL CASE)

### 🔍 Detection

Build failed in unrelated target after converting `SKPhotoBrowser`, for example:

```objc
/Build/Products/.../CTCommon.framework/Headers/CTCommon-Swift.h:354:9: error: module 'SKPhotoBrowser' not found
@import SKPhotoBrowser;
```

Typical compile path seen in this repo:

* `CTApiClient` compiles
* Swift compiler imports `CTCommon-Swift.h`
* generated header contains `@import SKPhotoBrowser`
* build fails with `Could not build Objective-C module 'CTCommon'`

### 🧠 Root Cause

This repo hit multiple issues at the same time:

1. **Partial migration only**

  * `Package.swift` + Podfile helper already existed
  * but `PackageDependencies/SKPhotoBrowserPackage/SKPhotoBrowserPackage.swift` was missing
  * and `bin/sync_spm_packages.rb` did not fully register/package-link it

2. **Acronym parsing bug in sync script**

  * `spm_sk_photo_browser` was normalized to `SkPhotoBrowserPackage`
  * real package key is `SKPhotoBrowserPackage`
  * result: target was parsed as using the helper, but package key lookup failed silently

3. **Alias path missing for helper-based usage**

  * many targets use `photoReview`
  * if `photoReview -> SKPhotoBrowserPackage` is not in `SPM_METHOD_ALIASES`, sync script misses those targets

4. **Package dependency existed but Frameworks phase link was missing**

  * `packageProductDependencies` could contain `SKPhotoBrowser`
  * but `PBXFrameworksBuildPhase.files` did not contain `SKPhotoBrowser in Frameworks`
  * result: project file looked partially correct, but compiler still could not resolve the module

5. **Public API leak from `CTCommon`**

  * `CTCommon` exposes SKPhotoBrowser-related public types, e.g. `SKPhotoProtocol` / `SKPhotoBrowserDelegate`
  * generated `CTCommon-Swift.h` therefore emits `@import SKPhotoBrowser`
  * every consumer that imports `CTCommon` through ObjC interop must also resolve `SKPhotoBrowser`

### ✅ Fix Applied In This Repo

For `SKPhotoBrowser`, the working fix was:

1. Create wrapper file:

```swift
// PackageDependencies/SKPhotoBrowserPackage/SKPhotoBrowserPackage.swift
@_exported import SKPhotoBrowser
```

2. Ensure `bin/sync_spm_packages.rb` contains:

  * `SPM_PACKAGES['SKPhotoBrowserPackage']`
  * `SPM_METHOD_ALIASES['photoReview'] = ['SKPhotoBrowserPackage']`

3. Fix direct helper parsing for acronym package names:

  * `spm_sk_photo_browser` must resolve to `SKPhotoBrowserPackage`
  * similar class of bugs can affect `IQKeyboardManagerSwiftPackage`

4. Ensure sync script adds BOTH:

  * `target.package_product_dependencies << product_dep`
  * `PBXBuildFile(productRef: product_dep)` into `target.frameworks_build_phase.files`

5. Run `pod install` again so `post_integrate` rewrites the `.xcodeproj`

### 🧪 Validation Signals

Expected `pod install` log after fix:

```text
Found SKPhotoBrowserPackage for target CTApiClient
Syncing CTApiClient.xcodeproj to use workspace SKPhotoBrowserPackage
Linking product in Frameworks phase: SKPhotoBrowser
Updated CTApiClient.xcodeproj
```

Expected project file state:

```text
SKPhotoBrowser in Frameworks
packageProductDependencies = (... SKPhotoBrowser ...)
packageReferences = (... XCRemoteSwiftPackageReference "SKPhotoBrowser" ...)
```

### ⚠️ Extra Notes

* If error still appears after project file is correct, clean DerivedData before trusting the result.
* CLI validation with `xcodebuild` may fail for unrelated local setup reasons if Xcode is using legacy build locations.
* Long-term better fix is to reduce public exposure of `SKPhotoBrowser` symbols from `CTCommon`; otherwise downstream targets must keep linking it.

---

## 5. RxSwift Family (BLOCKED)

Do NOT migrate:

* RxSwift
* RxCocoa
* RxDataSources
* RxGesture
* RxOptional
* Action

### ❌ Issues

* `Symbol not found`
* SDK crash

---

## 6. RxTest / RxBlocking Crash

* Test-only libs
  → Never link to app target

---

## 7. Static Lib in Embed Script

### ❌ Problem

* Added to `Pods-*-frameworks.sh`

### 💥 Result

* `rsync` error

### ✅ Fix

* Only `.framework` allowed

---

## 8. post_integrate Cleanup

Must clean BOTH:

* `.xcfilelist`
* `Pods-*-frameworks.sh`

---

# 🧪 Validation Checklist

## Build

* [ ] Clean (`Cmd + Shift + K`)
* [ ] Build (`Cmd + B`)

## Runtime

* [ ] App launches
* [ ] No crash

## Dependency

* [ ] No `module not found`
* [ ] No duplicate symbols
* [ ] No unexpected `@import`

## Structure

* [ ] Correct target linking
* [ ] Podfile cleaned

---

# 📁 Key Files

| File                                | Role            |
| ----------------------------------- | --------------- |
| `Package.swift`                     | Source of truth |
| `PackageDependencies/`              | Wrapper         |
| `bin/sync_spm_packages.rb`          | Inject SPM      |
| `Podfile`                           | Declare usage   |
| `references/spm-migration-guide.md`     | Ground truth       |
| `references/project_spm_migration.md`   | Migration status — ✅ converted (15), ⛔ blocked (SKPhotoBrowser, RecaptchaEnterprise, IGListKit, HMSegmentedControl, SwiftAlgorithms, FSPagerView...) |
| `references/revert-spm-to-cocoapods.md` | Revert guide — dual-link SIGABRT, sub-module SPM cleanup checklist |

---

# 🧠 Claude Behavior Rules

* ALWAYS read `Podfile.lock`
* ALWAYS update ALL targets
* NEVER assume mapping exists
* PREFER repo patterns
* If missing info → ASK

---

# 🚀 Outcome

* Deterministic
* Idempotent
* Debuggable
* Safe for monorepo
* AI-agent ready

---
