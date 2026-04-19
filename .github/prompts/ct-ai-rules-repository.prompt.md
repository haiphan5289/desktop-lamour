---
description: "Generate basic iOS Repository structure"
mode: "agent"
---

# iOS Basic Repository Generator

Generate basic Repository following Clean Architecture patterns.

## Instructions

Reference our iOS development guidelines: [iOS Guidelines](../instructions/ios-general-instructions.instructions.md)

Generate basic Repository structure with:

-   Protocol and implementation
-   Service dependency injection
-   Basic method signatures
-   TODO comments for implementation
-   Proper MARK sections

## Repository Template

```swift
import Foundation
import RxSwift
import CTCommon

protocol [Name]RepositoryType: AnyObject {
    // TODO: Define repository methods with Observable return types
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

    let service: [Name]ServiceRequestable

    // MARK: - Initialization

    init(service: [Name]ServiceRequestable) {
        self.service = service
    }

    // MARK: - [Name]RepositoryType

    // TODO: Implement repository methods
    // func getSomeData(
    //     parameter1: String?,
    //     parameter2: Int
    // ) -> Observable<SomeResponseModel> {
    //     service.getSomeData(parameter1: parameter1, parameter2: parameter2)
    //         .compactMap { $0 }
    //         .map { response in
    //             // TODO: Map service response to domain model
    //             return SomeResponseModel(from: response)
    //         }
    // }
    //
    // func processSomeData(
    //     data: SomeInputModel
    // ) -> Observable<[SomeOutputModel]> {
    //     service.processSomeData(data: data)
    //         .compactMap { response in
    //             guard let items = response?.items, !items.isEmpty else { return [] }
    //             // TODO: Process and map response items
    //             items.enumerated().forEach { index, item in
    //                 item.id = UUID().uuidString
    //                 item.index = index
    //             }
    //             return items
    //         }
    // }
}
```

## Template Variables

-   `${input:repositoryName}`: Repository name (e.g., "UserProfile")
-   `${input:feature}`: Feature module (e.g., "CTUserManagement")
-   `${input:entityName}`: Entity type (e.g., "User")
-   `${input:operations}`: Comma-separated operations (e.g., "get,create,update,delete")

## Usage Examples

-   `/ios-repository repositoryName:UserProfile feature:CTUserManagement entityName:User`
-   `/ios-repository repositoryName:Product feature:CTEcommerce entityName:Product`

## Output

Generate basic Repository with:

1. Protocol definition with method signatures
2. Implementation with service dependency
3. Observable return types
4. TODO comments for implementation
5. Proper naming conventions
6. MARK sections for organization

Keep implementation minimal with TODO guidance for data operations.
