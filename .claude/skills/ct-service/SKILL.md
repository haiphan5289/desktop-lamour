---
name: ct-service
description: Generate a C# Service (interface + implementation) for Desktop Lamour. Interface named I[Name]Service, implementation [Name]Service. Constructor injects HttpClient. Methods call REST API via HttpClient using System.Net.Http.Json extensions and return deserialized DTOs.
model: haiku
effort: low
---

# C# Service Generator for Desktop Lamour

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Generate Service interface and implementation following Clean Architecture. Services are the outermost data layer — they call the REST API via HttpClient and return raw DTOs.

## Input Format

```
SERVICE_NAME: <Name, e.g. "Employee">
MODULE: <Module name, e.g. "Employees">
ENDPOINTS: <comma-separated endpoint descriptions, e.g. "GET /api/employees, POST /api/employees, PUT /api/employees/{id}, DELETE /api/employees/{id}">
```

## Service Template

```csharp
// File: src/DesktopLamour/Features/[Module]/Data/I[Name]Service.cs
using DesktopLamour.Features.[Module].Data.DTOs;

namespace DesktopLamour.Features.[Module].Data;

public interface I[Name]Service
{
    Task<IEnumerable<[Name]Dto>> GetAllAsync(CancellationToken ct = default);
    Task<[Name]Dto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<[Name]Dto> CreateAsync([Name]Dto dto, CancellationToken ct = default);
    Task<[Name]Dto> UpdateAsync(int id, [Name]Dto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
```

```csharp
// File: src/DesktopLamour/Features/[Module]/Data/[Name]Service.cs
using System.Net.Http.Json;
using DesktopLamour.Features.[Module].Data.DTOs;

namespace DesktopLamour.Features.[Module].Data;

public class [Name]Service : I[Name]Service
{
    private readonly HttpClient _httpClient;

    public [Name]Service(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<[Name]Dto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<[Name]Dto>>(
            "/api/[endpoint]", ct)
               ?? Enumerable.Empty<[Name]Dto>();
    }

    public async Task<[Name]Dto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<[Name]Dto>(
            $"/api/[endpoint]/{id}", ct);
    }

    public async Task<[Name]Dto> CreateAsync([Name]Dto dto, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/[endpoint]", dto, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<[Name]Dto>(ct)
               ?? throw new InvalidOperationException("No response from server.");
    }

    public async Task<[Name]Dto> UpdateAsync(int id, [Name]Dto dto, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/[endpoint]/{id}", dto, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<[Name]Dto>(ct)
               ?? throw new InvalidOperationException("No response from server.");
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"/api/[endpoint]/{id}", ct);
        response.EnsureSuccessStatusCode();
    }
}
```

## HttpClient Registration

Services with HttpClient must be registered via `AddHttpClient<>` in the DI extension:

```csharp
// In [Module]ServiceExtensions.cs
services.AddHttpClient<I[Name]Service, [Name]Service>(client =>
{
    client.BaseAddress = new Uri(configuration["ApiBaseUrl"]!);
});
```

## HTTP Method Reference

| Operation | Method | Extension |
|---|---|---|
| Fetch list | GET | `GetFromJsonAsync<T>()` |
| Fetch single | GET | `GetFromJsonAsync<T?>()` |
| Create | POST | `PostAsJsonAsync()` + `ReadFromJsonAsync<T>()` |
| Update | PUT | `PutAsJsonAsync()` + `ReadFromJsonAsync<T>()` |
| Partial update | PATCH | `PatchAsJsonAsync()` + `ReadFromJsonAsync<T>()` |
| Delete | DELETE | `DeleteAsync()` + `EnsureSuccessStatusCode()` |

## Rules

1. Interface named `I[Name]Service` — always define interface, never concrete-only
2. Implementation class `[Name]Service`
3. All methods are `async Task<T>` with `CancellationToken ct = default`
4. Constructor injects `HttpClient` — registered via `AddHttpClient<>`
5. Use `System.Net.Http.Json` extension methods — never `JsonConvert` or manual deserialization
6. Call `EnsureSuccessStatusCode()` on mutating operations (POST/PUT/DELETE)
7. Return type is always DTO, never domain model
8. No business logic in services — only HTTP calls and deserialization
9. Namespace: `DesktopLamour.Features.[Module].Data`
