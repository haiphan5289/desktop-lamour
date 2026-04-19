---
name: ct-generate-usecase
description: Auto-generate and implement a UseCase across all 6 architecture layers (NetworkHelper → Targets → Services → Repositories → UseCases → ViewModels) by modifying only existing files. Accepts 13 required parameters + 1 auto-detected generic wrapper. Use when adding a new API-backed UseCase to any module.
---

# [CT] Auto-Generate UseCase — 6-Layer Implementation

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Auto-generate and wire a UseCase across all 6 architecture layers by modifying **only existing files**.

---

## Input Format

Provide only **6 required fields** + **1 reference UseCase** — all remaining class/file names are **auto-detected** from the reference:

```
USECASE_NAME:        <e.g. FetchOrderStatistics>
INPUT_PARAM:         <e.g. String | Int | CustomRequest>
ENDPOINT_PATH:       <e.g. "v1/orders/statistics">
HTTP_METHOD:         <get | post | put | delete>
OUTPUT_PARAM:        <e.g. OrderStatistics>
VIEWMODEL_CLASS:     <e.g. CRCheckoutPageViewModel>
REFERENCE_USECASE:   <e.g. CRStatusVideoAIUseCase>   ← existing UseCase in same module
```

> All remaining values (`TARGET_CLASS`, `SERVICE_CLASS`, `REPOSITORY_CLASS`, `USECASE_CLASS`, `MODEL_CLASS`, `ENDPOINT_CLASS`, `GENERIC_MODEL`, `REPO_PROPERTY_NAME`) are **auto-detected from REFERENCE_USECASE**.

---

## 🔍 Auto-Detection from REFERENCE_USECASE

When the user provides `REFERENCE_USECASE`, perform the following lookups **before generating any code**:

### Step A — Read the reference UseCase file

```
1. Find the file containing {REFERENCE_USECASE}: grep -r "{REFERENCE_USECASE}" --include="*.swift" -l
2. Read the file to extract:
   - Repository type: `let repository: XxxRepositoryType`
   - UseCase file path → derive USECASE_CLASS (filename without .swift)
   - Output type: `typealias Output = XxxModel?`

3. Find the Assembler that registers {REFERENCE_USECASE}:
   grep -r "{REFERENCE_USECASE}" --include="*.swift" -l → filter files with "Assembler" in name
   → Read that file to extract:
     - ASSEMBLER_CLASS = Assembler protocol name (e.g. CRUsecaseAssembler)
     - ASSEMBLER_FILE  = file path (e.g. Assembler/CRUsecaseAssembler.swift)
     - The resolve() pattern in use:
         func resolve() -> {REFERENCE_USECASE} {
             return {REFERENCE_USECASE}(repository: resolve())
         }
     → Use this pattern to generate resolve() for the new UseCase
```

### Step B — Trace back through Repository → Service → Target

```
4. Find the Repository file (implementing XxxRepositoryType):
   grep -r "XxxRepositoryType" --include="*.swift" -l
   → Read file → extract class name = REPOSITORY_CLASS
   → Read method body to find the service property call (self.service.xxx)
   → Extract Service class name = SERVICE_CLASS

5. Find the Service file:
   grep -r "XxxService" --include="*.swift" -l
   → Read file → find the Target struct being called (XxxTarget.YyyTarget)
   → Extract Target class name = TARGET_CLASS

6. Find NetworkHelper:
   grep -r "Api\." in the Target file → find the file containing extension Api
   → ENDPOINT_CLASS = that NetworkHelper filename
```

### Step C — Detect GENERIC_MODEL

```
7. Check the Output type of REFERENCE_USECASE:
   - If Output = CRModelCommon<T>?    → GENERIC_MODEL = CRModelCommon
   - If Output = BaseResponseModel<T>? → GENERIC_MODEL = BaseResponseModel
   - If Output = DirectModel?          → GENERIC_MODEL = none (direct decode)
   - If Output is a plain struct        → GENERIC_MODEL = none
```

### Step D — Detect REPO_PROPERTY_NAME from ViewModel

```
8. Open {VIEWMODEL_CLASS}.swift
   grep "let.*[Rr]epo\|var.*[Rr]epo\|let.*[Rr]epository\|var.*[Rr]epository"
   → Find the property whose type matches REPOSITORY_CLASS (or the same repository type as the reference)
   → That is the REPO_PROPERTY_NAME
```

### Step E — Detect MODEL_CLASS

```
9. Find the file containing the struct for the Output type from the reference:
   grep -r "struct {OutputType}" --include="*.swift" -l
   → MODEL_CLASS = that filename (without .swift)
```

### Confirmation Table — Display before generating

After all lookups are complete, **show this confirmation table** and wait for user approval before generating:

