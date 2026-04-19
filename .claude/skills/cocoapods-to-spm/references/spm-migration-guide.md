# SPM Migration Reference Guide

Full reference for migrating CocoaPods dependencies to SPM in the ChoTot iOS monorepo.

---

## Architecture Overview

```
 PODFILE (declare)                    PACKAGE.SWIFT (source of truth)
 ─────────────────                    ──────────────────────────────
 target 'CTFoo' do                   .package(url: "...Bar")
   spm_bar                           .library(name: "BarPackage", type: .dynamic)
 end
                        │
                        ▼
             ┌─── pod install ───┐
             │                   │
             ▼                   ▼
  CocoaPods writes        post_integrate hook
  .xcodeproj files        runs sync_spm_packages.rb
                                 │
                    ┌────────────┼────────────┐
                    ▼            ▼            ▼
             Parse Podfile   Add package  Add product
             for spm_*      reference    dependency
             calls          to .xcodeproj to target
```

**Why `type: .dynamic`?** 20+ modules share the same packages. Static = one copy per module = binary bloat + duplicate symbol crashes. Dynamic = built once, shared at runtime.

**Why `post_integrate` not `post_install`?** CocoaPods overwrites `.xcodeproj` files after `post_install`, wiping SPM references. `post_integrate` runs after all writes are complete.

---

## Step-by-Step Migration

### Step 1 — Package.swift

```swift
// products array — always .dynamic
.library(name: "FooPackage", type: .dynamic, targets: ["FooPackage"]),

// dependencies array
.package(url: "https://github.com/owner/Foo.git", .upToNextMajor(from: "1.0.0")),

// targets array
.target(
    name: "FooPackage",
    dependencies: [.product(name: "Foo", package: "Foo")],
    path: "PackageDependencies/FooPackage"
),
```

### Step 2 — Wrapper Module

Create `PackageDependencies/FooPackage/FooPackage.swift`:

```swift
@_exported import Foo
```

This re-exports everything from Foo so consumers only need `import FooPackage` — or just `import Foo` if the wrapper is in their search path.

### Step 3 — sync_spm_packages.rb: SPM_PACKAGES hash

```ruby
'FooPackage' => {
  url: 'https://github.com/owner/Foo',
  products: ['Foo'],
  requirement: {
    'kind' => 'upToNextMajorVersion',
    'minimumVersion' => '1.0.0'
  }
}
```

Valid `kind` values: `'upToNextMajorVersion'`, `'upToNextMinorVersion'`, `'exactVersion'`

### Step 4 — sync_spm_packages.rb: SPM_METHOD_ALIASES (if needed)

Only needed if the package is bundled inside a Podfile helper method that is NOT named `spm_*`:

```ruby
SPM_METHOD_ALIASES = {
  'uiTools' => ['SnapKitPackage', 'SwiftyGifPackage', 'FooPackage'],  # add here
  ...
}
```

### Step 5 — sync_spm_packages.rb: target case mapping

Every target that needs the package must be in the `case target_name` block:

```ruby
# ── AppFeatures ───────────────────────────────────────
when 'CTFoo'
  ['AppFeatures/CTFoo/CTFoo.xcodeproj', nil]

# ── Libraries ─────────────────────────────────────────
when 'CTBar'
  ['Libraries/CTBar/CTBar.xcodeproj', nil]
```

Missing entry → sync script skips silently → `module 'X' not found` at build time.

### Step 6 & 7 — Podfile

```ruby
# Top of file, SPM declarations section
def spm_foo
  [:spm, 'FooPackage']
end

# Inside each target block that needs it
target 'CTSomeFeature' do
  project 'AppFeatures/CTSomeFeature/CTSomeFeature.xcodeproj'
  rx
  spm_foo   # <- add here
end
```

### Step 8 — Remove old pod

```ruby
# Before
pod 'Foo', '~> 1.0'   # <- remove this line

# After
spm_foo               # <- only SPM declaration remains
```

### Step 9 & 10 — Run and verify

```bash
pod install
# Then in Xcode: Cmd+Shift+K (Clean), Cmd+B (Build)
```

---

## Diagnosing `module 'X' not found`

### Scenario A: Target added to Podfile but missing from sync script

**Symptom:** `pod install` runs without errors but build fails with `module 'X' not found`.

**Diagnosis:** Check `bin/sync_spm_packages.rb` case block for the failing target name.

**Fix:** Add `when 'TargetName' => ['path/to/Target.xcodeproj', nil]`.

