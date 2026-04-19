---
name: SPM Migration — Status
description: Full migration status — 15 converted, pending (medium/hard), blocked (Rx group + Firebase group), reverted (Lottie/SwiftDate/Swinject/Kingfisher — x86_64 CI fail), and never-migrate list. Hybrid CocoaPods+SPM workspace.
type: project
---

## ✅ Đã convert sang SPM (15 packages)

| Package | Podfile helper |
|---|---|
| SnapKit | `spm_snap_kit` / `snapkit` |
| SwiftyGif | `spm_swifty_gif` / `uiTools` |
| SwiftProtobuf | `spm_swift_protobuf` / `tools` |
| DifferenceKit | `spm_difference_kit` / `realm` |
| SwiftEntryKit | `spm_swift_entry_kit` / `popup` |
| FlowStacks | `spm_flow_stacks` |
| InputBarAccessoryView | `spm_input_bar_accessory_view` |
| DGCharts | `spm_dg_charts` / `chart` |
| IQKeyboardManagerSwift | `spm_iq_keyboard_manager_swift` / `uiTools` |
| SkyFloatingLabelTextField | `spm_sky_floating_label_text_field` / `layout` |
| Cache (hyperoslo) | `spm_cache` |
| GCDWebServer | `spm_gcd_web_server` |
| PanModal (Carousell fork) | `spm_pan_modal` / `panModal` / `uiTools` |
| FBSDKLoginKit (FacebookLogin) | `spm_facebook_login` / `social` |
| FBSDKShareKit (FacebookShare) | `spm_facebook_share` / `social` |

---

## ⚠️ Reverted về CocoaPods (SPM migration fail trên CI)

| Package | Podfile helper | Lý do |
|---|---|---|
| Lottie | `spm_lottie` / `lottie` | SPM chỉ build arm64 simulator slice — `ARCHS=x86_64` trên CI gây undefined symbols |
| SwiftDate | `spm_swift_date` / `tools` | Cùng vấn đề x86_64 SPM slice thiếu |
| Swinject + SwinjectAutoregistration | `spm_swinject` / `dependencyInjection` | Cùng vấn đề x86_64 SPM slice thiếu |
| Kingfisher | `spm_kingfisher` | OnFlowSDK conflict — 7 sub-projects cleaned (CTAIChat, CTAdView, CTJOB, CTShop, CTEcommerce, CTDesignSystem, CTCommon) |

**Root cause:** SPM build framework chỉ dùng native arch của runner (arm64). Fastfile cũ có `ARCHS=x86_64` → linker tìm x86_64 symbols trong SPM binary → không có → `Undefined symbols`. CocoaPods build fat framework (arm64 + x86_64) nên không bị ảnh hưởng.

**How to apply:** Có thể retry migrate về SPM sau khi xác nhận CI runner dùng `ARCHS=arm64` hoặc `ONLY_ACTIVE_ARCH=YES` thay vì force x86_64.

**Facebook SDK notes:**
- Source: `https://github.com/facebook/facebook-ios-sdk`, version `17.0.1+`
- Wrappers: `FacebookLoginPackage`, `FacebookSharePackage` (same SPM package `facebook-ios-sdk`)
- Targets linked: ChoTot + CTShop (via `social` alias), CTReward (direct `spm_facebook_share`)
- `social` alias added to `SPM_METHOD_ALIASES` in `bin/sync_spm_packages.rb`
- Prebuilt XCFramework — requires build + runtime test before marking fully stable

---

## 🔜 Chưa convert

### 🟢 Dễ — Làm ngay

*(Không còn package nào ⛔)*

### 🟡 Cần test kỹ

*(Không còn package nào — FBSDKLoginKit + FBSDKShareKit đã convert)*

### 🟠 Khó — Cần plan riêng