```
📋 Auto-detected from REFERENCE_USECASE: {REFERENCE_USECASE}

ENDPOINT_CLASS:     CRNetworkHelper
TARGET_CLASS:       CRFreeBumpPfTargets
SERVICE_CLASS:      CRFreeBumpPfServices
REPOSITORY_CLASS:   CRUserDashBoardRepositories
USECASE_CLASS:      CRFreeBumpPfUseCase (file)
MODEL_CLASS:        ShareAdCrossPlatformModel (file)
GENERIC_MODEL:      none (direct decode)
REPO_PROPERTY_NAME: repository
USECASE_INIT_PARAM: repository:
ASSEMBLER_CLASS:    CRUsecaseAssembler
ASSEMBLER_FILE:     Assembler/CRUsecaseAssembler.swift

✅ Proceed with generation? (user may override any value above)
```

---

## 📋 Module Repository Reference

Each module uses different property names and repository types. Always open the target ViewModel file to confirm the exact property name before generating Step 6.

### CTCorePayment — Repository Property Names

| ViewModel Class | Repo Property | Repository Type |
|---|---|---|
| `CRAIShareAdCrossViewModel` | `repository` | `CRUserDashBoardRepositoriesType` |
| `CRFreeBumpPFViewModel` | `repository` | `CRUserDashBoardRepositoriesType` |
| `CRCheckoutPageViewModel` | `checkoutRepo` | `CRCheckoutCartRepositoryType` |
| `CROtherPaymentViewModel` | `checkoutRepo` | `CRCheckoutCartRepositoryType` |
| `CRRepaymentCheckoutViewModel` | `checkoutRepo` | `CRCheckoutCartRepositoryType` |
| `CRCartCheckoutViewModel` | `checkoutRepo` | `CRCheckoutCartRepositoryType` |
| `CRSubscritionCheckoutViewModel` | `checkoutRepo` | `CRCheckoutCartRepositoryType` |
| `CRTopupOtherPaymentViewModel` | `checkoutRepo` | `CRCheckoutCartRepositoryType` |
| `CRHighValuePackageViewModel` | `dongtotRepo` | `CRDongtotBalanceRespositoryType` |
| `CRCartCheckoutViewModel` | `dongtotRepo` | `CRDongtotBalanceRespositoryType` |
| `CRCheckoutPageViewModel` | `propackageRepo` | `CRProPackageRespositoryType` |
| `CRDTBizTransferDTViewModel` | `transRepo` | `CRTransactionHistoryReposotiryType` |
| `CRDTBizHistoryViewModel` | `transRepo` | `CRTransactionHistoryReposotiryType` |
| `CRDTBizBudgetManagementViewModel` | `transRepo` | `CRTransactionHistoryReposotiryType` |
| `CRResultDTTransactionPAASViewModel` | `userRepository` | `CRUserDashBoardRepositoriesType` |
| `CRResultDTTransactionPAASViewModel` | `commonRepositoryType` | `CTUseCaseCommonRepositoryType` |
| `CRTopupDongtotViewModel` | `usecaseCommon` | `CTUseCaseCommonRepositoryType` |

> **UseCase init param name for CTCorePayment:**
> - Most UseCases: `repository:` e.g. `CRStatusVideoAIUseCase(repository: self.repository)`
> - Transaction UseCases: `transRepo:` e.g. `CRGetDTExpiredDateUseCase(transRepo: self.transRepo)`
> - ProPackage UseCases: `proPackageRespositoryType:` e.g. `SubscriptionCheckOutPageUseCase(proPackageRespositoryType: propackageRepo)`

### CTPos — Repository Property Names

| ViewModel / Navigator | Repo Property | Repository Type |
|---|---|---|
| `POSInternalNavigator` (injected) | `repository` | `POSRepositoryType` |
| POSUseCase init param | `posRepository:` | `POSRepositoryType` |

> **UseCase init param name for CTPos:** always `posRepository:` — e.g. `POSGetBundlesUseCase(posRepository: repository)`

### CTPrivateDashboard — Repository Property Names

| UseCase Class | Repo Property | Repository Type |
|---|---|---|
| `PDPartnerToolsUseCase` | `userAdDashboardRepo` | `PDUserAdRepositoryType` |
| `PDUserAdUseCase` | `userAdDashboardRepo` | `PDUserAdRepositoryType` |
| `PDAnnouncerUseCase` | `announcerRepo` | `PDAnnouncerRepositoryType` |
| `PDCartUseCase` | `cartRepo` | `PDCartRepositoryType` |

### CTInsertAd — Repository Property Names

| Context | Repo Property | Repository Type |
|---|---|---|
| MainForm / SubCategory | `preparePageRepository` | `IAPreparePageRepositoryType` |
| Assembler (most UseCases) | `resolve()` via Assembler | auto-resolved |
| Chart UseCases | `repo:` | feature-specific |

