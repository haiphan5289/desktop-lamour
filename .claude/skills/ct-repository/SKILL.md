---
name: ct-repository
description: Generate a basic iOS Repository (protocol + implementation) following Clean Architecture. Repositories abstract data access by delegating to Services. Use when adding a repository layer between UseCase and Service. Named [Name]RepositoryType (protocol) and [Name]Repository (class, subclasses NSObject).
---

# iOS Basic Repository Generator

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Generate Repository protocol and implementation following Clean Architecture patterns.

## Input Format

```
REPOSITORY_NAME: <Name, e.g. "UserProfile">
FEATURE: <Module, e.g. "CTUserManagement">
ENTITY: <Entity type, e.g. "User">
OPERATIONS: <comma-separated, e.g. "get,create,update,delete">
```

## Repository Template

```swift
import Foundation
import RxSwift
import CTCommon

protocol [Name]RepositoryType: AnyObject {
    // func getSomeData(
    //     parameter1: String?,
    //     parameter2: Int
    // ) -> Observable<SomeResponseModel>
    // func processSomeData(
    //     data: SomeInputModel
    // ) -> Observable<[SomeOutputModel]>
}

class [Name]Repository: NSObject, [Name]RepositoryType {

    // MARK: - Properties

    let service: [Name]ServiceType

    // MARK: - Initialization

    init(service: [Name]ServiceType) {
        self.service = service
    }

    // MARK: - [Name]RepositoryType

    // func getSomeData(
    //     parameter1: String?,
    //     parameter2: Int
    // ) -> Observable<SomeResponseModel> {
    //     service.getSomeData(parameter1: parameter1, parameter2: parameter2)
    //         .compactMap { $0 }
    //         .map { response in
    //             // Map service response to domain model if needed
    //             return response
    //         }
    // }
    //
    // func processSomeData(
    //     data: SomeInputModel
    // ) -> Observable<[SomeOutputModel]> {
    //     service.processSomeData(data: data)
    //         .compactMap { response in
    //             guard let items = response?.items, !items.isEmpty else { return [] }
    //             return items
    //         }
    // }
}
```

## Rules

1. Protocol named `[Name]RepositoryType` — must be `AnyObject`
2. Implementation is a `class` subclassing `NSObject` (required for Swinject compatibility)
3. Repository methods pass through to service with minimal transformation
4. Use `.compactMap { $0 }` when service returns Optional
5. Map service response models to domain models here if they differ
6. Inject `[Name]ServiceType` through initializer (never concrete type)
7. Return types must be RxSwift `Observable<T>`

## Naming Conventions

- **Protocol**: `[Name]RepositoryType`
- **Implementation**: `[Name]Repository`
- **Service dependency**: `[Name]ServiceType` (injected via init)
- **Methods**: pass-through from UseCase, mirrors service with domain-level naming