| Package | Ghi chú |
|---|---|
| R.swift | Build Tool Plugin, cấu hình hoàn toàn khác |
| SwiftLint | Command plugin, không phải library |
| Realm + RealmSwift | Binary dependency, hay conflict |
| Firebase suite (FirebaseCore, FirebaseAnalytics, FirebaseRemoteConfig, FirebaseCrashlytics, FirebasePerformance, GoogleTagManager, FirebaseAuth) | Duplicate symbols trong monorepo, cần plan kỹ |
| Google-Mobile-Ads-SDK | Binary xcframework |
| GoogleMaps + GooglePlaces | Binary xcframework |

---

## 🔴 Blocked — Chờ remove CarousellChatSDK + OnFlowSDK

| Package | Lý do |
|---|---|
| Alamofire | Cả 2 SDK require Alamofire = 5.9.1 |
| RxSwift | ABI mismatch với 2 SDK pre-built |
| RxCocoa | Cùng nhóm Rx |
| RxRelay | Cùng nhóm Rx |
| RxDataSources | Cùng nhóm Rx |
| RxGesture | Cùng nhóm Rx |
| RxOptional | Cùng nhóm Rx |
| RxBlocking | Sub-product của RxSwift package, depend on `RxSwift = 6.5.0` → blocked cùng nhóm |
| RxTest | Sub-product của RxSwift package, depend on `RxSwift = 6.5.0` → blocked cùng nhóm |
| Action | Depend on RxSwift |
| ObjectMapper | CarousellChatSDK depend on `pod 'ObjectMapper'` → 3 bản sao static+dynamic → duplicate `BaseMappable` descriptors → abort crash |

**Why:** CarousellChatSDK + OnFlowSDK là pre-built binaries compiled với RxSwift 6.5.x qua CocoaPods. Khi RxSwift chạy từ SPM: path khác → framework lookup fail, ABI có thể khác → symbol mismatch. ObjectMapper cùng pattern: CarousellChatSDK force `ObjectMapper.framework` (CocoaPods dynamic) vào bundle → duplicate protocol descriptors với SPM ObjectMapper (static).

**How to apply:** Không migrate bất kỳ package nào trong nhóm này cho đến khi cả 2 SDK được remove hoặc rebuild không embed Rx.

---

## 🔴 Blocked — Chờ Firebase migrate sang SPM

| Package | Lý do |
|---|---|
| GoogleSignIn | Firebase (CocoaPods) depends on AppAuth → CocoaPods AppAuth conflict với SPM AppAuth của GoogleSignIn → `undefined symbols: _OBJC_CLASS_$_OIDAuthState` khi link x86_64 |

**Why:** Firebase suite (CocoaPods) kéo `AppAuth` vào workspace dưới dạng CocoaPods framework. GoogleSignIn SPM cũng cần AppAuth qua SPM. Trong hybrid workspace, 2 bản AppAuth conflict → linker không tìm được OID* symbols cho GoogleSignIn.framework.

**How to apply:** Không migrate GoogleSignIn cho đến khi Firebase được migrate sang SPM.

---

## ⛔ Không migrate — Giữ CocoaPods vĩnh viễn

| Package | Lý do |
|---|---|
| CarousellChatSDK | Private pre-built SDK, link RxSwift CocoaPods ABI |
| OnFlowSDK | Private pre-built SDK |
| SwiftAlgorithms | `_NumericsShims` C module fail workspace-wide validation trên tất cả CocoaPods targets |
| FSPagerView | `Package.swift` tại tag 0.8.3 là file rỗng |
| BPLive / RangersAppLog / TTNetworkManager / libwebp | BytePlus binary SDK |
| ZaloSDK | VNG private, không có SPM |
| TikTokOpenSDKCore / TikTokOpenAuthSDK / TikTokOpenShareSDK | Binary SDK |
| MBProgressHUD | ObjC legacy (`jdg/MBProgressHUD`). Repo dormant từ 2019 (v1.2.0), không có `Package.swift` trên bất kỳ tag nào. Muốn migrate phải tự fork + viết `Package.swift` cho ObjC target → không worth it |
| GSKStretchyHeaderView | ObjC legacy |
| TTTAttributedLabel | ObjC legacy — no `Package.swift` on GitHub |
| HMSegmentedControl | `Package.swift` chỉ tồn tại trên `master` (không có trên tag nào). Thử `.branch("master")` — package xuất hiện trong list nhưng disabled, kéo toàn bộ packages bị disable. Suspect: `publicHeadersPath: ""` (empty string) trong Package.swift của repo gây conflict trong hybrid CocoaPods+SPM workspace |
| TTGTagCollectionView | ObjC legacy |
| RMStepsController | ObjC legacy |

