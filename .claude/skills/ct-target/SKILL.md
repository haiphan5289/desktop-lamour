---
name: ct-target
description: Generate an HttpClient API endpoint helper for Desktop Lamour. Creates strongly-typed request/response DTOs and the Service method that calls the endpoint using System.Net.Http.Json extensions. Use when adding a new API endpoint to an existing Service.
model: haiku
effort: low
---

# HttpClient Endpoint Helper Generator

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Generate strongly-typed DTOs and HttpClient service methods for a new API endpoint in Desktop Lamour.

## Input Format

```
ENDPOINT_PATH: <e.g. /api/inventory/products>
HTTP_METHOD: <GET | POST | PUT | PATCH | DELETE>
REQUEST_DTO: <e.g. CreateProductRequest | void>
RESPONSE_DTO: <e.g. ProductDto | IEnumerable<ProductDto>>
SERVICE_NAME: <e.g. Inventory>
```

---

## Request DTO Template

```csharp
// File: src/DesktopLamour/Features/[Module]/Data/DTOs/Create[Entity]Request.cs
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.[Module].Data.DTOs;

public class Create[Entity]Request
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("unit_price")]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("initial_stock")]
    public int InitialStock { get; set; }

    // Add fields matching the API contract
}
```

## Response DTO Template

```csharp
// File: src/DesktopLamour/Features/[Module]/Data/DTOs/[Entity]Dto.cs
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.[Module].Data.DTOs;

public class [Entity]Dto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("unit_price")]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("stock")]
    public int Stock { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
```

---

## Service Interface Method

```csharp
// ADD to I[ServiceName]Service.cs interface:

// GET list
Task<IEnumerable<[Entity]Dto>> Get[Entities]Async(CancellationToken ct = default);

// GET single
Task<[Entity]Dto?> Get[Entity]ByIdAsync(int id, CancellationToken ct = default);

// POST create
Task<[Entity]Dto> Create[Entity]Async(Create[Entity]Request request, CancellationToken ct = default);

// PUT update
Task<[Entity]Dto> Update[Entity]Async(int id, Update[Entity]Request request, CancellationToken ct = default);

// DELETE
Task Delete[Entity]Async(int id, CancellationToken ct = default);
```

---

## Service Implementation Methods

```csharp
// ADD to [ServiceName]Service.cs implementation:
// using System.Net.Http.Json;

// GET list
public async Task<IEnumerable<[Entity]Dto>> Get[Entities]Async(CancellationToken ct = default)
{
    return await _httpClient.GetFromJsonAsync<IEnumerable<[Entity]Dto>>(
        "/api/[endpoint]", ct)
           ?? Enumerable.Empty<[Entity]Dto>();
}

// GET single
public async Task<[Entity]Dto?> Get[Entity]ByIdAsync(int id, CancellationToken ct = default)
{
    return await _httpClient.GetFromJsonAsync<[Entity]Dto>(
        $"/api/[endpoint]/{id}", ct);
}

// POST create
public async Task<[Entity]Dto> Create[Entity]Async(
    Create[Entity]Request request, CancellationToken ct = default)
{
    var response = await _httpClient.PostAsJsonAsync("/api/[endpoint]", request, ct);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<[Entity]Dto>(ct)
           ?? throw new InvalidOperationException("Server returned no content.");
}

// PUT update
public async Task<[Entity]Dto> Update[Entity]Async(
    int id, Update[Entity]Request request, CancellationToken ct = default)
{
    var response = await _httpClient.PutAsJsonAsync($"/api/[endpoint]/{id}", request, ct);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<[Entity]Dto>(ct)
           ?? throw new InvalidOperationException("Server returned no content.");
}

// DELETE
public async Task Delete[Entity]Async(int id, CancellationToken ct = default)
{
    var response = await _httpClient.DeleteAsync($"/api/[endpoint]/{id}", ct);
    response.EnsureSuccessStatusCode();
}
```

---

## JSON Naming Conventions

| JSON key (snake_case) | C# property (PascalCase) | JsonPropertyName |
|---|---|---|
| `full_name` | `FullName` | `[JsonPropertyName("full_name")]` |
| `unit_price` | `UnitPrice` | `[JsonPropertyName("unit_price")]` |
| `created_at` | `CreatedAt` | `[JsonPropertyName("created_at")]` |
| `is_active` | `IsActive` | `[JsonPropertyName("is_active")]` |
| `phone_number` | `PhoneNumber` | `[JsonPropertyName("phone_number")]` |

---

## Rules

1. All DTO properties use `[JsonPropertyName]` with snake_case keys
2. Use `System.Net.Http.Json` extension methods — never `JsonConvert` or manual JSON
3. Call `EnsureSuccessStatusCode()` on POST/PUT/PATCH/DELETE
4. GET endpoints return `null`-safe results (use `?? Enumerable.Empty<T>()`)
5. Never put DTOs in the Domain layer — they belong in `Data/DTOs/`
6. Request DTOs are separate classes from Response DTOs
7. All methods accept `CancellationToken ct = default`