> **UseCase init param name for CTInsertAd:** usually `repository:` (most), sometimes `repo:` for chart/price UseCases.

### CTShop — Repository Property Names

| ViewModel Class | Repo Property | Repository Type |
|---|---|---|
| `ShopIntroViewModel` | `repository` | `ShopRepositoryType` |
| `ShopSelectionViewModel` | `repository` | `ShopRepositoryType` |
| `GDSShopFrontViewModel` | inline init | `GDSShopFrontRepository` |
| `VEHShopFrontViewModel` | inline init | `VEHShopFrontRepository` |

### CTReferral — Repository Property Names

| ViewModel Class | Repo Property | Repository Type |
|---|---|---|
| `RefereeViewModel` | `layoutRepository` | `RefereeLayoutRepositoryType` |
| `RefereeViewModel` | `referralRepository` | `ReferralRepositoryType` |
| `ReferrerViewModel` | `layoutRepository` | `ReferrerLayoutRepositoryType` |
| `ReferrerViewModel` | `referralRepository` | `ReferralRepositoryType` |
| `ReferralViewModel` | `campainRepository` | `CampainRepositoryType` |
| `GiftSelectionViewModel` | `giftRepository` | `GiftRepositoryType` |

### CTReward — Repository Property Names

| ViewModel Class | Repo Property | Repository Type |
|---|---|---|
| `RMVoucherManagementViewModel` | `repository` | `RMVoucherManagementRepositoryType` |

### CTVEH — Repository Property Names

| UseCase Class | Repo Property | Repository Type |
|---|---|---|
| `InstantSell*UseCase` | `repo` | `VEInstantSellRepositoryType` |
| `VEPriceSuggestion*UseCase` | `repo` | `VEPriceSuggestionRepositoryType` |

> **UseCase init param name for CTVEH:** uses `repo:` shorthand — e.g. `VEPriceSuggestionFetchCarPriceUseCase(repo: self.repo)`

### CTJOB — Repository Property Names

| Context | Repo Property | Repository Type |
|---|---|---|
| `JBModule` (injected) | `repository` | `JBRepositoryType` |
| JBUseCase init param | `repository:` | module-specific |

---

## 🔍 How to Find the Repo Property Name (Step-by-Step)

When the ViewModel is unfamiliar, follow this lookup:

```
1. Open {VIEWMODEL_CLASS}.swift
2. Search for: "let.*Repo\|var.*Repo\|let.*repository\|var.*repository"
3. Note the property name (e.g. checkoutRepo, transRepo, repository)
4. Search for an existing UseCase init in the same file:
   e.g. "UseCase(repository: self." or "UseCase(repo: self."
5. Use the same property name and init param label in Step 6
```

---

## Architecture Flow

```
┌─────────────────┐
│   ViewModel     │ ← Step 6: Call UseCase
├─────────────────┤
│    UseCase      │ ← Step 5: Business Logic
├─────────────────┤
│   Repository    │ ← Step 4: Data Access
├─────────────────┤
│    Service      │ ← Step 3: Network Layer
├─────────────────┤
│    Targets      │ ← Step 2: API Endpoint
├─────────────────┤
│ NetworkHelper   │ ← Step 1: API Constant
└─────────────────┘
```

---

## 🔒 Core Constraint

- ✅ **ONLY modify existing files** — insert code at the end of each file
- ✅ ADD methods/structs to existing classes/enums
- ❌ **NEVER create** new Swift files, MD files, or documentation
- ❌ **NEVER skip** any of the 6 steps

---

## Step 1 — Add Endpoint to `{ENDPOINT_CLASS}`

```swift
// ⚠️ Endpoint key MUST be lowercase
extension Api {
    static let {usecaseName} = "{ENDPOINT_PATH}"
}
```

> `{usecaseName}` = lowercase-first version of `{USECASE_NAME}` (e.g. `fetchOrderStatistics`)

---

## Step 2 — Add Target to `{TARGET_CLASS}`

```swift
// Add inside the existing {TARGET_CLASS} enum/class
struct {USECASE_NAME}Target: Requestable {
    typealias Output = {RESPONSE_MODEL}?

    var httpMethod: HTTPMethod { .{HTTP_METHOD} }
    var endpoint: String { Api.{usecaseName} }
    var parameterEncoding: ParameterEncoding { URLEncoding.default }

    let input: {INPUT_PARAM}

    var params: Parameters {
        // Mapping rules:
        // String/Int/Bool/Double → ["param_key": input]
        // Custom model           → input.toDictionary() or explicit field map
        // GET requests           → query params
        // POST requests          → body params
        return ["param_key": input] // ⚠️ Replace with actual key
    }

    func decode(data: Any) -> Output {
        guard let data = data as? [String: Any],
              let jsonData = try? JSONSerialization.data(withJSONObject: data),
              let result = try? JSONDecoder().decode({RESPONSE_MODEL}.self, from: jsonData) else {
            return nil
        }
        return result
    }
}
```