---

## Root cause notes cho blocked packages

### SKPhotoBrowser
`SKPhotoBrowserObjC` là Clang module riêng. CocoaPods inject modulemap vào toàn workspace. `SwiftVerifyEmittedModuleInterface` trên targets có `BUILD_LIBRARY_FOR_DISTRIBUTION = YES` (CTLiveStream) fail vì SPM-built ObjC module không resolve được trong hybrid setup. Mọi workaround (private adapter, `@_implementationOnly`, remove delegate từ public API) đều thất bại.

### RecaptchaEnterprise
Binary XCFramework của Google, chỉ có arm64 simulator slice. SPM link được nhưng fail ở linker với `_OBJC_CLASS_$_Recaptcha` / `_OBJC_CLASS_$_RecaptchaAction` trên x86_64. CocoaPods workaround qua `EXCLUDED_ARCHS[sdk=iphonesimulator*] = x86_64`; SPM không có cơ chế tương đương.

### IGListKit
`CTComponent` expose `CTListDataSource` (open class: `ListAdapterDataSource`) và `CTListSectionController` (open class: `ListBindingSectionController<ListDiffable>`) là public API — không thể dùng private proxy pattern vì đây chính là adapter. Compiler force-generate `@import IGListKit` vào `CTComponent-Swift.h` → `SwiftVerifyEmittedModuleInterface` fail trên CTLiveStream dù đã link IGListKit vào target.

### HMSegmentedControl

**Problem:** Thêm HMSegmentedControl vào SPM khiến toàn bộ packages trong Xcode bị disabled (không chỉ HMSegmentedControl).

**Root cause:** `Package.swift` của repo chỉ tồn tại trên branch `master` — không có trên bất kỳ release tag nào (v1.5.6, v1.5.5, ...). Author thêm SPM support vào `master` năm 2020 nhưng không tạo release tag mới sau đó. Khi dùng `.upToNextMajor(from: "1.5.6")`, SPM tìm đến tag `v1.5.6` và không thấy `Package.swift` → resolution fail toàn workspace.

Đã thử `.branch("master")` để bypass vấn đề tag:
- Package xuất hiện đúng trong Xcode Package Dependencies list
- `pbxproj` được viết đúng format `kind = branch; branch = master`
- Nhưng package vẫn ở trạng thái disabled và kéo toàn bộ packages bị disabled

Suspect thêm: `Package.swift` của repo dùng `publicHeadersPath: ""` (empty string) cho ObjC target — format bất thường, khác với các ObjC package hoạt động tốt (GCDWebServer dùng `publicHeadersPath: "include"`). Empty string có thể gây ra ObjC module không hợp lệ trong hybrid CocoaPods+SPM workspace.

```
// HMSegmentedControl Package.swift (suspect)
.target(
    name: "HMSegmentedControl",
    path: "HMSegmentedControl",
    publicHeadersPath: ""   // ← empty — GCDWebServer dùng "include" và hoạt động tốt
)
```

**CocoaPods path worked** vì CocoaPods build thành `HMSegmentedControl.framework` và inject workspace-wide, không expose ObjC module structure cho SPM resolver.

**Action:** Attempted full migration (Package.swift, wrapper, sync script, Podfile) với cả version tag lẫn branch. Cả hai đều fail workspace-wide. Fully reverted.