### Scenario B: Transitive dependency via generated ObjC header

**Symptom:** TargetB imports LibraryA. LibraryA uses PackageX internally. TargetB fails with `module 'X' not found` even though TargetB never directly uses X.

**Root cause:**
```
LibraryA uses PackageX
  └─ Swift compiler generates LibraryA-Swift.h
       └─ @import X  ← auto-generated because public class conforms to @objc protocol from X
            └─ TargetB imports LibraryA
                 └─ Clang processes LibraryA-Swift.h
                      └─ needs to resolve @import X → FAILS if X not in TargetB's build graph
```

**Fix Option 1 (preferred): Private delegate proxy**

Replace public `@objc` protocol conformance with a private wrapper class:

```swift
// BEFORE — leaks @import SwiftyGif into LibraryA-Swift.h
public class MyView: UIView { }

extension MyView: SwiftyGifDelegate {   // @objc public protocol → leaks!
    public func gifURLDidFinish(sender: UIImageView) { ... }
    public func gifURLDidFail(sender: UIImageView, url: URL, error: Error?) { ... }
}

// AFTER — @import SwiftyGif no longer appears in LibraryA-Swift.h
public class MyView: UIView {
    // swiftlint:disable:next private_outlet
    // Needs internal access so SwiftyGifDelegateProxy can update it
    // without leaking SwiftyGifDelegate into the public ObjC header.
    @IBOutlet weak var backgroundPlayView: UIView!

    private let gifDelegate = SwiftyGifDelegateProxy()

    private func setup() {
        gifDelegate.owner = self
        gifImageView.delegate = gifDelegate
    }
}

// Private proxy — never appears in generated ObjC header
private final class SwiftyGifDelegateProxy: NSObject, SwiftyGifDelegate {
    weak var owner: MyView?

    func gifURLDidFinish(sender: UIImageView) {
        owner?.backgroundPlayView.isHidden = true
    }

    func gifURLDidFail(sender: UIImageView, url: URL, error: Error?) {
        owner?.backgroundPlayView.isHidden = false
    }
}
```

**Why this works:** `private final class` is not part of the module's public API → Swift compiler does NOT emit `@import X` in the generated header → no downstream targets need X.

**Fix Option 2: Add X to the affected target**

Add `spm_x` to the failing target's Podfile block AND add that target to the sync script `case` mapping. Less clean — scales poorly as more targets import the library.

---

## Why `@objc public protocol` Causes the Leak

```
Swift compiler rule:
  IF public class conforms to @objc protocol from module X
  THEN generated -Swift.h MUST contain @import X
  (so ObjC code can reference the protocol type)

Result:
  Every target that imports the library (via ObjC interop)
  needs module X in its build graph.
```

This affects `@objc protocol` and `@objc class` types in public interfaces. Pure Swift protocols (`protocol Foo { }` without `@objc`) do NOT trigger this.

---

## SwiftyGif-specific Notes

`SwiftyGifDelegate` is `@objc public protocol` — any public class conforming to it will always leak `@import SwiftyGif` into the generated header. Always use the private proxy pattern for `SwiftyGifDelegate` conformance in library modules.

```ruby
# Podfile helper
def spm_swifty_gif
  [:spm, 'SwiftyGifPackage']
end

# SPM_METHOD_ALIASES — SwiftyGif bundled inside uiTools
SPM_METHOD_ALIASES = {
  'uiTools' => ['SnapKitPackage', 'SwiftyGifPackage'],
  ...
}
```

---

## Currently Migrated Packages

| Package | Wrapper | Podfile helper | Version |
|---------|---------|----------------|---------|
| FlowStacks | FlowStacksPackage | `spm_flow_stacks` | `0.8.4+` |
| InputBarAccessoryView | InputBarAccessoryViewPackage | `spm_input_bar_accessory_view` | `7.0.2+` |
| SnapKit | SnapKitPackage | `spm_snap_kit` / via `snapkit` alias | `5.7.1+` |
| Kingfisher | KingfisherPackage | `spm_kingfisher` | `7.12.0+` |
| Lottie | LottiePackage | `spm_lottie` / via `lottie` alias | `4.1.3+` |
| SwiftDate | SwiftDatePackage | `spm_swift_date` / via `tools` alias | `6.3.1+` |
| SwiftProtobuf | SwiftProtobufPackage | `spm_swift_protobuf` / via `tools` alias | `1.36.1+` |
| DifferenceKit | DifferenceKitPackage | `spm_difference_kit` / via `realm` alias | `1.3.0+` |
| SwiftyGif | SwiftyGifPackage | `spm_swifty_gif` / via `uiTools` alias | `5.4.4+` |
| SwiftEntryKit | SwiftEntryKitPackage | `spm_swift_entry_kit` / via `popup` alias | `2.0.0+` |