---

## Step 3 — Add Service Method to `{SERVICE_CLASS}`

Add to the **protocol** and the **implementation**:

```swift
// Protocol
protocol {SERVICE_CLASS}Type {
    // ... existing methods ...
    func {usecaseName}(input: {INPUT_PARAM}) -> Observable<{RESPONSE_MODEL}?>
}

// Implementation
extension {SERVICE_CLASS}: {SERVICE_CLASS}Type {
    func {usecaseName}(input: {INPUT_PARAM}) -> Observable<{RESPONSE_MODEL}?> {
        return {TARGET_CLASS}.{USECASE_NAME}Target(input: input)
            .execute()
            .observe(on: resultScheduler)
    }
}
```

---

## Step 4 — Add Repository Method to `{REPOSITORY_CLASS}`

Add to the **protocol** and the **implementation**:

```swift
// Protocol
protocol {REPOSITORY_CLASS}Type {
    // ... existing methods ...
    func {usecaseName}(input: {INPUT_PARAM}) -> Observable<{RESPONSE_MODEL}?>
}

// Implementation
extension {REPOSITORY_CLASS}: {REPOSITORY_CLASS}Type {
    func {usecaseName}(input: {INPUT_PARAM}) -> Observable<{RESPONSE_MODEL}?> {
        return service.{usecaseName}(input: input)
    }
}
```

---

## Step 5 — Add UseCase to `{USECASE_CLASS}.swift`

```swift
// Add at the end of {USECASE_CLASS}.swift
final class CR{USECASE_NAME}UseCase: CTActionUseCaseType {
    typealias Input = {INPUT_PARAM}
    typealias Output = {RESPONSE_MODEL}?

    let repository: {REPOSITORY_CLASS}Type
    var action: Action<Input, Output>?

    init(repository: {REPOSITORY_CLASS}Type) {
        self.repository = repository
        self.action = initAction()
    }

    private func initAction() -> Action<Input, Output> {
        Action<Input, Output> { [unowned self] input in
            self.repository.{usecaseName}(input: input)
        }
    }
}
```

---

## Step 6 — Add Execution Method to `{VIEWMODEL_CLASS}`

```swift
// Add as an extension at the end of {VIEWMODEL_CLASS} file
extension {VIEWMODEL_CLASS} {
    func execute{USECASE_NAME}(input: {INPUT_PARAM}) {
        // 🔍 {REPO_PROPERTY_NAME} = auto-detected repository property name
        let useCase = CR{USECASE_NAME}UseCase(repository: self.{REPO_PROPERTY_NAME})

        // ✅ Success — always observe on MainScheduler before UI updates
        useCase.action?.elements
            .observe(on: MainScheduler.instance)
            .subscribe(onNext: { [weak self] result in
                guard let self, let result else { return }
                // TODO: bind result to presenter relay
                // self.presenter?.{dataRelay}.accept(result)
            })
            .disposed(by: disposeBag)

        // ✅ Loading
        useCase.action?.executing
            .observe(on: MainScheduler.instance)
            .subscribe(onNext: { [weak self] isLoading in
                self?.presenter?.loading.accept(isLoading)
            })
            .disposed(by: disposeBag)

        // ✅ Error — NO observe(on:) for underlyingError
        useCase.action?.underlyingError
            .subscribe(onNext: { [weak self] error in
                guard let self else { return }
                Logger.print("execute{USECASE_NAME} error: \(String(describing: error))")
            })
            .disposed(by: disposeBag)

        // ✅ Trigger
        useCase.action?.execute(input)
    }
}
```

### Critical Rules for Step 6

| Rule | Correct | Wrong |
|---|---|---|
| `elements` scheduler | `.observe(on: MainScheduler.instance)` ✅ | missing observe ❌ |
| `underlyingError` scheduler | no `.observe(on:)` ✅ | adding observe ❌ |
| subscription operator | `.subscribe(onNext:)` ✅ | `.bind(onNext:)` ❌ |
| retain cycle | `[weak self]` ✅ | `[unowned self]` in closures ❌ |
| disposal | `.disposed(by: disposeBag)` ✅ | missing disposal ❌ |
| logging | `Logger.print(...)` ✅ | `print(...)` ❌ |

---

## Step 7 — Register UseCase in `{ASSEMBLER_FILE}`

Add to **both the protocol and the default extension** of the Assembler file (auto-detected from REFERENCE_USECASE):

```swift
// Protocol — add to protocol {ASSEMBLER_CLASS}
protocol {ASSEMBLER_CLASS} {
    // ... existing resolves ...
    func resolve() -> CR{USECASE_NAME}UseCase
}

// Default extension — add to extension {ASSEMBLER_CLASS} where Self: CRDefaultAssembler
extension {ASSEMBLER_CLASS} where Self: CRDefaultAssembler {
    // ... existing resolves ...
    func resolve() -> CR{USECASE_NAME}UseCase {
        return CR{USECASE_NAME}UseCase(repository: resolve())
    }
}
```

