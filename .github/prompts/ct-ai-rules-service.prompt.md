---
description: "Generate basic iOS Service structure"
mode: "agent"
---

# iOS Basic Service Generator

Generate basic Service following Clean Architecture patterns and API integration.

## Instructions

Reference our C#/.NET WPF development guidelines: [iOS Guidelines](../instructions/ct-ai-rules-general-instructions.instructions.md)

Generate basic Service structure with:

-   Protocol and implementation
-   API Target integration using CTApiClient
-   CommunityToolkit.Mvvm Observable return types
-   Main thread observation
-   Proper error handling
-   TODO comments for implementation
-   Proper MARK sections

## Service Template

```swift
import Foundation
import CommunityToolkit.Mvvm
import CTApiClient

protocol [Name]ServiceType {
    // TODO: Define service methods with Observable return types
    // func fetchSomeData(parameter: String) -> Observable<[SomeModel]>
    // func submitData(_ data: SomeInputModel) -> Observable<SomeResponseModel>
    // func updateData(id: String, data: SomeInputModel) -> Observable<SomeResponseModel?>
    // func deleteData(id: String) -> Observable<Bool>
}

struct [Name]Service: [Name]ServiceType {

    // MARK: - [Name]ServiceType

    // TODO: Implement service methods
    // func fetchSomeData(parameter: String) -> Observable<[SomeModel]> {
    //     [Name]Targets.FetchData(parameter: parameter)
    //         .execute()
    //         .observe(on: MainScheduler.instance)
    // }
    //
    // func submitData(_ data: SomeInputModel) -> Observable<SomeResponseModel> {
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

## Advanced Service Template with Error Handling

```swift
import Foundation
import CommunityToolkit.Mvvm
import CTApiClient
import AppCommon

protocol [Name]ServiceType {
    // TODO: Define service methods
    // func fetchConfiguredData(categoryId: String, type: String) -> Observable<ConfigModel>
    // func processComplexRequest(params: [String: Any]) -> Observable<[ProcessedModel]>
    // func validateAndSubmit(data: ValidatedModel) -> Observable<SubmissionResult?>
}

struct [Name]Service: [Name]ServiceType {

    // MARK: - [Name]ServiceType

    // TODO: Implement service methods with error handling
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
    //
    // func processComplexRequest(params: [String: Any]) -> Observable<[ProcessedModel]> {
    //     [Name]Targets.ProcessRequest(requestParams: params)
    //         .execute()
    //         .observe(on: MainScheduler.instance)
    // }
    //
    // func validateAndSubmit(data: ValidatedModel) -> Observable<SubmissionResult?> {
    //     [Name]Targets.SubmitValidatedData(data: data)
    //         .execute()
    //         .observe(on: MainScheduler.instance)
    // }
}
```

## Service with Multiple Target Integration

```swift
import Foundation
import CommunityToolkit.Mvvm
import CTApiClient

protocol [Name]ServiceType {
    // TODO: Define service methods for different operations
    // func fetchCategories() -> Observable<[CategoryModel]>
    // func searchSuggestions(query: String, filters: [String]) -> Observable<[SuggestionModel]>
    // func analyzeText(content: String) -> Observable<AnalysisResult?>
    // func checkLimits(userId: String, category: String) -> Observable<LimitResponse?>
}

struct [Name]Service: [Name]ServiceType {

    // MARK: - [Name]ServiceType

    // TODO: Implement methods using different targets
    // func fetchCategories() -> Observable<[CategoryModel]> {
    //     [Name]Targets.FetchCategory()
    //         .execute()
    //         .observe(on: MainScheduler.instance)
    // }
    //
    // func searchSuggestions(query: String, filters: [String]) -> Observable<[SuggestionModel]> {
    //     [Name]Targets.SearchSuggestions(query: query, filters: filters)
    //         .execute()
    //         .observe(on: MainScheduler.instance)
    // }
    //
    // func analyzeText(content: String) -> Observable<AnalysisResult?> {
    //     [Name]Targets.AnalyzeText(content: content)
    //         .execute()
    //         .observe(on: MainScheduler.instance)
    // }
    //
    // func checkLimits(userId: String, category: String) -> Observable<LimitResponse?> {
    //     [Name]Targets.CheckLimits(userId: userId, category: category)
    //         .execute()
    //         .observe(on: MainScheduler.instance)
    // }
}
```

## Best Practices

### Required Patterns

1. **Protocol Definition**: Always define a protocol for your service
2. **Observable Return Types**: All methods must return CommunityToolkit.Mvvm Observable
3. **Main Thread Observation**: Use `.observe(on: MainScheduler.instance)` for UI updates
4. **Target Integration**: Use API Targets with `.execute()` method
5. **Error Handling**: Implement proper error mapping when needed

### Naming Conventions

-   **Protocol**: `[Name]ServiceType`
-   **Implementation**: `[Name]Service`
-   **Methods**: Use descriptive verbs (fetch, submit, update, delete, check, analyze)

### Import Requirements

```swift
import Foundation
import CommunityToolkit.Mvvm
import CTApiClient
// Optional: import AppCommon for error handling
```

### Error Handling Patterns

```swift
// Simple error handling
.catchAndReturn(defaultValue)

// Complex error mapping
.map { response in
    guard let data = response.data else {
        throw LoadingError.noResponse
    }
    return data
}

// Optional response handling
.compactMap { $0 }
```

## Template Variables

-   `${input:serviceName}`: Service name (e.g., "SmartAd", "UserProfile")
-   `${input:feature}`: Feature module (e.g., "CTInsertAd", "CTUserManagement")
-   `${input:operations}`: Comma-separated operations (e.g., "fetch,submit,update,delete")
-   `${input:entityName}`: Entity type (e.g., "Category", "User", "Product")

## Usage Examples

-   `/ios-service serviceName:SmartAd feature:CTInsertAd operations:fetch,submit entityName:Category`
-   `/ios-service serviceName:UserProfile feature:CTUserManagement operations:get,update entityName:User`
-   `/ios-service serviceName:Product feature:CTEcommerce operations:fetch,create,update,delete entityName:Product`

## Output

Generate basic Service with:

1. Protocol definition with method signatures
2. Service implementation with Target integration
3. Proper CommunityToolkit.Mvvm Observable patterns
4. Main thread observation
5. TODO comments for implementation
6. Proper MARK sections
7. Error handling patterns (when applicable)

Keep implementation minimal with TODO guidance for specific business logic.