### SKPhotoBrowser — BLOCKED (do not migrate)

Migration was fully attempted and reverted. SKPhotoBrowser cannot be converted to SPM in this
monorepo's hybrid CocoaPods+SPM setup.

**Root cause:** SKPhotoBrowser contains an ObjC extension (`SKPhotoBrowser/extensions/ObjC`) that
CocoaPods compiles into a separate Clang module named `SKPhotoBrowserObjC`. In the SPM version,
this ObjC module still exists but is exposed differently. When any target with
`BUILD_LIBRARY_FOR_DISTRIBUTION = YES` (e.g. CTLiveStream) runs `SwiftVerifyEmittedModuleInterface`,
the Swift frontend tries to compile `SKPhotoBrowserObjC` as a Clang module. This compilation fails
regardless of:
- Whether the target has SKPhotoBrowserPackage in its build graph
- `@_implementationOnly import SKPhotoBrowser` in CTCommon
- Removing `SKPhotoBrowserDelegate` from public API
- Removing `SKPhotoProtocol` conformance from `SKPhotoByKingfisher`

The `GeneratedModuleMaps-iphonesimulator/SKPhotoBrowserObjC.modulemap` is injected into ALL targets
by CocoaPods workspace integration. The SPM-built `SKPhotoBrowserObjC` Clang module cannot be
resolved by the interface verifier in the hybrid setup.

**Keep as:** `pod 'SKPhotoBrowser'` in Podfile indefinitely.

## Blocked Packages

| Package | Reason |
|---------|--------|
| SwiftAlgorithms | `_NumericsShims` C module fails workspace-wide validation in hybrid CocoaPods+SPM setup |
| FSPagerView | `Package.swift` at tag `0.8.3` is empty (no products, no targets) — SPM cannot resolve it, which disables all packages workspace-wide |
| RecaptchaEnterprise | Binary XCFramework (Google) missing x86_64 simulator slice — linker fails with undefined `_OBJC_CLASS_$_Recaptcha` / `_OBJC_CLASS_$_RecaptchaAction` on Intel/Rosetta simulator |
| IGListKit | `CTComponent` publicly exposes IGListKit types (`CTListDataSource`, `CTListSectionController`) — compiler emits `@import IGListKit` in `CTComponent-Swift.h`, causing `SwiftVerifyEmittedModuleInterface` to fail on all AppFeature targets with `BUILD_LIBRARY_FOR_DISTRIBUTION = YES` |

### RecaptchaEnterprise — Blocked

**Problem:** SPM build links the framework but fails at the linker stage with undefined symbols on x86_64 simulator.

**Root cause:** RecaptchaEnterprise is a Google binary XCFramework distributed via SPM. The binary contains only an `arm64` simulator slice — no `x86_64` slice. When Xcode builds for an x86_64 simulator target (Rosetta mode or Intel Mac), the linker resolves the framework but cannot find the ObjC class symbols:

```
Undefined symbols for architecture x86_64:
  "_OBJC_CLASS_$_Recaptcha"
  "_OBJC_CLASS_$_RecaptchaAction"
ld: symbol(s) not found for architecture x86_64
```

**Why CocoaPods worked:** The `static_frameworks` entry caused CocoaPods to force-set `EXCLUDED_ARCHS[sdk=iphonesimulator*] = x86_64` on the pod target, skipping x86_64 compilation entirely. SPM has no equivalent mechanism for binary targets.

**Migration attempt result:** Full migration was attempted (Package.swift, wrapper, sync script, Podfile). Build passed package resolution and fetched successfully, but failed at link time on CTCommon. Fully reverted.

**Keep as:** `pod 'RecaptchaEnterprise'` in Podfile indefinitely unless one of the following is true:
- Google publishes an XCFramework with an x86_64 simulator slice
- The entire team + CI confirms native arm64 simulator only (no Rosetta)

---

## IGListKit — BLOCKED (do not migrate)

**Problem:** SPM build causes `SwiftVerifyEmittedModuleInterface` to fail with `module 'IGListKit' not found` on all targets with `BUILD_LIBRARY_FOR_DISTRIBUTION = YES` (e.g. CTLiveStream).

