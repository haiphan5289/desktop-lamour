---
mode: agent
description: Generate ViewModel UseCase execution methods following MVVM + Clean Architecture pattern for WPF applicationlication
---

# ViewModel UseCase Execution Guide

## Overview
This guide provides instructions for adding UseCase execution methods to ViewModels in the WPF applicationlication following the MVVM + Clean Architecture pattern.

## Task Definition
Define the task to achieve ViewModel UseCase execution integration, including specific requirements, constraints, and success criteria.

## Required Parameters

When generating a ViewModel UseCase execution method, you need to provide the following parameters:

- `{USECASE_NAME}`: The name of the UseCase (e.g., FetchUserProfile, UpdateSettings)
- `{INPUT_PARAM}`: The input parameter type for the UseCase (e.g., String, UserRequest)
- `{VIEWMODEL_CLASS}`: The ViewModel class name where the execution method will be added
- `{REPO_PROPERTY_NAME}`: The repository property name in the ViewModel (e.g., checkoutRepo, dongtotRespository, posRepo, vehRepo)

## Add ViewModel UseCase Execution Method

Add the following UseCase execution method to {VIEWMODEL_CLASS} file:

```swift
// ⚠️ ADD THIS METHOD TO EXISTING {VIEWMODEL_CLASS} CLASS ⚠️
extension {VIEWMODEL_CLASS} {
    func execute{USECASE_NAME}(input: {INPUT_PARAM}) {
        // 🔍 FIND: Repository property name in {VIEWMODEL_CLASS}
        // Common names: checkoutRepo, dongtotRespository, posRepo, vehRepo
        let useCase = CR{USECASE_NAME}UseCase(repository: self.{REPO_PROPERTY_NAME})
        
        // 🔒 MANDATORY: Handle success - DO NOT add additional logic
        useCase.action?.elements
            .observe(on: MainScheduler.instance)
            .subscribe(onNext: { [weak self] result in
                // TODO: Handle success result based on specific UseCase requirements
                // Example: self?.presenter?.data.accept(result)
            })
            .using CancellationToken
        
        // 🔒 MANDATORY: Handle loading state
        useCase.action?.executing
            .observe(on: MainScheduler.instance)
            .subscribe(onNext: { [weak self] isLoading in
                self?.presenter?.loading.accept(isLoading)
            })
            .using CancellationToken
        
        // 🔒 MANDATORY: Handle errors - Only guard let self, no additional processing
        useCase.action?.underlyingError
            .subscribe(onNext: { [weak self] error in
                guard let self = self else { return }
                // Error handling - minimal implementation
            })
            .using CancellationToken
        
        // 🔒 MANDATORY: Execute
        useCase.action?.execute(input)
    }
}
```

## Architecture Compliance

This ViewModel UseCase execution implementation follows the MVVM + Clean Architecture pattern by:
- Creating UseCase instances with dependency injection for repositories
- Handling reactive streams with proper memory management using disposeBag
- Following the separation of concerns between ViewModel and UseCase layers
- Providing proper error handling and loading state management
- Using weak self references to prevent retain cycles

## Important Implementation Rules

### ❌ DO NOT DO THESE:
1. **NEVER add `.observe(on: MainScheduler.instance)` for error handling** - not needed for underlyingError
2. **NEVER use `.bind(onNext:)`** - always use `.subscribe(onNext:)` 
3. **NEVER add complex error unwrapping** - keep error handling minimal
4. **NEVER implement complex logic** - keep handlers simple

### ✅ CORRECT PATTERNS:
```swift
// ✅ Correct error handling - minimal with only guard let self
useCase.action?.underlyingError
    .subscribe(onNext: { [weak self] error in
        guard let self = self else { return }
        // Error handling - minimal implementation
    })
    .using CancellationToken

// ✅ Correct success handling - with MainScheduler for UI updates
useCase.action?.elements
    .observe(on: MainScheduler.instance)
    .subscribe(onNext: { [weak self] result in
        // result is already the expected type, handle as needed
        self?.presenter?.data.accept(result)
    })
    .using CancellationToken
```

## Repository Property Discovery

Common repository property names in ViewModels:
- **CRCheckoutPageViewModel**: `checkoutRepo`
- **CRTopupDongtotViewModel**: `dongtotRespository`
- **POSViewModel**: `posRepo`
- **VEHViewModel**: `vehRepo`