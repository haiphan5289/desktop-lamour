---
description: Auto Generate and implement a UseCase through the layers: Targets, Services, Repositories, UseCase, ViewModel
mode: agent
model: gpt-4o
parameters:
  - name: usecaseName
    description: The name of the UseCase to generate (e.g., FetchOrderStatistics)
    required: true
  - name: outputParam
    description: The output parameter type (e.g., OrderStatistics)
    required: true
  - name: endpointPath
    description: The API endpoint path (e.g., "v1/orders/statistics")
    required: true
  - name: httpMethod
    description: The HTTP method (e.g., get)
    required: true
  - name: inputParam
    description: The input parameter type (e.g., Int, order_id)
    required: true
  - name: responseModel
    description: The response model name (e.g., OrderStatisticsResponseModel)
    required: true
  - name: endpointClass
    description: The endpoint class name (e.g., CRNetworkHelper)
    required: true
  - name: targetClass
    description: The target class name (e.g., CRCheckoutTargets)
    required: true
  - name: serviceClass
    description: The service class name (e.g., CRCheckoutService)
    required: true
  - name: repositoryClass
    description: The repository class name (e.g., CRCheckoutCartRepository)
    required: true
  - name: useCaseClass
    description: The usecase class name (e.g., CRCheckoutUseCase)
    required: true
  - name: modelClass
    description: The model class name (e.g., CRCheckOutModel)
    required: true
  - name: viewModelClass
    description: The viewmodel class name (e.g., CRCheckoutPageViewModel)
    required: true
  - name: genericModel
    description: The generic wrapper model class (auto-detect or use default)
    default: "CRModelCommon"
---

# [AI] Auto-generate a UseCase through the layers: Targets, Services, Repositories, UseCase, ViewModel

**Module:** CorePayment  
**UseCase:** {USECASE_NAME}  
**Input:** {INPUT_PARAM}  
**Output:** {OUTPUT_PARAM}  
**Endpoint:** {ENDPOINT_PATH}  
**Method:** {HTTP_METHOD}  
**Response Model:** {RESPONSE_MODEL}  
**Endpoint Class:** {ENDPOINT_CLASS}  
**Target Class:** {TARGET_CLASS}  
**Service Class:** {SERVICE_CLASS}  
**Repository Class:** {REPOSITORY_CLASS}  
**UseCase Class:** {USECASE_CLASS}  
**Model Class:** {MODEL_CLASS}  
**ViewModel Class:** {VIEWMODEL_CLASS}  
**Generic Model:** {GENERIC_MODEL}  

---
## 1. Quick Setup

**🎯 Replace these core parameters (13 required + 1 auto-detected):**

| Parameter | Description | Example |
|-----------|-------------|---------|
| `{USECASE_NAME}` | UseCase identifier | FetchOrderStatistics |
| `{INPUT_PARAM}` | Input parameter type | String, Int, CustomModel |
| `{OUTPUT_PARAM}` | Output data type | OrderStatistics |
| `{ENDPOINT_PATH}` | API endpoint path | "v1/orders/statistics" |
| `{HTTP_METHOD}` | HTTP method | get, post, put, delete |
| `{RESPONSE_MODEL}` | Response model name | OrderStatisticsResponseModel |
| `{ENDPOINT_CLASS}` | Endpoint class | CRNetworkHelper |
| `{TARGET_CLASS}` | Target class | CRCheckoutTargets |
| `{SERVICE_CLASS}` | Service class | CRCheckoutService |
| `{REPOSITORY_CLASS}` | Repository class | CRCheckoutCartRepository |
| `{USECASE_CLASS}` | UseCase class | CRCheckoutUseCase |
| `{MODEL_CLASS}` | Model class | CRCheckOutModel |
| `{VIEWMODEL_CLASS}` | ViewModel class | CRCheckoutPageViewModel |
| `{GENERIC_MODEL}` | Generic wrapper (auto-detect) | **Auto:** CRModelCommon, BaseResponseModel, None |