**Root cause:** `CTComponent` publicly exposes IGListKit types as part of its own API — `CTListDataSource` is `open class` conforming to `ListAdapterDataSource` (`@objc` protocol from IGListKit), and `CTListSectionController` is `open class` extending `ListBindingSectionController<ListDiffable>`. The Swift compiler **bắt buộc** generate `@import IGListKit` vào `CTComponent-Swift.h`:

```objc
// CTComponent-Swift.h (auto-generated)
@import IGListKit;   // ← compiler tự inject vì public API dùng IGListKit @objc types
```

Unlike other packages where the private delegate proxy pattern removes the leak, `CTListDataSource` và `CTListSectionController` **chính là** các IGListKit adapter — chúng kế thừa và expose `ListAdapterDataSource`, `ListSectionController` như là public API. Making them private is not an option.

```
CTComponent.framework/Headers/CTComponent-Swift.h:354:9: error: module 'IGListKit' not found
@import IGListKit;
could not build Objective-C module 'CTComponent'
failed to verify module interface of 'CTLiveStream'
```

CocoaPods path worked because IGListKit builds as `IGListKit.framework` and CocoaPods injects it into **all** targets workspace-wide via `OTHER_LDFLAGS` and `FRAMEWORK_SEARCH_PATHS` — so `SwiftVerifyEmittedModuleInterface` always finds it. With SPM, the framework is only injected into explicitly linked targets, and the verifier runs in a separate compiler invocation with more restricted search paths — so it still can't find IGListKit even after linking it to CTLiveStream.

**Action:** Attempted full migration (Package.swift, wrapper, sync script, Podfile). Also attempted: adding `igList` to CTLiveStream, wiring IGListKit into `package_product_dependencies` and `frameworks_build_phase`, clean DerivedData. All failed — `SwiftVerifyEmittedModuleInterface` cannot resolve `@import IGListKit` from `CTComponent-Swift.h` in the hybrid setup. Fully reverted.

**Keep as:** `pod 'IGListKit'` in Podfile indefinitely unless one of the following is true:
- `CTComponent` is refactored to hide IGListKit types from its public API (breaking change for all consumers)
- `BUILD_LIBRARY_FOR_DISTRIBUTION` is turned off on CTLiveStream — eliminates `SwiftVerifyEmittedModuleInterface` (see note below)
- The project fully migrates away from CocoaPods to pure SPM (no hybrid setup)

---

### `BUILD_LIBRARY_FOR_DISTRIBUTION` — context và trade-off

**Nó làm gì khi bật:** compiler thêm 3 thứ vào build pipeline:
- `-enable-library-evolution` — compile với ABI stability support
- Generate `.swiftinterface` — text-based interface file thay vì chỉ binary `.swiftmodule`
- `SwiftVerifyEmittedModuleInterface` — bước post-compile verify lại interface đó

Đây là setting dùng cho framework **phân phối dưới dạng binary** (precompiled) — ví dụ: Apple's SDKs, third-party closed-source frameworks. Mục đích: cho phép framework compiled với Swift 5.8 vẫn dùng được với app compile bằng Swift 5.9.

**Chỉ có CTLiveStream bật cờ này** — không có module nào khác trong AppFeatures hay Libraries. Cờ này được set vào thời điểm tích hợp BytePlus SDK (TTSDK).

**Tại sao nó gây vấn đề với IGListKit SPM:**

```
CTLiveStream compile xong
  → SwiftVerifyEmittedModuleInterface chạy
    → verify CTLiveStream.swiftinterface
      → import CTComponent
        → CTComponent-Swift.h có @import IGListKit
          → IGListKit không resolve được trong context SPM hybrid
            → BUILD FAILED ❌
```

`SwiftVerifyEmittedModuleInterface` chạy với một invocation compiler riêng biệt, search paths hạn chế hơn compile bình thường — nên dù IGListKit đã link vào CTLiveStream, step này vẫn không tìm thấy.

**Nếu tắt `BUILD_LIBRARY_FOR_DISTRIBUTION` trên CTLiveStream:**

