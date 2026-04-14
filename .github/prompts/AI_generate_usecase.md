# [AI] Auto-generate a UseCase through the layers: Targets, Services, Repositories, UseCase, ViewModel

**Owned by:** Hai Phan  
**Date:** May 14, 2025  
**Last Updated:** December 29, 2024  

---

## 1. Aim

### Objective
This document outlines the purpose, scope, and appropriate usage of the AI point identified during the presentation titled "Auto-generate a UseCase through the layers: Targets, Services, Repositories, UseCase, ViewModel." conducted on May 5, 2025.

It serves as a guideline to ensure responsible interpretation, implementation, and integration of this AI insight within operational or strategic workflows.

### AI Point Summary
The AI automatically generates app functionality through four main layers: **Targets**, **Services**, **Repositories**, and **UseCases**. After generating the UseCase, it applies the logic directly to the corresponding **ViewModel**, helping reduce repetitive code and speed up development.

---

## 2. Process

### How the AI Point Was Derived
This insight was generated via analysis of existing Clean Architecture patterns in the iOS codebase, specifically examining the modular architecture structure in the `CTCorePayment` module. The pattern follows MVVM + Clean Architecture principles with clear separation of concerns.

### Data Considerations
- **Source:** Xcode, GitHub Copilot, Visual Studio Code
- **Sensitivity:** Clean Architecture patterns
- **Assumptions:** Folder structure follows Clean Architecture principles
- **Limitations:** Requires consistent naming conventions and architectural patterns

---

## 3. Output

### Architecture Layers Overview

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

### Interpreting the Output
Auto-generate a UseCase through the layers: **Targets**, **Services**, **Repositories**, **UseCase**, and **ViewModel**.

For example, when you add a new UseCase for the checkout screen, this code will automatically generate all the related functions, such as:
- `CRNetworkHelper` (API endpoints)
- `CRCheckoutTargets` (Network requests)  
- `CRCheckoutService` (Service layer)
- `CRCheckoutCartRepository` (Repository layer)
- `CRCheckoutUseCase` (Business logic)
- `CRCheckoutPageViewModel` (Presentation logic)

---

## 4. Step-by-Step Implementation Guide