### 🔒 **Core Rule: Only Modify Existing Files**
- ✅ **Auto-insert** code into 6 layers through existing project files
- ✅ **ADD methods** to existing ViewModels / UseCases / Services
- ❌ **NEVER create** new files (Swift, MD, examples, docs)

### 🔍 **Auto-Detection Steps:**
1. **Open `{VIEWMODEL_CLASS}` file**
2. **Find repository property name** (e.g., `checkoutRepo`, `dongtotRespository`, `posRepo`)
3. **Replace `{REPO_PROPERTY_NAME}`** with actual property name in Step 6
4. **Auto-detect `{GENERIC_MODEL}`** based on module:
   - **CorePayment modules**: `CRModelCommon`
   - **VEH modules**: `BaseResponseModel`  
   - **POS modules**: Skip wrapper (direct model)
   - **Other modules**: Check existing patterns or use `CRModelCommon`

**📋 Files Modified:** NetworkHelper → Targets → Services → Repositories → UseCases → ViewModels

---

## 2. Architecture Overview

```
┌─────────────────┐
│   ViewModel     │ ← 6. Call UseCase
├─────────────────┤
│    UseCase      │ ← 5. Business Logic
├─────────────────┤
│   Repository    │ ← 4. Data Access Layer
├─────────────────┤
│    Service      │ ← 3. Network Layer
├─────────────────┤
│    Targets      │ ← 2. API Endpoints
├─────────────────┤
│ NetworkHelper   │ ← 1. API Constants
└─────────────────┘
```

---

## 3. Implementation Templates

<details>
<summary>📋 <strong>Step-by-Step Code Templates</strong> (Click to expand - All {PLACEHOLDER} dynamic)</summary>

### Step 1: Add Endpoint to {ENDPOINT_CLASS}
```swift
// ⚠️ NOTE: Endpoint name MUST be lowercase (e.g. fetchUserProfile, not FetchUserProfile)
extension Api {
    // Existing endpoints...
    static let {USECASE_NAME} = "{ENDPOINT_PATH}"
}
```

### Step 2: Add Target to {TARGET_CLASS}
```swift
enum {TARGET_CLASS} {
    struct {USECASE_NAME}Target: Requestable {
        typealias Output = {RESPONSE_MODEL}?
        
        var httpMethod: HTTPMethod { return .{HTTP_METHOD} }
        var endpoint: String { return Api.{USECASE_NAME} }
        var parameterEncoding: ParameterEncoding { return URLEncoding.default }
        
        let input: {INPUT_PARAM}

        var params: Parameters {
            // TODO: Customize parameters based on your INPUT_PARAM type
            // Examples:
            // - For String: ["user_id": input]
            // - For Int: ["order_id": input]
            // - For custom model: input.toDictionary() or manual mapping
            // - For GET requests: query parameters
            // - For POST requests: body parameters
            return nil // Replace with actual parameters
        }
        
        func decode(data: Any) -> Output {
            guard let data = data as? [String: Any],
                  let jsonData = try? JSONSerialization.data(withJSONObject: data, options: []),
                  let result = try? JSONDecoder().decode({RESPONSE_MODEL}.self, from: jsonData) else {
                return nil
            }
            return result
        }
    }
}
```
> 📝 **Note:** Add the new target at the end of the {TARGET_CLASS} class/enum.

### Step 3: Add Service Method
```swift
protocol {SERVICE_CLASS}Type {
    func {USECASE_NAME}(input: {INPUT_PARAM}) -> Observable<{RESPONSE_MODEL}?>
}

extension {SERVICE_CLASS}: {SERVICE_CLASS}Type {
    func {USECASE_NAME}(input: {INPUT_PARAM}) -> Observable<{RESPONSE_MODEL}?> {
        return {TARGET_CLASS}.{USECASE_NAME}Target(input: input)
            .execute()
            .observe(on: resultScheduler)
    }
}
```
> 📝 **Note:** Add the new service method at the end of the {SERVICE_CLASS} class.

