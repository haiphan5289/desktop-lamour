---
name: ct-repository
description: Generate a C# Repository (interface + implementation) for Desktop Lamour. Interface named I[Name]Repository, implementation [Name]Repository. Constructor injects I[Name]Service. Methods are async Task<T> with CancellationToken. Maps DTOs to domain models.
model: haiku
effort: low
---

# C# Repository Generator for Desktop Lamour

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Generate Repository interface and implementation following Clean Architecture.

## Input Format

```
REPOSITORY_NAME: <Name, e.g. "Employee">
MODULE: <Module name, e.g. "Employees">
METHODS: <comma-separated operations, e.g. "GetAll, GetById, Create, Update, Delete">
```

## Repository Template

```csharp
// File: src/DesktopLamour/Features/[Module]/Data/I[Name]Repository.cs
namespace DesktopLamour.Features.[Module].Data;

public interface I[Name]Repository
{
    Task<IEnumerable<[Entity]>> GetAllAsync(CancellationToken ct = default);
    Task<[Entity]?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<[Entity]> CreateAsync([Entity] entity, CancellationToken ct = default);
    Task<[Entity]> UpdateAsync([Entity] entity, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
```

```csharp
// File: src/DesktopLamour/Features/[Module]/Data/[Name]Repository.cs
using DesktopLamour.Features.[Module].Data.DTOs;
using DesktopLamour.Features.[Module].Domain;

namespace DesktopLamour.Features.[Module].Data;

public class [Name]Repository : I[Name]Repository
{
    private readonly I[Name]Service _service;

    public [Name]Repository(I[Name]Service service)
    {
        _service = service;
    }

    public async Task<IEnumerable<[Entity]>> GetAllAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetAllAsync(ct);
        return dtos.Select(MapToDomain);
    }

    public async Task<[Entity]?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var dto = await _service.GetByIdAsync(id, ct);
        return dto is null ? null : MapToDomain(dto);
    }

    public async Task<[Entity]> CreateAsync([Entity] entity, CancellationToken ct = default)
    {
        var dto = MapToDto(entity);
        var created = await _service.CreateAsync(dto, ct);
        return MapToDomain(created);
    }

    public async Task<[Entity]> UpdateAsync([Entity] entity, CancellationToken ct = default)
    {
        var dto = MapToDto(entity);
        var updated = await _service.UpdateAsync(entity.Id, dto, ct);
        return MapToDomain(updated);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
    }

    // MARK: - Private mapping methods

    private static [Entity] MapToDomain([Entity]Dto dto) => new()
    {
        Id = dto.Id,
        // Map other fields
    };

    private static [Entity]Dto MapToDto([Entity] entity) => new()
    {
        Id = entity.Id,
        // Map other fields
    };
}
```

## Rules

1. Interface named `I[Name]Repository` — prefix with `I`
2. Implementation class `[Name]Repository` — no prefix
3. All methods are `async Task<T>` with `CancellationToken ct = default`
4. Constructor injects `I[Name]Service` — never a concrete type
5. Repository maps DTOs to domain models (never leaks DTOs to callers)
6. Mapping methods are `private static` — no mapping logic in interface methods
7. Namespace: `DesktopLamour.Features.[Module].Data`
8. Domain model namespace: `DesktopLamour.Features.[Module].Domain`

## Naming Conventions

| Layer | Pattern | Example |
|---|---|---|
| Interface | `I[Name]Repository` | `IEmployeeRepository` |
| Implementation | `[Name]Repository` | `EmployeeRepository` |
| Service dependency | `I[Name]Service` | `IEmployeeService` |
| DTO type | `[Name]Dto` | `EmployeeDto` |
| Domain type | `[Entity]` | `Employee` |
