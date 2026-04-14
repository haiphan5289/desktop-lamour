---
name: ct-service
description: Generate a C#/.NET Service (interface + implementation) following Clean Architecture patterns. Services use typed HttpClient to call REST APIs and return Task<T>. Use when adding a new service layer for network API calls. Named I[Name]Service (interface) and [Name]Service (sealed class).
---

# WPF Basic Service Generator

Generate Service interface and implementation following Clean Architecture and REST API integration patterns.

## Input Format

```
SERVICE_NAME: <ServiceName, e.g. "Product">
FEATURE: <Module, e.g. "Features/Products">
OPERATIONS: <comma-separated, e.g. "get,create,update,delete">
ENTITY: <DTO type, e.g. "ProductDto">
```

## Service Template

```csharp
// Data layer — I[Name]Service.cs
namespace App.[Feature].Data.Services;

public interface I[Name]Service
{
    // Task<[Entity]Dto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    // Task<IReadOnlyList<[Entity]Dto>> GetAllAsync(CancellationToken cancellationToken = default);
    // Task<[Entity]Dto> CreateAsync([Entity]CreateDto request, CancellationToken cancellationToken = default);
    // Task<[Entity]Dto> UpdateAsync(string id, [Entity]UpdateDto request, CancellationToken cancellationToken = default);
    // Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
```

```csharp
// Data layer — [Name]Service.cs
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace App.[Feature].Data.Services;

public sealed class [Name]Service : I[Name]Service
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<[Name]Service> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public [Name]Service(HttpClient httpClient, ILogger<[Name]Service> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    // public async Task<[Entity]Dto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    // {
    //     var response = await _httpClient.GetAsync($"api/[entity]/{id}", cancellationToken);
    //     response.EnsureSuccessStatusCode();
    //     return await response.Content.ReadFromJsonAsync<[Entity]Dto>(_jsonOptions, cancellationToken);
    // }

    // public async Task<IReadOnlyList<[Entity]Dto>> GetAllAsync(CancellationToken cancellationToken = default)
    // {
    //     var result = await _httpClient.GetFromJsonAsync<List<[Entity]Dto>>("api/[entity]", _jsonOptions, cancellationToken);
    //     return result ?? [];
    // }

    // public async Task<[Entity]Dto> CreateAsync([Entity]CreateDto request, CancellationToken cancellationToken = default)
    // {
    //     var response = await _httpClient.PostAsJsonAsync("api/[entity]", request, cancellationToken);
    //     response.EnsureSuccessStatusCode();
    //     return (await response.Content.ReadFromJsonAsync<[Entity]Dto>(_jsonOptions, cancellationToken))!;
    // }

    // public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    // {
    //     var response = await _httpClient.DeleteAsync($"api/[entity]/{id}", cancellationToken);
    //     response.EnsureSuccessStatusCode();
    // }
}
```

## DTO Model

```csharp
// Data/Dtos/[Entity]Dto.cs
using System.Text.Json.Serialization;

namespace App.[Feature].Data.Dtos;

public sealed record [Entity]Dto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    // [JsonPropertyName("description")]
    // public string? Description { get; init; }
}
```

## DI Registration

```csharp
// In ServiceCollectionExtensions.cs
services.AddHttpClient<I[Name]Service, [Name]Service>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
```

## Error Handling Patterns

```csharp
// Re-throw with context
try
{
    var response = await _httpClient.GetAsync(url, cancellationToken);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<TDto>(_jsonOptions, cancellationToken);
}
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "HTTP error fetching {Url}", url);
    throw; // Let Repository/UseCase handle
}

// Default fallback
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to fetch entity");
    return default;
}
```

## Rules

1. Always define an interface `I[Name]Service` — never inject the concrete class
2. All methods MUST return `Task<T>` (never `void`)
3. All methods MUST accept `CancellationToken`
4. Use `System.Net.Http.Json` extension methods (`GetFromJsonAsync`, `PostAsJsonAsync`)
5. Implementation is a `sealed class`, never abstract
6. Use `[JsonPropertyName]` on all DTO properties for explicit JSON mapping
7. Never add business logic to services — only HTTP calls and DTO deserialization
8. Services work with DTOs — domain model mapping belongs in the Repository layer

## Naming Conventions

- **Interface**: `I[Name]Service`
- **Implementation**: `[Name]Service`
- **DTOs**: `[Entity]Dto`, `[Entity]CreateDto`, `[Entity]UpdateDto`
- **Methods**: `GetByIdAsync`, `GetAllAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`

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
import CommunityToolkit.Mvvm
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
import CommunityToolkit.Mvvm
import CTApiClient
import AppCommon

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
import CommunityToolkit.Mvvm
import CTApiClient
// Optional: import AppCommon for error handling
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
2. All methods MUST return `Observable<T>` (CommunityToolkit.Mvvm)
3. Always add `.observe(on: MainScheduler.instance)` at the end of each chain
4. Use Target's `.execute()` method for API calls
5. Implementation is a `struct`, not a `class`
6. Never add business logic to services — only data fetch/transform