### Step 4: Add Repository Method
```swift
protocol {REPOSITORY_CLASS}Type {
    func {USECASE_NAME}(input: {INPUT_PARAM}) -> Observable<{RESPONSE_MODEL}?>
}

extension {REPOSITORY_CLASS}: {REPOSITORY_CLASS}Type {
    func {USECASE_NAME}(input: {INPUT_PARAM}) -> Observable<{RESPONSE_MODEL}?> {
        return service.{USECASE_NAME}(input: input)
    }
}
```
> 📝 **Note:** Add the new repository method at the end of the {REPOSITORY_CLASS} class and {REPOSITORY_PROTOCOL} protocol.

### Step 5: Add UseCase to {USECASE_CLASS}.swift 
```swift
// Add to {USECASE_CLASS}.swift file
final class CR{USECASE_NAME}UseCase: CTActionUseCaseType {
    typealias Output = {RESPONSE_MODEL}?
    typealias Input = {INPUT_PARAM}
    
    let repository: {REPOSITORY_CLASS}Type
    var action: Action<Input, Output>?
    
    init(repository: {REPOSITORY_CLASS}Type) {
        self.repository = repository
        self.action = initAction()
    }
    
    private func initAction() -> Action<Input, Output> {
        Action<Input, Output> { [unowned self] input in
            self.repository.{USECASE_NAME}(input: input)
        }
    }
}
```
> 📝 **Note:** Add the new use case at the end of the {USECASE_CLASS}.swift class.

### Step 6: Add Method to {VIEWMODEL_CLASS} (Existing File)
```swift
// ⚠️ FORCE TEMPLATE: DO NOT ADD ANYTHING ELSE. ADD THIS METHOD TO EXISTING {VIEWMODEL_CLASS} CLASS ⚠️
// NOTE: Place this function at the end of the ViewModel class.
extension {VIEWMODEL_CLASS} {
    func execute{USECASE_NAME}(input: {INPUT_PARAM}) {
        // 🔍 FIND: Repository property name in {VIEWMODEL_CLASS}
        // Common names: checkoutRepo, dongtotRespository, posRepo, vehRepo
        let useCase = CR{USECASE_NAME}UseCase(repository: self.{REPO_PROPERTY_NAME})
        
        // 🔒 MANDATORY: Handle success - AUTO-GENERATED: No manual implementation needed
        useCase.action?.elements
            .bind(onNext: { [weak self] result in
                guard let self = self, let result = result else { return }
                // Success handling is auto-generated and complete
            })
            .disposed(by: disposeBag)
        
        // 🔒 MANDATORY: Handle loading
        useCase.action?.executing
            .bind(onNext: { [weak self] loading in
                self?.presenter?.loading.accept(loading)
            })
            .disposed(by: disposeBag)
        
        // 🔒 MANDATORY: Handle errors - AUTO-GENERATED: No manual implementation needed
        useCase.action?.underlyingError
            .bind(onNext: { [weak self] error in 
                // Error handling is auto-generated and complete
            })
            .disposed(by: disposeBag)
        
        // 🔒 MANDATORY: Execute
        useCase.action?.execute(input)
    }
}
```

</details>

---

## 4. Add Response Model to {MODEL_CLASS}

### 4.1 JSON Response Example
```json
{
    "data": {
        "total_negative": 0,
        "total_positive": 0
    },
    "success": true
}
```

### 4.2 Add to {MODEL_CLASS}.swift (Recommended)
Add your response model at the bottom of the existing `{MODEL_CLASS}.swift` file:

```swift
// MARK: - {USECASE_NAME} Response Models (Add at bottom of {MODEL_CLASS}.swift)

// Define the actual data model first
public struct {OUTPUT_PARAM}: Codable {
    // Define your data model properties here
    // Example for UserProfile:
    // public let id: String?
    // public let name: String?
    // public let email: String?
    
    // Example for Statistics (based on your response):
    // public let totalNegative: Int?
    // public let totalPositive: Int?
    
    enum CodingKeys: String, CodingKey {
        // Map your properties here with snake_case if needed
        // case id
        // case name
        // case email
        
        // Example for Statistics:
        // case totalNegative = "total_negative"
        // case totalPositive = "total_positive"
    }
}

// Use existing generic model (Recommended)
typealias {RESPONSE_MODEL} = {GENERIC_MODEL}<{OUTPUT_PARAM}>
```

