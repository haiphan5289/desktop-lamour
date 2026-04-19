---
description: Auto Generate and implement a UseCase through the layers for CTUseCaseCommon: Targets, Services, Repositories, UseCase, ViewModel
mode: agent
model: gpt-4o
parameters:
  - name: usecaseName
    description: The name of the UseCase to generate (e.g., GetForceUpdatePos)
    required: true
    default: GetForceUpdatePos
  - name: outputParam
    description: The output parameter type (e.g., ForceUpdatePos)
    required: true
    default: ForceUpdatePos
  - name: endpointPath
    description: The API endpoint path (e.g., "v1/force-update/pos")
    required: true
    default: "v1/force-update/pos"
  - name: httpMethod
    description: The HTTP method (e.g., get)
    required: true
    default: get
  - name: inputParam
    description: The input parameter type (e.g., String)
    required: true
    default: String
  - name: responseModel
    description: The response model name (e.g., ForceUpdatePosResponseModel)
    required: true
    default: ForceUpdatePosResponseModel
  - name: viewModelClass
    description: The target ViewModel class to update (must be parameter)
    required: true
    default: POSPremiumFeaturesViewModel
  - name: moduleType
    description: The module type for specific handling (e.g., CTPOS, CTFeed, CTShop)
    required: false
    default: CTPOS
---

# Follow template ct_ai_generate_usecase_template.prompt.md with these CTUseCaseCommon-specific parameters:

1. Follow the template file `ct_ai_generate_usecase_template.prompt.md` for Steps 1-5
2. **SKIP Step 6** from the template - Replace with Steps 8-10 below

### Step 1-5 CTUseCaseCommon Specific Modifications:

#### Step 2 Modification: CTUseCaseCommonTargets Pattern
```swift
struct {{usecaseName}}Target: Requestable {
    typealias Output = UseCaseCommon<{{responseModel}}>?
    
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
        guard let data = data as? Data else {
            return nil
        }
        do {
            return try JSONDecoder().decode(UseCaseCommon<{{responseModel}}>.self, from: data)
        } catch {
            print("Decode error: \(error)")
            return nil
        }
    }
}
```

**⚠️ Key Differences from POS Module:**
- Use `JSONDecoder` instead of `ObjectMapper`
- Use `UseCaseCommon<{{responseModel}}>` wrapper
- Use `Codable` protocol instead of `Mappable`

### Step 8: Update {{viewModelClass}} (Parameter)
#### 8.1: Add Property
```swift
let {{usecaseName}}UseCase: {{usecaseName}}UseCase
```

#### 8.2: Update Init Method - Add parameter and assignment
```swift
{{usecaseName}}UseCase: {{usecaseName}}UseCase
```
```swift
self.{{usecaseName}}UseCase = {{usecaseName}}UseCase
```

#### 8.3: Add Execution Method (with Module Handling)

**Module-Specific Implementation:**
- **For {{moduleType}} Module**: 
  - **CTPOS**: Refer to `/Users/hai.phan/Desktop/haiphan/ct-ios-app--v3/AppFeatures/CTPos`
  - **Other Modules**: Use standard implementation pattern

```swift
extension {{viewModelClass}} {
    func execute{{usecaseName}}(input: {{inputParam}}) {
        self.{{usecaseName}}UseCase.action?.elements
            .bind(onNext: { [weak self] result in
                guard let self = self, let result = result else { return }
                print("{{usecaseName}} success: \(result)")
                
                // Module-specific handling ({{moduleType}})
                // For CTPOS: Handle result according to POS business logic
                // For other modules: Customize as needed
                self.handle{{usecaseName}}Success(result)
            })
            .disposed(by: disposeBag)
        
        self.{{usecaseName}}UseCase.action?.executing
            .bind(onNext: { [weak self] loading in
                self?.presenter?.onLoadingPublisher.onNext(loading)
            })
            .disposed(by: disposeBag)
        
        self.{{usecaseName}}UseCase.action?.underlyingError
            .bind(onNext: { [weak self] error in 
                guard let error = error else { return }
                print("{{usecaseName}} error: \(error)")
                
                // Module-specific error handling ({{moduleType}})
                // For CTPOS: Handle error according to POS error patterns
                // For other modules: Customize as needed
                self.handle{{usecaseName}}Error(error)
            })
            .disposed(by: disposeBag)
        
        self.{{usecaseName}}UseCase.action?.execute(input)
    }
    
    // MARK: - {{moduleType}} Module-Specific Handlers
    private func handle{{usecaseName}}Success(_ result: {{outputParam}}) {
        // TODO: Implement {{moduleType}}-specific success handling
        // For CTPOS: Update UI state, trigger POS-specific actions
        // For other modules: Implement appropriate business logic
    }
    
    private func handle{{usecaseName}}Error(_ error: Error) {
        // TODO: Implement {{moduleType}}-specific error handling
        // For CTPOS: Show POS-appropriate error messages
        // For other modules: Implement appropriate error handling
    }
}
```

**📁 Module Reference Paths:**
- **CTPOS Module**: `/Users/hai.phan/Desktop/haiphan/ct-ios-app--v3/AppFeatures/CTPos`
- **Other Modules**: Follow similar patterns in respective AppFeatures directories

