---
description: Auto Generate and implement a UseCase through the layers: Targets, Services, Repositories, UseCase, ViewModel
mode: agent
model: gpt-4o
parameters:
  - name: usecaseName
    description: The name of the UseCase to generate (e.g., FetchOrderStatistics2)
    required: true
    default: FetchOrderStatistics2
  - name: outputParam
    description: The output parameter type (e.g., OrderStatistics)
    required: true
    default: OrderStatistics
  - name: endpointPath
    description: The API endpoint path (e.g., "v1/orders/statistics2")
    required: true
    default: "v1/orders/statistics2"
  - name: httpMethod
    description: The HTTP method (e.g., get)
    required: true
    default: get
  - name: inputParam
    description: The input parameter type (e.g., String)
    required: true
    default: String
  - name: responseModel
    description: The response model name (e.g., OrderStatisticsResponseModel)
    required: true
    default: OrderStatisticsResponseModel
  - name: genericModel
    description: The generic wrapper model class (auto-detect or use default)
    default: "None"
---

# Follow template ct_ai_generate_usecase_template.prompt.md with these POS-specific parameters:

**Module:** POS
**UseCase:** {{usecaseName}}
**Input:** {{inputParam}}
**Output:** {{outputParam}}
**Endpoint:** {{endpointPath}}
**Method:** {{httpMethod}}
**Response Model:** {{responseModel}}
**Endpoint Class:** POSUniTargets (⚠️ Direct endpoint, not NetworkHelper+Api)
**Target Class:** POSUniTargets
**Service Class:** POSUniService
**Repository Class:** POSRepository
**UseCase Class:** POSGetBundlesUseCase
**Model Class:** POSQuantityModel
**ViewModel Class:** POSPremiumFeaturesViewModel
**Generic Model:** {{genericModel}} (For POS: Direct model, no wrapper)

## Mapping to Template Variables:

Replace these placeholders in ct_ai_generate_usecase_template.prompt.md:

- `{USECASE_NAME}` → {{usecaseName}}
- `{INPUT_PARAM}` → {{inputParam}}
- `{OUTPUT_PARAM}` → {{outputParam}}
- `{ENDPOINT_PATH}` → {{endpointPath}}
- `{HTTP_METHOD}` → {{httpMethod}}
- `{RESPONSE_MODEL}` → {{responseModel}}
- `{ENDPOINT_CLASS}` → POSUniTargets
- `{TARGET_CLASS}` → POSUniTargets
- `{SERVICE_CLASS}` → POSUniService
- `{REPOSITORY_CLASS}` → POSRepository
- `{USECASE_CLASS}` → POSGetBundlesUseCase
- `{MODEL_CLASS}` → POSQuantityModel
- `{VIEWMODEL_CLASS}` → POSPremiumFeaturesViewModel
- `{GENERIC_MODEL}` → {{genericModel}}

## Instructions:

### Step 0: Create Output Models (If not exists)
**Before following template steps, create the output models:**

```swift
//
//  {{outputParam}}.cs
//  CTPos
//
//  Created by AI Assistant on $(date)
//

import Foundation
import ObjectMapper
import AppCommon

struct {{outputParam}}: Mappable {
    // TODO: Add properties based on JSON response structure
    // Example for response: {"data": {"total_orders": 150, "pending_orders": 25}}
    var totalOrders: Int = 0
    var pendingOrders: Int = 0
    // ... add more properties
    
    init() {}
    
    init?(map: Map) {}
    
    mutating func mapping(map: Map) {
        totalOrders <- map["total_orders"]
        pendingOrders <- map["pending_orders"]
        // ... map other properties
    }
}

struct {{responseModel}}: Mappable {
    var data: {{outputParam}} = {{outputParam}}()
    var success: Bool = false
    var message: String = ""
    
    init() {}
    init?(map: Map) {}
    
    mutating func mapping(map: Map) {
        data <- map["data"]
        success <- map["success"] 
        message <- map["message"]
    }
}
```

1. Follow the template file `ct_ai_generate_usecase_template.prompt.md` for Steps 1-5
2. **SKIP Step 6** from the template - Replace with Steps 8-10 below

### Step 1-5 POS Specific Modifications:

#### Step 2 Modification: POSUniTargets Pattern
```swift
struct {{usecaseName}}Target: Requestable {
    typealias Output = {{responseModel}}?
    
    var endpoint: String {
        return "{{endpointPath}}"
    }
    
    var httpMethod: HTTPMethod {
        return .{{httpMethod}}
    }
    
    var responseDispatchQueue: DispatchQueue {
        .global(qos: .userInitiated)
    }
    
    var parameters: Parameters {
        // TODO: Customize based on {{inputParam}} type
        return ["param_key": input, "additional_param": true]
    }
    
    let input: {{inputParam}}
    
    func decode(data: Any) -> Output {
        guard let data = data as? [String: Any] else {
            return nil
        }
        return Mapper<{{responseModel}}>().map(JSONObject: data)
    }
}
```