### 4.3 Alternative: Create Separate File
If you prefer to create a separate file, create `{RESPONSE_MODEL}.swift`:

```swift
// {RESPONSE_MODEL}.swift
import Foundation

// Define the actual data model first
public struct {OUTPUT_PARAM}: Codable {
    // Your properties here
    enum CodingKeys: String, CodingKey {
        // Your coding keys
    }
}

// Use existing generic model
typealias {RESPONSE_MODEL} = {GENERIC_MODEL}<{OUTPUT_PARAM}>
```

### 4.4 Why {GENERIC_MODEL}?
✅ **Benefits:**
- **Consistency**: Reuse existing `{GENERIC_MODEL}<T>` pattern
- **Cleaner**: No duplicate response wrapper code
- **Maintainable**: Single source of truth for response structure
- **Generic**: Works with any data type

✅ **{GENERIC_MODEL} Structure:**
```swift
public struct {GENERIC_MODEL}<T: Codable>: Codable {
    let success: Bool?
    let data: T?
}
```
### 4.5 Conversion Rules
When you have a JSON response, follow these rules to convert to Codable:

1. **JSON Object** → `struct Model: Codable`
2. **JSON Array** → `[Model]` or `Array<Model>`
3. **snake_case** → Use `CodingKeys` to map to camelCase
4. **Optional fields** → Use optional properties (`String?`, `Int?`)
5. **Nested objects** → Create separate struct models

**Example Conversion:**
```
JSON: "total_negative": 0     → Swift: totalNegative: Int?
JSON: "user_profile": {...}   → Swift: userProfile: UserProfile?
JSON: "items": [...]          → Swift: items: [Item]?
```

### 4.6 Parameters Customization
You can customize the API parameters in the Target. **General Rule:** Convert your Input Parameters directly to API parameters:

```swift
var parameters: [String: Any]? {
    // ✅ GENERAL CONVERSION RULE:
    // Input Parameters: "paramName: Type" → ["paramName": input]
    
    // Examples:
    // orderId: String     → return ["orderId": input]
    // userId: Int         → return ["userId": input]  
    // productId: String   → return ["productId": input]
    // amount: Double      → return ["amount": input]
    // page: Int           → return ["page": input]
    // query: String       → return ["query": input]
    
    return ["paramName": input] // ⚠️ Replace "paramName" with your actual parameter name
}
```
**Conversion Examples:**
```
Input Parameters: userId: String        → ["userId": input]
Input Parameters: orderId: Int          → ["orderId": input]  
Input Parameters: email: String         → ["email": input]
Input Parameters: amount: Double        → ["amount": input]
Input Parameters: isActive: Bool        → ["isActive": input]
Input Parameters: categories: [String]  → ["categories": input]
```
---

## 5. GitHub Copilot Prompt

```
Generate a CorePayment UseCase with these parameters:
- UseCase: {USECASE_NAME}
- Output: {OUTPUT_PARAM}
- Endpoint: {ENDPOINT_PATH}
- Method: {HTTP_METHOD}
- Response Model: {RESPONSE_MODEL}
- Endpoint Class: {ENDPOINT_CLASS}
- Target Class: {TARGET_CLASS}
- Service Class: {SERVICE_CLASS}
- UseCase Class: {USECASE_CLASS}
- Model Class: {MODEL_CLASS}
- ViewModel Class: {VIEWMODEL_CLASS}
- Generic Model: {GENERIC_MODEL}

Input Parameters:
- key: value (customize your API parameters here)

Follow the 6-layer architecture pattern in the template.
```

---

## 5. Usage Examples & Patterns