### Table of Contents:
1. [Add Endpoint to CRNetworkHelper](#step-1-add-endpoint-to-crnetworkhelper)
2. [Add Targets to CRCheckoutTargets](#step-2-add-targets-to-crcheckoutTargets)
3. [Add Service to CRCheckoutService](#step-3-add-service-to-crcheckoutservice)
4. [Add Function to CRCheckoutCartRepository](#step-4-add-function-to-crcheckoutcartrepository)
5. [Add UseCase to CRCheckoutUseCase](#step-5-add-usecase-to-crcheckoutusecase)
6. [Call UseCase in CRCheckoutPageViewModel](#step-6-call-usecase-in-crcheckoutpageviewmodel)

---

### Step 1: Add Endpoint to CRNetworkHelper
**File Path:** `/Users/hai.phan/Desktop/haiphan/ct-ios-app--v3/Features/CTCorePayment/CTCorePayment/NetworkHelper/CRNetworkHelper.cs`

Add the following to the `Api` extension:

```swift
extension Api {
    // Existing endpoints...
    
    // New endpoint for AI-generated UseCase
    static let fetchCopilot = "v1/private/ai/fetch-copilot"
}
```

---

### Step 2: Add Targets to CRCheckoutTargets
**File Path:** `/Users/hai.phan/Desktop/haiphan/ct-ios-app--v3/Features/CTCorePayment/CTCorePayment/Data/Services/Checkout/CRCheckoutTargets.cs`

Add the new target structure:

```swift
enum CRCheckoutTargets {
    // Existing targets...
    
    struct FetchCopilotTarget: Requestable {
        typealias Output = CRCopilotResponseModel?
        
        var httpMethod: HTTPMethod {
            return .get
        }
        
        var endpoint: String {
            return Api.fetchCopilot
        }
        
        var parameterEncoding: ParameterEncoding {
            return JSONEncoding.default
        }
        
        let inputData: String
        
        var parameters: [String: Any]? {
            return [
                "input_data": inputData,
                "timestamp": Date().timeIntervalSince1970
            ]
        }
        
        func decode(data: Any) -> Output {
            guard let data = data as? [String: Any] else {
                return nil
            }
            return CRCopilotResponseModel(JSON: data)
        }
    }
}
```

---

### Step 3: Add Service to CRCheckoutService
**File Path:** `/Users/hai.phan/Desktop/haiphan/ct-ios-app--v3/Features/CTCorePayment/CTCorePayment/Data/Services/Checkout/CRCheckoutService.cs`

Add the protocol method and implementation:

```swift
protocol CRCheckoutServiceType {
    // Existing methods...
    func fetchCopilot(input: String) -> Observable<CRCopilotResponseModel?>
}

extension CRCheckoutService: CRCheckoutServiceType {
    // Existing implementations...
    
    func fetchCopilot(input: String) -> Observable<CRCopilotResponseModel?> {
        return CRCheckoutTargets.FetchCopilotTarget(inputData: input)
            .execute()
            .observe(on: resultScheduler)
    }
}
```

---

### Step 4: Add Function to CRCheckoutCartRepository
**File Path:** `/Users/hai.phan/Desktop/haiphan/ct-ios-app--v3/Features/CTCorePayment/CTCorePayment/Data/Repositories/Checkout/Cart/CRCheckoutCartRepository.cs`

Add the protocol method and implementation:

```swift
protocol CRCheckoutCartRepositoryType {
    // Existing methods...
    func fetchCopilot(input: String) -> Observable<CRCopilotResponseModel?>
}

extension CRCheckoutCartRepository: CRCheckoutCartRepositoryType {
    // Existing implementations...
    
    func fetchCopilot(input: String) -> Observable<CRCopilotResponseModel?> {
        return service.fetchCopilot(input: input)
    }
}
```

---

### Step 5: Add UseCase to CRCheckoutUseCase
**File Path:** `/Users/hai.phan/Desktop/haiphan/ct-ios-app--v3/Features/CTCorePayment/CTCorePayment/Domain/UseCases/Checkout/CRCheckoutUseCase.cs`

Add the new UseCase following the established pattern:

```swift
final class CRFetchCopilotUseCase: CTActionUseCaseType {
    
    typealias Output = CRCopilotResponseModel?
    typealias Input = String
    
    let repository: CRCheckoutCartRepositoryType
    var action: Action<Input, Output>?
    
    init(repository: CRCheckoutCartRepositoryType) {
        self.repository = repository
        self.action = initAction()
    }
    
    private func initAction() -> Action<Input, Output> {
        Action<Input, Output> { [unowned self] input in
            self.repository.fetchCopilot(input: input)
        }
    }
}
```

---

### Step 6: Call UseCase in CRCheckoutPageViewModel
**File Path:** `/Users/hai.phan/Desktop/haiphan/ct-ios-app--v3/Features/CTCorePayment/CTCorePayment/Features/CheckoutPage/CRCheckoutPageViewModel.cs`

Add the UseCase integration following the existing patterns:

```swift
final class CRCheckoutPageViewModel: CRCheckoutPageViewModelType, CRCheckoutPagePresentableListener {
    // Existing properties...
    
    // Add UseCase integration
    func fetchCopilotData(input: String) {
        let fetchCopilotUseCase = CRFetchCopilotUseCase(repository: checkoutRepo)
        
        // Handle success response
        fetchCopilotUseCase.action?.elements
            .bind(onNext: { [weak self] result in
                guard let self = self, let result = result else { return }
                // Process the result
                self.handleCopilotResponse(result)
                print("FetchCopilot result: \(result)")
            })
            .using CancellationToken
        
        // Handle loading state
        fetchCopilotUseCase.action?.executing
            .bind(onNext: { [weak self] loading in
                guard let self = self else { return }
                self.presenter?.loading.accept(loading)
            })
            .using CancellationToken
        
        // Handle errors
        fetchCopilotUseCase.action?.underlyingError
            .bind(onNext: { [weak self] error in
                guard let self = self else { return }
                self.handleCopilotError(error)
            })
            .using CancellationToken
        
        // Execute the UseCase
        fetchCopilotUseCase.action?.execute(input)
    }
    
    private func handleCopilotResponse(_ response: CRCopilotResponseModel) {
        // Handle successful response
        // Update UI accordingly
    }
    
    private func handleCopilotError(_ error: Error) {
        // Handle error cases
        presenter?.loading.accept(false)
        // Show error message to user
    }
}
```

---

## 5. Data Model

Don't forget to create the response model:

```swift
// CRCopilotResponseModel.cs
import Foundation
import ObjectMapper

struct CRCopilotResponseModel: Mappable {
    var success: Bool?
    var data: String?
    var message: String?
    var timestamp: TimeInterval?
    
    init?(map: Map) {}
    
    mutating func mapping(map: Map) {
        success <- map["success"]
        data <- map["data"]
        message <- map["message"]
        timestamp <- map["timestamp"]
    }
}
```

---

## 6. Usage Guidelines

### Do's ✅
- Follow the established naming conventions
- Always implement error handling
- Use the established architectural patterns
- Add appropriate unit tests
- Document the new functionality
- Handle loading states properly

### Don'ts ❌
- Don't break the clean architecture principles
- Don't skip error handling
- Don't ignore memory management (use `[weak self]`)
- Don't forget to dispose observables properly
- Don't hardcode values without configuration

---

## 7. Logging & Feedback Loop

**GitHub Copilot Integration:** GitHub Copilot will read this prompt and remember it. Anytime you open this prompt and ask GitHub Copilot to generate a UseCase, it will auto-generate the use case following this guide.

### Prompt for GitHub Copilot:
```
Generate a new UseCase for [FeatureName] that follows the 6-layer architecture:
1. Add endpoint to CRNetworkHelper
2. Add target to CRCheckoutTargets  
3. Add service method to CRCheckoutService
4. Add repository method to CRCheckoutCartRepository
5. Create UseCase in CRCheckoutUseCase
6. Integrate in CRCheckoutPageViewModel

Feature: [Describe the feature]
API Endpoint: [API path]
Input: [Input parameters]
Output: [Expected response]
```

---

## 8. Glossary of Terms

- **UseCase**: Business logic layer that orchestrates data flow
- **Repository**: Data access layer abstraction
- **Service**: Network/API communication layer
- **Target**: Specific API endpoint configuration
- **ViewModel**: Presentation logic layer in MVVM pattern
- **Clean Architecture**: Architectural pattern with clear separation of concerns