> The `resolve()` pattern is **copied from REFERENCE_USECASE** — ensuring the init param label (`repository:`, `repo:`, `posRepository:`, etc.) matches the new UseCase.

---

## Step 8 — Add Response Model to `{MODEL_CLASS}.swift`

Add at the **bottom** of the existing `{MODEL_CLASS}.swift` file:

```swift
// MARK: - {USECASE_NAME} Response Model

public struct {OUTPUT_PARAM}: Codable {
    // Define properties matching the API JSON response
    // snake_case JSON keys → camelCase Swift properties via CodingKeys

    // Example:
    // public let totalOrders: Int?
    // public let pendingOrders: Int?

    enum CodingKeys: String, CodingKey {
        // case totalOrders = "total_orders"
        // case pendingOrders = "pending_orders"
    }
}

// Generic wrapper — auto-detected by module:
typealias {RESPONSE_MODEL} = {GENERIC_MODEL}<{OUTPUT_PARAM}>
// For CTPos (no wrapper): typealias {RESPONSE_MODEL} = {OUTPUT_PARAM}
```

### CodingKeys Conversion Rules

| JSON | Swift |
|---|---|
| `"total_negative": 0` | `totalNegative: Int?` + `case totalNegative = "total_negative"` |
| `"user_profile": {...}` | `userProfile: UserProfile?` (nested struct) |
| `"items": [...]` | `items: [Item]?` |
| Optional field | Use `Type?` |

---

## Parameter Mapping Reference

| Input Type | Parameter Pattern |
|---|---|
| `String` | `["param_key": input]` |
| `Int` | `["param_key": input]` |
| `Bool` | `["param_key": input]` |
| `Double` | `["param_key": input]` |
| Custom model | `input.toDictionary()` or explicit field map |
| `[String]` | `["param_key": input]` |

---

## Common Repository Property Names

| ViewModel Class | Repository Property |
|---|---|
| `CRCheckoutPageViewModel` | `checkoutRepo` |
| `CRTopupDongtotViewModel` | `dongtotRespository` |
| `POSViewModel` | `posRepo` |
| `VEHViewModel` | `vehRepo` |

---

## Example — FetchOrderStatistics

```
USECASE_NAME:     FetchOrderStatistics
INPUT_PARAM:      String
OUTPUT_PARAM:     OrderStatistics
ENDPOINT_PATH:    "v1/orders/statistics"
HTTP_METHOD:      get
RESPONSE_MODEL:   OrderStatisticsResponseModel
ENDPOINT_CLASS:   CRNetworkHelper
TARGET_CLASS:     CRCheckoutTargets
SERVICE_CLASS:    CRCheckoutService
REPOSITORY_CLASS: CRCheckoutCartRepository
USECASE_CLASS:    CRCheckoutUseCase
MODEL_CLASS:      CRCheckOutModel
VIEWMODEL_CLASS:  CRCheckoutPageViewModel
GENERIC_MODEL:    CRModelCommon  ← auto-detected (CTCorePayment module)
REPO_PROPERTY:    checkoutRepo   ← auto-detected from ViewModel file
```

```json
// Expected API response:
{
    "data": {
        "total_orders": 150,
        "pending_orders": 25,
        "completed_orders": 120,
        "total_revenue": 50000000
    },
    "success": true
}
```

---

## Step 9 — Update Presentable Protocol + ViewController (if needed)

When the UseCase output needs to be **rendered on the UI**, also update:

### 9a — Presentable Protocol (in the `{VIEWMODEL_CLASS}` file or a dedicated Presentable file)
```swift
protocol {FEATURE}Presentable: AnyObject {
    // ... existing relays ...
    var {outputRelay}: BehaviorRelay<{OUTPUT_PARAM}?> { get }
    // Use PublishRelay instead if this is a one-shot event with no initial value
}
```

### 9b — ViewController (declare relay + bind in `configurePresenter()`)
```swift
final class {FEATURE}ViewController: UIViewController, {FEATURE}Presentable {
    // Declare relay
    let {outputRelay}: BehaviorRelay<{OUTPUT_PARAM}?> = .init(value: nil)

    private func configurePresenter() {
        // Bind relay → UI update
        {outputRelay}
            .observe(on: MainScheduler.instance)
            .subscribe(onNext: { [weak self] model in
                guard let self, let model else { return }
                // TODO: update UI with model
            })
            .disposed(by: disposeBag)
    }
}
```

> ⚠️ **When Step 9 is required:** When the ViewModel calls `self?.presenter?.{relay}.accept(result)` inside `execute{USECASE_NAME}` — meaning the result must be pushed to the ViewController for rendering.  
> **Not needed** if the ViewModel only uses the result internally (routing, side effects).