| Khía cạnh | Ảnh hưởng | Mức độ |
|---|---|---|
| Build trong monorepo | ✅ Không ảnh hưởng — tất cả module compile từ source cùng 1 Xcode | An toàn |
| BytePlus SDK (TTSDK) | ✅ Không ảnh hưởng — CTLiveStream *dùng* BytePlus, không phải ngược lại | An toàn |
| App submission lên AppStore | ✅ Không ảnh hưởng — module nội bộ, không phân phối binary ra ngoài | An toàn |
| `.swiftinterface` file | ⚠️ Sẽ không generate — module tied với đúng Swift compiler version | Chấp nhận được trong monorepo |
| `SwiftVerifyEmittedModuleInterface` | ✅ Không chạy nữa → IGListKit issue biến mất | Đây là mục tiêu |
| Build speed | ✅ Nhẹ hơn — bỏ được 1 verification step | Bonus |

**Kết luận:** An toàn để tắt trong context monorepo này — tất cả modules compile từ source cùng nhau, không cần ABI stability. Tuy nhiên cờ này được set có chủ ý khi tích hợp BytePlus — nên xác nhận lại với team marketplace/BytePlus trước khi tắt.

---

## IGListKit — SPM version note (for reference only)

**Problem:** IGListKit 4.0.0's `Package.swift` has a broken ObjC header path configuration.

**Symptom:**
```
fatal error: 'IGListDiffKit/IGListMacros.h' file not found
```

**Root cause:** In 4.0.0, the `IGListDiffKit` SPM target sets `publicHeadersPath: "include"`, but headers like `IGListMacros.h` are in the target root (`Source/IGListDiffKit/`), not under an `include/` subdirectory. The compiler gets `-I.../Source/include` and cannot resolve `<IGListDiffKit/IGListMacros.h>`. Fixed in 5.0.0 which corrects the `publicHeadersPath` and header layout.

**Fix:** Use `from: "5.0.0"` in both `Package.swift` and `SPM_PACKAGES` in `sync_spm_packages.rb`.

**Breaking API changes from 4.x → 5.x** to fix after upgrading:

| Removed (4.x) | Replacement (5.x) |
|---|---|
| `updater.experiments = [.backgroundDiffing]` | Remove — background diffing is now default behavior |
| `.reloadDataFallback` in `adapter.experiments` | Remove — promoted to default behavior |
| `.invalidateLayoutForUpdates` | Still available — keep as-is |

**Lý do remove `.backgroundDiffing` và `.reloadDataFallback`:**

Trong IGListKit 4.x, hai flag này là **opt-in experiments** — phải bật thủ công để sử dụng tính năng. Từ IGListKit 5.0.0, cả hai đã được **promote thành default behavior** (luôn bật sẵn), nên enum case bị xóa khỏi `IGListExperiment`. Nếu giữ lại sẽ gây lỗi compile:

```
error: type 'IGListExperiment' has no member 'backgroundDiffing'
error: type 'IGListExperiment' has no member 'reloadDataFallback'
```

**Fix áp dụng tại** `Libraries/CTComponent/CTComponent/Core/UI/CTListViewController/CTListView.swift`:

```swift
// BEFORE (4.x)
let updater = ListAdapterUpdater()
updater.experiments = [.backgroundDiffing]          // ❌ removed in 5.x
self.adapter.experiments = [.reloadDataFallback,    // ❌ removed in 5.x
                             .invalidateLayoutForUpdates]

// AFTER (5.x)
let updater = ListAdapterUpdater()
// backgroundDiffing: không cần set, đã là default
self.adapter.experiments = [.invalidateLayoutForUpdates]  // ✅ vẫn còn
```

`.invalidateLayoutForUpdates` vẫn giữ nguyên vì nó vẫn là optional flag trong 5.x (chưa được promote thành default).

---

## Troubleshooting

| Error | Cause | Fix |
|-------|-------|-----|
| `module 'X' not found` | Target missing from sync script case block | Add `when 'TargetName'` entry |
| `module 'X' not found` (transitive) | Public class conforms to `@objc` protocol from X | Private proxy pattern |
| `No such module 'X'` after pod install | `spm_x` not called in target's Podfile block | Add `spm_x` to Podfile target |
| Build works locally, fails on CI | `Package.resolved` not committed | Commit `Package.resolved` |
| Duplicate symbol crash at runtime | Package linked as static in multiple modules | Ensure `type: .dynamic` in Package.swift |
| Sync script runs but xcodeproj unchanged | Target not in `case target_name` block | Add `when` entry to sync script |
| `'IGListDiffKit/X.h' file not found` | IGListKit version < 5.0.0 has broken header paths in SPM | Use `from: "5.0.0"` |
