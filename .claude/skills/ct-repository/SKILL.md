---
name: ct-repository
description: Generate a C#/.NET Repository (interface + implementation) following Clean Architecture. Repositories abstract data access by delegating to Services/HttpClient. Use when adding a repository layer between UseCase and Service. Named I[Name]Repository (interface) and [Name]Repository (sealed class).
---

# WPF Basic Repository Generator

Generate Repository interface and implementation following Clean Architecture patterns.

## Input Format

```
REPOSITORY_NAME: <Name, e.g. "UserProfile">
FEATURE: <Module, e.g. "Features/UserManagement">
ENTITY: <Entity type, e.g. "UserModel">
OPERATIONS: <comma-separated, e.g. "get,create,update,delete">
```

## Repository Template

```csharp
// Domain layer — I[Name]Repository.cs
using App.[Feature].Domain.Models;

namespace App.[Feature].Domain.Repositories;

public interface I[Name]Repository
{
    // Task<[Entity]?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    // Task<IReadOnlyList<[Entity]>> GetAllAsync(CancellationToken cancellationToken = default);
    // Task<[Entity]> CreateAsync([Entity]CreateRequest request, CancellationToken cancellationToken = default);
    // Task<[Entity]> UpdateAsync(string id, [Entity]UpdateRequest request, CancellationToken cancellationToken = default);
    // Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
```

```csharp
// Data layer — [Name]Repository.cs
using App.[Feature].Domain.Models;
using App.[Feature].Domain.Repositories;
using App.[Feature].Data.Services;
using Microsoft.Extensions.Logging;

namespace App.[Feature].Data.Repositories;

public sealed class [Name]Repository : I[Name]Repository
{
    // #region Dependencies

    private readonly I[Name]Service _service;
    private readonly ILogger<[Name]Repository> _logger;

    // #region Initialization

    public [Name]Repository(I[Name]Service service, ILogger<[Name]Repository> logger)
    {
        _service = service;
        _logger = logger;
    }

    // #region I[Name]Repository

    // public async Task<[Entity]?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    // {
    //     var dto = await _service.GetByIdAsync(id, cancellationToken);
    //     return dto is null ? null : MapToEntity(dto);
    // }

    // public async Task<IReadOnlyList<[Entity]>> GetAllAsync(CancellationToken cancellationToken = default)
    // {
    //     var dtos = await _service.GetAllAsync(cancellationToken);
    //     return dtos.Select(MapToEntity).ToList();
    // }

    // #region Private Mapping

    // private static [Entity] MapToEntity([Entity]Dto dto)
    //     => new [Entity]
    //     {
    //         Id = dto.Id,
    //         Name = dto.Name,
    //         // ... map other fields
    //     };
}
```

## Rules

1. Interface named `I[Name]Repository` — registered in DI as interface
2. Implementation is a `sealed class` — no inheritance needed
3. Repository methods pass through to service with DTO → domain model mapping
4. All methods are `async Task<T>` and accept `CancellationToken`
5. Map service DTOs to domain models inside the repository (DTOs stay in Data layer)
6. Inject `I[Name]Service` through constructor (never concrete type)
7. Never expose DTOs to the Domain or Presentation layers

## Naming Conventions

- **Interface**: `I[Name]Repository`
- **Implementation**: `[Name]Repository`
- **Service dependency**: `I[Name]Service` (injected via constructor)
- **Methods**: descriptive — `GetByIdAsync`, `GetAllAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`

## DI Registration

```csharp
// In ServiceCollectionExtensions.cs
services.AddScoped<I[Name]Repository, [Name]Repository>();
```