---

## Step 10 — Update Test Mock (if a Mock Repository exists)

When a new method is added to the Repository protocol, **all Mock classes** implementing that protocol must be updated:

```
1. Find the Mock file:
   grep -r "Mock.*{REPOSITORY_CLASS}\|{REPOSITORY_CLASS}.*Mock" --include="*.swift" -l
   → typically located at: ChoTotTests/{ModuleName}/

2. Add the stub method to the Mock class:
```

```swift
// Add to Mock{REPOSITORY_CLASS}.swift
var stubbed{USECASE_NAME}Result: Observable<{RESPONSE_MODEL}?>!

func {usecaseName}(input: {INPUT_PARAM}) -> Observable<{RESPONSE_MODEL}?> {
    return stubbed{USECASE_NAME}Result
}
```

> ⚠️ **Critical:** Skipping this step will cause a **compile error** because the Mock no longer conforms to the Repository protocol.

---

## ✅ Completion Checklist

**Core 6 layers (mandatory):**
- [ ] `Api.{usecaseName}` added to `{ENDPOINT_CLASS}` (Step 1)
- [ ] `{USECASE_NAME}Target` added to `{TARGET_CLASS}` (Step 2)
- [ ] `{usecaseName}(input:)` added to `{SERVICE_CLASS}` (protocol + impl) (Step 3)
- [ ] `{usecaseName}(input:)` added to `{REPOSITORY_CLASS}` (protocol + impl) (Step 4)
- [ ] `CR{USECASE_NAME}UseCase` added to `{USECASE_CLASS}.swift` (Step 5)
- [ ] `execute{USECASE_NAME}(input:)` added to `{VIEWMODEL_CLASS}` (Step 6)
- [ ] `resolve() -> CR{USECASE_NAME}UseCase` added to `{ASSEMBLER_FILE}` (protocol + default extension) (Step 7)
- [ ] `{OUTPUT_PARAM}` struct + `{RESPONSE_MODEL}` typealias added to `{MODEL_CLASS}.swift` (Step 8)

**UI & Test (conditional — per use case):**
- [ ] Presentable protocol updated with new relay (Step 9a) — *only if result needs to be pushed to UI*
- [ ] ViewController declares relay + binds in `configurePresenter()` (Step 9b) — *only if result needs rendering*
- [ ] Mock Repository updated with stub method (Step 10) — *only if a Mock file exists for this repository*

**Quality:**
- [ ] No new Swift files created
- [ ] SwiftLint passes on all modified files

---

## 📚 Real-World Reference — `CRStatusVideoAIUseCase` (CTCorePayment)

This is a complete, production reference showing how all 6 layers look in the actual codebase.

### Parameters Used

| Parameter | Value |
|---|---|
| `USECASE_NAME` | `StatusVideoAI` |
| `INPUT_PARAM` | `String` (request_id) |
| `OUTPUT_PARAM` | `StatusVideoAIModel` |
| `ENDPOINT_PATH` | `"api-uni-rev/private/ad_social/video_status"` |
| `HTTP_METHOD` | `get` |
| `RESPONSE_MODEL` | `StatusVideoAIModel` *(no generic wrapper — direct model)* |
| `ENDPOINT_CLASS` | `CRNetworkHelper` |
| `TARGET_CLASS` | `CRFreeBumpPfTargets` |
| `SERVICE_CLASS` | `CRFreeBumpPfServices` |
| `REPOSITORY_CLASS` | `CRUserDashBoardRepositories` |
| `USECASE_CLASS` | `CRFreeBumpPfUseCase.swift` |
| `MODEL_CLASS` | `ShareAdCrossPlatformModel.swift` (in `Libraries/CTUseCaseCommon`) |
| `VIEWMODEL_CLASS` | `CRAIShareAdCrossViewModel` |
| `REPO_PROPERTY_NAME` | `repository` *(auto-detected from ViewModel)* |

### Impact Map — Files Modified Across the Project