**⚠️ Key Differences from Template:**
- Use `ObjectMapper` instead of `JSONDecoder`
- Direct endpoint strings (no Api.constantName)
- No ResponseEntity wrapper

### Step 8: Update POSPremiumFeaturesViewModel
#### 8.1: Add Property
```swift
let {{usecaseName}}UseCase: POS{{usecaseName}}UseCase
```

#### 8.2: Update Init Method - Add parameter and assignment
```swift
{{usecaseName}}UseCase: POS{{usecaseName}}UseCase
```
```swift
self.{{usecaseName}}UseCase = {{usecaseName}}UseCase
```

#### 8.3: Add Execution Method
```swift
extension POSPremiumFeaturesViewModel {
    func execute{{usecaseName}}(input: {{inputParam}}) {
        self.{{usecaseName}}UseCase.action?.elements
            .bind(onNext: { [weak self] result in
                guard let self = self, let result = result else { return }
                print("{{usecaseName}} success: \(result)")
            })
            .using CancellationToken
        
        self.{{usecaseName}}UseCase.action?.executing
            .bind(onNext: { [weak self] loading in
                self?.presenter?.onLoadingPublisher.onNext(loading)
            })
            .using CancellationToken
        
        self.{{usecaseName}}UseCase.action?.underlyingError
            .bind(onNext: { [weak self] error in 
                guard let error = error else { return }
                print("{{usecaseName}} error: \(error)")
            })
            .using CancellationToken
        
        self.{{usecaseName}}UseCase.action?.execute(input)
    }
}
```

### Step 9: Update POSInternalNavigator.cs
```swift
// Create UseCase
let {{usecaseName}}UseCase = POS{{usecaseName}}UseCase(posRepository: repository)

// Add to POSPremiumFeaturesViewModel initialization
{{usecaseName}}UseCase: {{usecaseName}}UseCase
```

### Step 10: Update UseCasesAssembly.cs
```swift
container.autoregister(POS{{usecaseName}}UseCase.self, initializer: POS{{usecaseName}}UseCase.init)
```

## ⚠️ POS Module Specific Notes:

**CRITICAL:** 
- NO NetworkHelper+Api.cs - Use direct endpoint strings in POSUniTargets.cs
- NO Generic Wrapper - Use direct models ({{responseModel}} = {{outputParam}})
- Dependency Injection Pattern - POSPremiumFeaturesViewModel requires UseCase injection
- POSInternalNavigator Integration - Must create UseCase and inject into ViewModel

## 📁 File Creation Strategy:

### Add to Existing Files (For consolidation)
- Add models to `POSQuantityModel.cs`
- Add UseCase to `POSGetBundlesUseCase.cs`

## 📋 File Structure After Implementation:
```
CTPos/
├── Data/
│   ├── Services/
│   │   ├── POSUniTargets.cs ✏️ (Modified)
│   │   ├── POSUniService.cs ✏️ (Modified)
│   │   └── POSServiceType.cs ✏️ (Modified)
│   └── Repositories/
│       ├── POSRepository.cs ✏️ (Modified)
│       └── POSRepositoryType.cs ✏️ (Modified)
├── Domain/
│   ├── Entities/
│   │   └── POSQuantityModel.cs ✏️ (Modified - Added {{outputParam}} models)
│   └── UseCases/
│       └── POSGetBundlesUseCase.cs ✏️ (Modified - Added POS{{usecaseName}}UseCase)
├── Presentation/
│   └── PremiumFeatures/
│       └── POSPremiumFeaturesViewModel.cs ✏️ (Modified)
├── Navigator/
│   └── POSInternalNavigator.cs ✏️ (Modified)
└── DependencyInjection/
    └── UseCasesAssembly.cs ✏️ (Modified)
```

## 🔧 Troubleshooting:

### ❌ Compilation errors in POSUniTargets:
**Solution**: Verify ObjectMapper import and decode pattern matches existing targets

### ❌ "Property '{{usecaseName}}UseCase' not found":
**Solution**: Check Step 8.1 and 8.2 are completed in POSPremiumFeaturesViewModel

## ✅ Verification Checklist:
- [ ] Step 0: Added {{outputParam}} models and {{responseModel}} to POSQuantityModel.cs
- [ ] Step 1-5: Updated all layers (Targets, Service, Repository, UseCase)
- [ ] Step 8: Updated POSPremiumFeaturesViewModel (property, init, method)
- [ ] Step 9: Updated POSInternalNavigator
- [ ] Step 10: Updated UseCasesAssembly
- [ ] Added POS{{usecaseName}}UseCase to POSGetBundlesUseCase.cs
- [ ] Project builds successfully