**Keep as:** `pod 'HMSegmentedControl'` in Podfile indefinitely unless one of the following is true:
- Upstream repo publish release tag sau khi đã có `Package.swift`
- Upstream sửa `publicHeadersPath: ""` thành explicit path (e.g. `"include"`)
- Project fully migrates away từ CocoaPods sang pure SPM

---

### ObjectMapper

**Crash message:**
```
failed to demangle witness for associated type 'Output' in conformance
'ChoTot.CTHomeTargests.FetchHomePersonalAdTarget: Requestable'
from mangled name '…' — subject type x does not conform to protocol BaseMappable

An abort signal terminated the process. Such crashes often happen because of
an uncaught exception or unrecoverable error or calling the abort() function.
```

**Problem:** Sau khi migrate ObjectMapper sang SPM, app crash ngay khi runtime khởi động — trước cả khi `main()` return.

**Root cause: 3 bản sao ObjectMapper trong cùng 1 process**

```
CarousellChatSDK.framework  →  pod 'ObjectMapper' 4.2.0
                                └─ ObjectMapper.framework (dynamic, CocoaPods)  ← copy 1

ChoTot binary               →  SPM ObjectMapper (static)                        ← copy 2
                                  └─ linked trực tiếp vào ChoTot binary

CTApiClient.framework       →  SPM ObjectMapper (static)                        ← copy 3
                                  └─ linked trực tiếp vào CTApiClient.framework
```

Mỗi copy static tạo ra một `BaseMappable` protocol descriptor riêng trong memory. Swift runtime dùng pointer identity để kiểm tra protocol conformance — ba descriptor khác nhau → `CTHomePersonalAd: Mappable` (từ copy 2 hoặc 3) không match với `BaseMappable` constraint trong `ResponseEntity<T: BaseMappable>` (từ copy khác) → `abort()`.

**Tại sao `ObjectMapperPackage` dynamic wrapper không cứu được?**

Wrapper `.dynamic` trong `Package.swift` chỉ gom copy SPM thành 1 dylib dùng chung. Nhưng `CarousellChatSDK` (prebuilt binary) đã hardcode dependency vào CocoaPods `ObjectMapper.framework` — không thể redirect nó sang SPM. Kết quả vẫn là 2 `BaseMappable` descriptors (CocoaPods + SPM).

**Chuỗi type liên quan:**

```
FetchHomePersonalAdTarget: Requestable          // ChoTot target
  typealias Output = ResponseEntity<CTHomePersonalAd>?

ResponseEntity<T: BaseMappable>                 // CTApiClient — BaseMappable từ copy A
CTHomePersonalAd: Mappable (→ BaseMappable)     // ChoTot — BaseMappable từ copy B

copy A ≠ copy B  →  Swift demangler crash khi resolve witness table
```

**Files reverted:**

| File | Thay đổi |
|---|---|
| `Podfile` | `parse` → `pod 'ObjectMapper'` (bỏ `spm_object_mapper`) |
| `Package.swift` | Xoá `ObjectMapperPackage` product, dependency, target |
| `bin/sync_spm_packages.rb` | Xoá registry entry + `'parse'` alias, thêm blocker comment |
| `ChoTot.xcodeproj` | Xoá `XCRemoteSwiftPackageReference "ObjectMapper"` + product dep |
| `CTApiClient.xcodeproj` | Xoá `XCRemoteSwiftPackageReference "ObjectMapper"` + product dep |

**Keep as:** `pod 'ObjectMapper'` vĩnh viễn cho đến khi `CarousellChatSDK` bị remove hoặc được rebuild mà không embed ObjectMapper.

---

### SwiftAlgorithms
`@_exported import Algorithms` trong swiftinterface → Xcode validate `_NumericsShims` (C module private của swift-numerics) trên mọi CocoaPods target → `missing required module '_NumericsShims'` workspace-wide.