### **Parameter Mapping Patterns:**
| Input Type | {USECASE_NAME} Example | {INPUT_PARAM} | Parameters Pattern |
|------------|----------------------|---------------|-------------------|
| **String** | FetchUserProfile | `userId: String` | `["userId": input]` |
| **Int** | FetchOrderDetails | `orderId: Int` | `["orderId": input]` |
| **Custom Model** | CreateOrder | `CreateRequest` | Map object properties |

### **Repository Property Discovery:**
| ViewModel | Repository Property | Usage |
|-----------|-------------------|-------|
| **CRCheckoutPageViewModel** | `checkoutRepo` | `self.checkoutRepo` |
| **CRTopupDongtotViewModel** | `dongtotRespository` | `self.dongtotRespository` |
| **POSViewModel** | `posRepo` | `self.posRepo` |
| **VEHViewModel** | `vehRepo` | `self.vehRepo` |

### **Quick Reference:**
```swift
// String/Int inputs → Direct mapping
var parameters: [String: Any]? { return ["paramName": input] }

// Custom Model → Map properties  
var parameters: [String: Any]? { 
    return ["customerId": input.customerId, "items": input.items] 
}
```

### **Model Pattern with Auto-Detected {GENERIC_MODEL}:**
```swift
// 1. Define data model (Add to {MODEL_CLASS}.swift)
public struct {OUTPUT_PARAM}: Codable {
    public let totalOrders: Int?
    // ... properties with CodingKeys for snake_case
    
    enum CodingKeys: String, CodingKey {
        case totalOrders = "total_orders"
    }
}

// 2. Auto-generated wrapper pattern:
// 🔍 AUTO-DETECT based on module location:
// - AppFeatures/CTCorePayment/* → CRModelCommon<{OUTPUT_PARAM}>
// - AppFeatures/CTVEH/* → BaseResponseModel<{OUTPUT_PARAM}>
// - AppFeatures/CTPos/* → Direct model (no wrapper)
typealias {RESPONSE_MODEL} = {GENERIC_MODEL}<{OUTPUT_PARAM}>
```

<details>
<summary>📋 <strong>Complete Example Templates</strong> (Click to expand)</summary>

#### **Example: FetchOrderStatistics**
```markdown
{USECASE_NAME}: FetchOrderStatistics
{INPUT_PARAM}: String  
{OUTPUT_PARAM}: OrderStatistics
{ENDPOINT_PATH}: "v1/orders/statistics"
{HTTP_METHOD}: get
{RESPONSE_MODEL}: OrderStatisticsResponseModel
{TARGET_CLASS}: CRCheckoutTargets
{SERVICE_CLASS}: CRCheckoutService
{REPOSITORY_CLASS}: CRCheckoutCartRepository
{USECASE_CLASS}: CRCheckoutUseCase
{MODEL_CLASS}: CRCheckOutModel
{VIEWMODEL_CLASS}: CRCheckoutPageViewModel
{GENERIC_MODEL}: CRModelCommon

Parameters: ["order_id": input, "include_details": true]
```

#### **JSON Response Template:**
```json
{
    "data": {
        "total_orders": 150,
        "pending_orders": 25,
        "completed_orders": 120,
        "cancelled_orders": 5,
        "total_revenue": 50000000
    },
    "success": true,
    "message": "Statistics fetched successfully"
}
```

</details>

---

## ✅ **Final Result**

**🎯 Complete 6-Layer Implementation** using 13 required + 1 auto-detected {PLACEHOLDER} variables:
- **NetworkHelper**: Add `{USECASE_NAME}` endpoint  
- **Targets**: Add `{USECASE_NAME}Target` to `{TARGET_CLASS}`
- **Services**: Add method to `{SERVICE_CLASS}`  
- **Repositories**: Add method to Repository
- **UseCases**: Add `CR{USECASE_NAME}UseCase` to `{USECASE_CLASS}.swift`
- **ViewModels**: Add `execute{USECASE_NAME}` method to `{VIEWMODEL_CLASS}`

**🔒 Core Constraint**: Only modify existing files - never create new files

**🚀 Ready to use**: Complete UseCase implementation across all architecture layers