```
CTCorePayment module
│
├── NetworkHelper/
│   └── CRNetworkHelper.swift
│       └── Api.statusVideoAI = "api-uni-rev/private/ad_social/video_status"
│
├── Data/Services/UserDashBoard/FreeBumpPremiumService/
│   ├── CRFreeBumpPfTargets.swift
│   │   └── struct StatusVideoAITarget: Requestable { ... }
│   └── CRFreeBumpPfServices.swift
│       ├── protocol CRFreeBumpPfServicesType { func statusVideoAI(...) }
│       └── extension CRFreeBumpPfServices { func statusVideoAI(...) }
│
├── Data/Repositories/UserDashBoard/
│   └── CRUserDashBoardRepositories.swift
│       ├── protocol CRUserDashBoardRepositoriesType { func statusVideoAI(...) }
│       └── extension CRUserDashBoardRepositories { func statusVideoAI(...) }
│
├── Domain/UseCases/UserDashBoard/
│   └── CRFreeBumpPfUseCase.swift
│       └── final class CRStatusVideoAIUseCase: CTActionUseCaseType { ... }
│
├── Features/UserDashBoard/AIShareAdCross/
│   ├── CRAIShareAdCrossViewModel.swift         ← Step 6: executeStatusVideoAI()
│   │   └── func executeStatusVideoAI(input: String) { ... }
│   ├── CRAIShareAdCrossViewController.swift    ← Step 9: relay + binding
│   │   ├── let statusVideoTrigger: BehaviorRelay<StatusVideoAIModel?> = .init(value: nil)
│   │   └── configurePresenter() binds statusVideoTrigger → UI update
│   └── Cell/AdVideoAI/
│       └── CRAdShareVideoAICell.swift          ← Step 9 (downstream consumer)
│           ├── func setupStatusView(status: StatusVideoAIModel.StatusType)
│           └── func setupStepStackView(model: StatusVideoAIModel)
│
└── Assembler/
    └── CRUsecaseAssembler.swift                ← Step 7: resolve()
        ├── protocol CRUsecaseAssembler { func resolve() -> CRStatusVideoAIUseCase }
        └── extension { func resolve() -> CRStatusVideoAIUseCase { ... } }

Libraries/CTUseCaseCommon (shared model — cross-module)
└── CTUseCaseCommon/Model/ShareAdCrossPlatformModel.swift  ← Step 8: model struct
    └── public struct StatusVideoAIModel: Codable { ... }

ChoTotTests/CTCorePayment (test mock — MUST update when protocol changes)
└── Data/Repositories/UserDashboard/MockCRUserDashBoard.swift  ← Step 10: mock stub
    ├── var stubbedStatusVideoAIResult: Observable<StatusVideoAIModel?>!
    └── func statusVideoAI(input: String) -> Observable<StatusVideoAIModel?>
```

### Layer-by-Layer Reference Code

**Step 1 — NetworkHelper** ([CRNetworkHelper.swift](../../../AppFeatures/CTCorePayment/CTCorePayment/NetworkHelper/CRNetworkHelper.swift))
```swift
extension Api {
    static let statusVideoAI = "api-uni-rev/private/ad_social/video_status"
}
```

**Step 2 — Target** ([CRFreeBumpPfTargets.swift](../../../AppFeatures/CTCorePayment/CTCorePayment/Data/Services/UserDashBoard/FreeBumpPremiumService/CRFreeBumpPfTargets.swift))
```swift
struct StatusVideoAITarget: Requestable {
    typealias Output = StatusVideoAIModel?

    var httpMethod: HTTPMethod { return .get }
    var endpoint: String { return Api.statusVideoAI }
    var parameterEncoding: ParameterEncoding { return URLEncoding.default }

    let input: String

    var params: Parameters {
        return ["request_id": input]
    }

    func decode(data: Any) -> Output {
        guard let data = data as? [String: Any],
              let jsonData = try? JSONSerialization.data(withJSONObject: data, options: []),
              let result = try? JSONDecoder().decode(StatusVideoAIModel.self, from: jsonData) else {
            return nil
        }
        return result
    }
}
```

**Step 3 — Service** ([CRFreeBumpPfServices.swift](../../../AppFeatures/CTCorePayment/CTCorePayment/Data/Services/UserDashBoard/FreeBumpPremiumService/CRFreeBumpPfServices.swift))
```swift
// Protocol
protocol CRFreeBumpPfServicesType {
    func statusVideoAI(input: String) -> Observable<StatusVideoAIModel?>
}

// Implementation
extension CRFreeBumpPfServices: CRFreeBumpPfServicesType {
    func statusVideoAI(input: String) -> Observable<StatusVideoAIModel?> {
        return CRFreeBumpPfTargets.StatusVideoAITarget(input: input)
            .execute()
            .observe(on: resultScheduler)
    }
}
```

**Step 4 — Repository** ([CRUserDashBoardRepositories.swift](../../../AppFeatures/CTCorePayment/CTCorePayment/Data/Repositories/UserDashBoard/CRUserDashBoardRepositories.swift))
```swift
// Protocol
protocol CRUserDashBoardRepositoriesType {
    func statusVideoAI(input: String) -> Observable<StatusVideoAIModel?>
}

// Implementation
extension CRUserDashBoardRepositories: CRUserDashBoardRepositoriesType {
    func statusVideoAI(input: String) -> Observable<StatusVideoAIModel?> {
        return service.statusVideoAI(input: input)
    }
}
```