### Step 9: Update Navigator (Parameter - must specify which Navigator)
```swift
// Create UseCase
let {{usecaseName}}UseCase = {{usecaseName}}UseCase(repository: repository)

// Add to {{viewModelClass}} initialization
{{usecaseName}}UseCase: {{usecaseName}}UseCase
```

### Step 10: Update UseCasesAssembly.swift (Parameter)
```swift
container.autoregister({{usecaseName}}UseCase.self, initializer: {{usecaseName}}UseCase.init)
```

## ⚠️ CTUseCaseCommon Module Specific Notes:

**CRITICAL:** 
- Use `JSONDecoder` with `Codable` protocol - NOT ObjectMapper
- Use `UseCaseCommon<{{responseModel}}>` wrapper for responses
- ViewModel Class is a parameter - must specify which ViewModel to update
- Dependency Injection Pattern - {{viewModelClass}} requires UseCase injection
- Navigator is parameterized - Must specify which Navigator to update

## 📁 File Creation Strategy:

### Add to Existing Files (For consolidation)
- Add models to `ForceUpdatePosModel.swift`
- Add UseCase to `GetForceUpdatePosUseCase.swift`

## 📋 File Structure After Implementation:
```
CTUseCaseCommon/
├── Data/
│   ├── Services/
│   │   ├── CTUseCaseCommonTargets.swift ✏️ (Modified)
│   │   ├── CTUseCaseCommonServices.swift ✏️ (Modified)
│   │   └── CTUseCaseCommonServiceType.swift ✏️ (Modified)
│   └── Repositories/
│       ├── CTUseCaseCommonRepository.swift ✏️ (Modified)
│       └── CTUseCaseCommonRepositoryType.swift ✏️ (Modified)
├── Domain/
│   ├── Entities/
│   │   └── ForceUpdatePosModel.swift ✏️ (Modified - Added {{outputParam}} models)
│   └── UseCases/
│       └── GetForceUpdatePosUseCase.swift ✏️ (Modified - Added {{usecaseName}}UseCase)
├── Presentation/
│   └── {{viewModelClass}}.swift ✏️ (Modified - Parameter)
├── Navigator/
│   └── [Navigator].swift ✏️ (Modified - Parameter)
└── DependencyInjection/
    └── UseCasesAssembly.swift ✏️ (Modified)
```

## 🔧 Troubleshooting:

### ❌ Compilation errors in CTUseCaseCommonTargets:
**Solution**: Verify JSONDecoder import and decode pattern with UseCaseCommon wrapper

### ❌ "Property '{{usecaseName}}UseCase' not found":
**Solution**: Check Step 8.1 and 8.2 are completed in {{viewModelClass}}

### ❌ "Cannot find UseCaseCommon wrapper":
**Solution**: Ensure UseCaseCommon<T> generic model is imported and available

## ✅ Verification Checklist:
- [ ] Step 0: Added {{outputParam}} models and {{responseModel}} to ForceUpdatePosModel.swift (using Codable)
- [ ] Step 1-5: Updated all layers (Targets, Service, Repository, UseCase) with JSONDecoder
- [ ] Step 8: Updated {{viewModelClass}} (property, init, method)
- [ ] Step 9: Updated specified Navigator
- [ ] Step 10: Updated UseCasesAssembly
- [ ] Added {{usecaseName}}UseCase to GetForceUpdatePosUseCase.swift
- [ ] Project builds successfully
- [ ] UseCaseCommon<{{responseModel}}> wrapper used correctly

## Instructions:

### Step 0: Create Output Models (If not exists)
**Before following template steps, create the output models using Codable:**

```swift
//
//  {{outputParam}}.swift
//  CTUseCaseCommon
//
//  Created by AI Assistant on $(date)
//

import Foundation

struct {{outputParam}}: Codable {
    // TODO: Add properties based on JSON response structure
    // Example for response: {"data": {"force_update": true, "version": "1.0.0"}}
    let forceUpdate: Bool
    let version: String
    // ... add more properties
    
    enum CodingKeys: String, CodingKey {
        case forceUpdate = "force_update"
        case version
        // ... map other properties
    }
}

struct {{responseModel}}: Codable {
    let data: {{outputParam}}
    let success: Bool
    let message: String
    
    enum CodingKeys: String, CodingKey {
        case data
        case success
        case message
    }
}
```

## 📚 How to Use

This prompt generates a complete UseCase implementation for the CTUseCaseCommon module. 

### Example Usage:
```
usecaseName: GetForceUpdatePos
outputParam: ForceUpdatePos  
endpointPath: "v1/force-update/pos"
httpMethod: get
inputParam: String
responseModel: ForceUpdatePosResponseModel
viewModelClass: POSPremiumFeaturesViewModel
moduleType: CTPOS
```

### The prompt will:
1. Create Codable models for the response
2. Update CTUseCaseCommonTargets with new endpoint
3. Update CTUseCaseCommonServices, Repository, and UseCase layers
4. Inject the UseCase into the specified ViewModel
5. Update Navigator and DI Assembly

### Key differences from POS module:
- Uses JSONDecoder with Codable (not ObjectMapper)
- Uses UseCaseCommon<T> wrapper
- ViewModel and Navigator are parameterized
- Module-specific handling with special references (e.g., CTPOS → `/Users/hai.phan/Desktop/haiphan/ct-ios-app--v3/AppFeatures/CTPos`)