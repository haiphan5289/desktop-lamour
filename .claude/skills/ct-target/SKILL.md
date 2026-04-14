---
name: ct-target
description: Generate a typed HttpClient service for a C#/.NET REST API endpoint. Use when adding a new API call. Each HTTP operation is a method on a typed HttpClient service class. Uses System.Net.Http.Json + System.Text.Json for request/response serialization.
---

# WPF API HttpClient Generator

Generate typed HttpClient service for REST API integration.

## Input Format

```
TARGET_NAME: <Name, e.g. "UserProfile">
FEATURE: <Module, e.g. "Features/UserManagement">
OPERATIONS: <comma-separated, e.g. "get,create,update,delete">
ENTITY: <entity name, e.g. "User">
BASE_PATH: <API base path, e.g. "api/v1/users">
```

## Single Operation HttpClient Template

```csharp
// Data/Services/I[Name]Service.cs
namespace App.[Feature].Data.Services;

public interface I[Name]Service
{
    Task<[ResponseType]?> [Operation][Entity]Async([InputType] input, CancellationToken cancellationToken = default);
}
```

```csharp
// Data/Services/[Name]Service.cs
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

    public async Task<[ResponseType]?> [Operation][Entity]Async([InputType] input, CancellationToken cancellationToken = default)
    {
        // GET example
        // return await _httpClient.GetFromJsonAsync<[ResponseType]>($"api/[entity]/{input}", _jsonOptions, cancellationToken);

        // POST example
        // var response = await _httpClient.PostAsJsonAsync("api/[entity]", input, cancellationToken);
        // response.EnsureSuccessStatusCode();
        // return await response.Content.ReadFromJsonAsync<[ResponseType]>(_jsonOptions, cancellationToken);

        throw new NotImplementedException();
    }
}
```

## Multiple Operations Template

```csharp
// Data/Services/[Name]Service.cs
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace App.[Feature].Data.Services;

public sealed class [Name]Service : I[Name]Service
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<[Name]Service> _logger;
    private const string BasePath = "[BASE_PATH]";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public [Name]Service(HttpClient httpClient, ILogger<[Name]Service> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    // GET /[BASE_PATH]/{id}
    public async Task<[Entity]Dto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => await _httpClient.GetFromJsonAsync<[Entity]Dto>($"{BasePath}/{id}", _jsonOptions, cancellationToken);

    // GET /[BASE_PATH]?page=N&limit=N
    public async Task<PagedResultDto<[Entity]Dto>> GetPagedAsync(int page, int limit, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<PagedResultDto<[Entity]Dto>>(
            $"{BasePath}?page={page}&limit={limit}", _jsonOptions, cancellationToken);
        return result ?? new PagedResultDto<[Entity]Dto>();
    }

    // POST /[BASE_PATH]
    public async Task<[Entity]Dto> CreateAsync([Entity]CreateDto request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath, request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<[Entity]Dto>(_jsonOptions, cancellationToken))!;
    }

    // PUT /[BASE_PATH]/{id}
    public async Task<[Entity]Dto> UpdateAsync(string id, [Entity]UpdateDto request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"{BasePath}/{id}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<[Entity]Dto>(_jsonOptions, cancellationToken))!;
    }

    // DELETE /[BASE_PATH]/{id}
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"{BasePath}/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
```

## DTO Models

```csharp
// Data/Dtos/[Entity]Dtos.cs
using System.Text.Json.Serialization;

namespace App.[Feature].Data.Dtos;

public sealed record [Entity]Dto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record [Entity]CreateDto
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}

public sealed record [Entity]UpdateDto
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }
}

public sealed record PagedResultDto<T>
{
    [JsonPropertyName("items")]
    public IReadOnlyList<T> Items { get; init; } = [];

    [JsonPropertyName("total")]
    public int Total { get; init; }

    [JsonPropertyName("page")]
    public int Page { get; init; }
}
```

## DI Registration

