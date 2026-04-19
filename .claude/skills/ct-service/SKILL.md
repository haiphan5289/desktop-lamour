---
name: ct-service
description: Generate a basic iOS Service (protocol + implementation) following Clean Architecture patterns. Services call API Targets with .execute() and return RxSwift Observables. Use when adding a new service layer for network API calls. Named [Name]ServiceType (protocol) and [Name]Service (struct).
---

# iOS Basic Service Generator

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Generate Service protocol and implementation following Clean Architecture and API integration patterns.

## Input Format

```
SERVICE_NAME: <ServiceName, e.g. "SmartAd">
FEATURE: <Module, e.g. "CTInsertAd">
OPERATIONS: <comma-separated, e.g. "fetch,submit,update,delete">
ENTITY: <entity type, e.g. "Category">
```

## Service Template

```swift
import Foundation
import RxSwift
import CTApiClient

protocol [Name]ServiceType {
    // func fetchSomeData(parameter: String) -> Observable<[SomeModel]?>
    // func submitData(_ data: SomeInputModel) -> Observable<SomeResponseModel?>
    // func updateData(id: String, data: SomeInputModel) -> Observable<SomeResponseModel?>
    // func deleteData(id: String) -> Observable<Bool>
}

struct [Name]Service: [Name]ServiceType {

    // MARK: - [Name]ServiceType

    // func fetchSomeData(parameter: String) -> Observable<[SomeModel]?> {
    //     [Name]Targets.FetchData(parameter: parameter)
    //         .execute()
    //         .observe(on: MainScheduler.instance)
    // }
    //
    // func submitData(_ data: SomeInputModel) -> Observable<SomeResponseModel?> {
    //     [Name]Targets.SubmitData(data: data)
    //         .execute()
    //         .observe(on: MainScheduler.instance)
    // }
    //
    // func updateData(id: String, data: SomeInputModel) -> Observable<SomeResponseModel?> {
    //     [Name]Targets.UpdateData(id: id, data: data)
    //         .execute()
    //         .observe(on: MainScheduler.instance)
    // }
    //
    // func deleteData(id: String) -> Observable<Bool> {
    //     [Name]Targets.DeleteData(id: id)
    //         .execute()
    //         .map { _ in true }
    //         .catchAndReturn(false)
    //         .observe(on: MainScheduler.instance)
    // }
}
```

## Advanced Service with Error Handling

```swift
import Foundation
import RxSwift
import CTApiClient
import CTCommon

protocol [Name]ServiceType {
    // func fetchConfiguredData(categoryId: String, type: String) -> Observable<ConfigModel>
}

struct [Name]Service: [Name]ServiceType {

    // func fetchConfiguredData(categoryId: String, type: String) -> Observable<ConfigModel> {
    //     let observable = [Name]Targets.GetConfiguration(categoryId: categoryId).execute()
    //     return observable
    //         .map { response in
    //             guard let config = response[type] else {
    //                 throw LoadingError.noResponse
    //             }
    //             return config
    //         }
    //         .observe(on: MainScheduler.instance)
    // }
}
```

## Naming Conventions

- **Protocol**: `[Name]ServiceType`
- **Implementation**: `[Name]Service` (struct, not class)
- **Methods**: descriptive verbs — `fetch`, `submit`, `update`, `delete`, `check`, `analyze`

## Required Imports

```swift
import Foundation
import RxSwift
import CTApiClient
// Optional: import CTCommon for error handling
```

## Error Handling Patterns

```swift
// Simple fallback
.catchAndReturn(defaultValue)

// Custom error mapping
.map { response in
    guard let data = response.data else {
        throw LoadingError.noResponse
    }
    return data
}

// Optional compaction
.compactMap { $0 }
```

## Rules

1. Always define a protocol `[Name]ServiceType` — never a concrete-only service
2. All methods MUST return `Observable<T>` (RxSwift)
3. Always add `.observe(on: MainScheduler.instance)` at the end of each chain
4. Use Target's `.execute()` method for API calls
5. Implementation is a `struct`, not a `class`
6. Never add business logic to services — only data fetch/transform
