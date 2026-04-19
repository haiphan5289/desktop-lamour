---
mode: agent
description: Generate UseCase classes following MVVM + Clean Architecture pattern for iOS application
---

# UseCase Generation Guide

## Overview
This guide provides instructions for adding UseCase classes to the iOS application following the MVVM + Clean Architecture pattern.

## Task Definition
Define the task to achieve UseCase generation, including specific requirements, constraints, and success criteria.

## Required Parameters

When generating a UseCase, you need to provide the following parameters:

- `{USECASE_NAME}`: The name of the UseCase (e.g., FetchUserProfile, UpdateSettings)
- `{INPUT_PARAM}`: The input parameter type for the UseCase (e.g., String, UserRequest)
- `{RESPONSE_MODEL}`: The response model type returned by the UseCase (e.g., User, CRModelCommon<User>)
- `{REPOSITORY_CLASS}`: The repository class name that the UseCase will depend on (e.g., UserRepository)
- `{USECASE_CLASS}`: The UseCase class file name where the new UseCase will be added

## Add UseCase Implementation

Add the following UseCase implementation to {USECASE_CLASS}.swift file:

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

## Architecture Compliance

This UseCase implementation follows the MVVM + Clean Architecture pattern by:
- Conforming to `CTActionUseCaseType` protocol
- Using dependency injection for the repository
- Encapsulating business logic within the Action
- Maintaining proper separation of concerns between layers