**Step 5 — UseCase** ([CRFreeBumpPfUseCase.swift](../../../AppFeatures/CTCorePayment/CTCorePayment/Domain/UseCases/UserDashBoard/CRFreeBumpPfUseCase.swift))
```swift
final class CRStatusVideoAIUseCase: CTActionUseCaseType {
    typealias Output = StatusVideoAIModel?
    typealias Input = String

    let repository: CRUserDashBoardRepositoriesType
    var action: Action<Input, Output>?

    init(repository: CRUserDashBoardRepositoriesType) {
        self.repository = repository
        self.action = initAction()
    }

    private func initAction() -> Action<Input, Output> {
        Action<Input, Output> { [unowned self] input in
            self.repository.statusVideoAI(input: input)
        }
    }
}
```

**Step 6 — ViewModel** ([CRAIShareAdCrossViewModel.swift](../../../AppFeatures/CTCorePayment/CTCorePayment/Features/UserDashBoard/AIShareAdCross/CRAIShareAdCrossViewModel.swift))
```swift
extension CRAIShareAdCrossViewModel {
    func executeStatusVideoAI(input: String) {
        let useCase = CRStatusVideoAIUseCase(repository: self.repository)

        useCase.action?.elements
            .bind(onNext: { [weak self] result in
                guard let self = self,
                      let statusVideo = result else { return }
                if self.presenter?.statusVideoTrigger.value?.status != statusVideo.status {
                    self.presenter?.statusVideoTrigger.accept(statusVideo)
                }
                // Additional business logic: timer management
                if !self.firstLoad || statusVideo.status == .inProgress || self.remainingSeconds == nil {
                    let defaultSecond = self.remainingSeconds ?? 10
                    self.autoTimer(remainSecond: defaultSecond)
                }
                self.remainingSeconds = statusVideo.remainingSeconds
                self.firstLoad = false
                if statusVideo.status == .completed {
                    self.autoGenerateVideo?.dispose()
                }
            })
            .disposed(by: disposeBag)

        useCase.action?.underlyingError
            .bind(onNext: { [weak self] _ in
                guard let self = self else { return }
                self.presenter?.generateVideoErrorTrigger.onNext(())
            })
            .disposed(by: disposeBag)

        useCase.action?.execute(input)
    }
}
```

**Assembler** ([CRUsecaseAssembler.swift](../../../AppFeatures/CTCorePayment/CTCorePayment/Assembler/CRUsecaseAssembler.swift))
```swift
// Protocol
protocol CRUsecaseAssembler {
    func resolve() -> CRStatusVideoAIUseCase
}

// Default implementation
extension CRUsecaseAssembler where Self: CRDefaultAssembler {
    func resolve() -> CRStatusVideoAIUseCase {
        return CRStatusVideoAIUseCase(repository: resolve())
    }
}
```

**Model** ([ShareAdCrossPlatformModel.swift](../../../Libraries/CTUseCaseCommon/CTUseCaseCommon/Model/ShareAdCrossPlatformModel.swift))
```swift
// Located in Libraries/CTUseCaseCommon — shared across modules
public struct StatusVideoAIModel: Codable {
    public enum StatusType: String, CaseIterable {
        case completed
        case inProgress = "in_progress"
        case pending
        case unknown
    }

    public let status: StatusType
    public let totalStep: Int?
    public let stepDescription: String?
    public let step: Int?
    public let waitingIconUrl: String?
    public let aiVideoDownloadUrl: String?
    public let aiVideoSize: Double?
    public let aiVideoStreamingUrl: String?
    public let remainingSeconds: Int?

    enum CodingKeys: String, CodingKey {
        case status
        case totalStep = "total_step"
        case stepDescription = "step_description"
        case step
        case waitingIconUrl = "waiting_icon_url"
        case aiVideoDownloadUrl = "ai_video_download_url"
        case aiVideoSize = "ai_video_byte_size"
        case aiVideoStreamingUrl = "ai_video_streaming_url"
        case remainingSeconds = "remaining_seconds"
    }
}
```

### Key Observations from This Reference

1. **No generic wrapper** — `StatusVideoAIModel` is decoded directly (no `CRModelCommon<T>`). The Target decodes it directly from the root JSON object.
2. **Model in shared library** — `StatusVideoAIModel` lives in `Libraries/CTUseCaseCommon`, not inside CTCorePayment. Use this when the model needs to be shared across modules.
3. **Assembler registration is mandatory** — every new UseCase must be registered in `CRUsecaseAssembler` (both protocol + default extension).
4. **ViewModel business logic** — `executeStatusVideoAI` contains real business logic (timer management, status comparison). The elements closure is not always a simple relay accept.
5. **`bind(onNext:)` used in production** — this ViewModel uses `.bind(onNext:)` for elements. Both `.bind(onNext:)` and `.subscribe(onNext:)` are valid; prefer `.subscribe(onNext:)` with `observe(on: MainScheduler.instance)` for UI updates.