```csharp
// In ServiceCollectionExtensions.cs
services.AddHttpClient<I[Name]Service, [Name]Service>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

## Rules

1. All service classes are `sealed` — no inheritance
2. All methods return `Task<T>` and accept `CancellationToken`
3. Use `System.Net.Http.Json` extension methods — not manual JSON serialization
4. Always use `[JsonPropertyName]` on DTO properties for explicit mapping
5. Use `EnsureSuccessStatusCode()` on write operations; let exceptions propagate to Repository
6. No business logic in services — only HTTP calls and DTO deserialization
7. `BasePath` is a `const string` — not hardcoded inline in each method
8. Register via `AddHttpClient<I[Name]Service, [Name]Service>()` — typed HttpClient pattern

## Input Format

```
TARGET_NAME: <Name, e.g. "UserProfile">
FEATURE: <Module, e.g. "CTUserManagement">
OPERATIONS: <comma-separated, e.g. "get,create,update,delete">
ENTITY: <entity name, e.g. "User">
```

## Single Operation Target Template

```swift
import Foundation
import Action
import Alamofire
import ObjectMapper
import AppCommon
import CTApiClient

struct [Name]Target {
    typealias HTTPMethod = Alamofire.HTTPMethod
    typealias Parameters = Alamofire.Parameters

    struct [Operation]Target: Requestable {
        typealias Output = [ResponseType]?

        // let someID: String
        // let someData: SomeModel

        var httpMethod: HTTPMethod {
            .post // or .get, .put, .delete
        }

        var params: Parameters {
            var params: Parameters = [:]
            // params["key"] = value
            return params
        }

        var additionalHeaders: Alamofire.HTTPHeaders {
            HTTPConstants.HTTPAcceptHeaders.V1.plain
        }

        var endpoint: String {
            "api-endpoint/path"
        }

        func decode(data: Any) -> Output {
            Mapper<[WrapperType]<[ResponseType]>>()
                .map(JSONObject: data)?.data
        }
    }
}
```

## Multiple Operations Template

```swift
import Foundation
import Action
import Alamofire
import ObjectMapper
import AppCommon
import CTApiClient

struct [Name]Target {
    typealias HTTPMethod = Alamofire.HTTPMethod
    typealias Parameters = Alamofire.Parameters

    struct Get[Entity]Target: Requestable {
        typealias Output = [Entity]?

        let entityID: String

        var httpMethod: HTTPMethod { .get }

        var params: Parameters {
            ["id": entityID]
        }

        var additionalHeaders: Alamofire.HTTPHeaders {
            HTTPConstants.HTTPAcceptHeaders.V1.plain
        }

        var endpoint: String {
            "api/[entity]/\(entityID)"
        }

        func decode(data: Any) -> Output {
            Mapper<[WrapperType]<[Entity]>>()
                .map(JSONObject: data)?.data
        }
    }

    struct Create[Entity]Target: Requestable {
        typealias Output = [Entity]?

        let entityData: [Entity]CreateParams

        var httpMethod: HTTPMethod { .post }

        var params: Parameters {
            entityData.toJSON()
        }

        var additionalHeaders: Alamofire.HTTPHeaders {
            HTTPConstants.HTTPAcceptHeaders.V1.plain
        }

        var endpoint: String {
            "api/[entity]"
        }

        func decode(data: Any) -> Output {
            Mapper<[WrapperType]<[Entity]>>()
                .map(JSONObject: data)?.data
        }
    }
}
```

## Common Parameter Patterns

```swift
// GET with optional query params
var params: Parameters {
    var params: Parameters = [:]
    if let filterValue = filterValue {
        params["filter"] = filterValue
    }
    params["page"] = page
    params["limit"] = limit
    return params
}

// POST with optional fields
var params: Parameters {
    var params: Parameters = [:]
    params["owner"] = (UserManager.shared().getUserInfo()?.accountId ?? 0).stringValue
    if let fileID = fileID, !fileID.isEmpty {
        params["file_id"] = fileID
    }
    return params
}

// POST from Mappable model
var params: Parameters {
    entityData.toJSON()
}
```

## Rules

1. All targets are nested structs inside a container `struct [Name]Target`
2. Each operation struct conforms to `Requestable`
3. `Output` typealias is always Optional: `[ResponseType]?`
4. `decode(data:)` uses `Mapper<WrapperType<ResponseType>>().map(JSONObject: data)?.data`
5. `additionalHeaders` defaults to `HTTPConstants.HTTPAcceptHeaders.V1.plain` unless specified
6. No business logic in targets — only HTTP method, params, endpoint, decode
7. Endpoints use string literals (not `Api.*` key lookup — that's added in NetworkHelper separately)
