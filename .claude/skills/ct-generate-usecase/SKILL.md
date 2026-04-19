---
name: ct-generate-usecase
description: Auto-generate and wire a new UseCase across all 5 layers of Desktop Lamour — add IUseCase interface + UseCase class (Domain), update IRepository + Repository (Data), add IService method + Service implementation + DTOs (Data), update ViewModel with [RelayCommand], register DI in ServiceCollectionExtensions. Modifies only existing files where possible.
model: sonnet
effort: high
---

# Auto-Generate UseCase — 5-Layer Implementation

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Auto-generate and wire a UseCase across all 5 architecture layers by modifying existing files where they exist, creating new files only when necessary.

---

## Input Format

```
USECASE_NAME:    <e.g. GetInventoryItems>
MODULE:          <e.g. Inventory>
INPUT_MODEL:     <e.g. GetInventoryRequest | void>
OUTPUT_MODEL:    <e.g. IEnumerable<InventoryItem>>
API_ENDPOINT:    <e.g. /api/inventory>
HTTP_METHOD:     <GET | POST | PUT | DELETE>
VIEWMODEL_FILE:  <e.g. InventoryListViewModel.cs>
```

---

## Pre-Generation Verification

Before generating, verify:

```
1. Glob for I[Module]Repository.cs — extract existing interface methods
2. Glob for I[Module]Service.cs — extract existing interface methods
3. Read VIEWMODEL_FILE — find existing [ObservableProperty] fields and [RelayCommand] methods
4. Grep for ServiceCollectionExtensions in the module — find DI registration file
5. Glob for existing DTOs in Data/DTOs/ folder
```

Show confirmation table before generating:

```
MODULE:              Inventory
DOMAIN_FILE:         Features/Inventory/Domain/IGetInventoryItemsUseCase.cs  (NEW)
REPOSITORY_FILE:     Features/Inventory/Data/IInventoryRepository.cs         (MODIFY)
SERVICE_FILE:        Features/Inventory/Data/IInventoryService.cs            (MODIFY)
DTO_FILE:            Features/Inventory/Data/DTOs/InventoryItemDto.cs        (NEW)
VIEWMODEL_FILE:      Features/Inventory/ViewModels/InventoryListViewModel.cs (MODIFY)
DI_FILE:             Features/Inventory/InventoryServiceExtensions.cs        (MODIFY)

Proceed? (user may override)
```

---

## Architecture Flow

```
ViewModel ([RelayCommand] method)
    ↓ calls
IGetInventoryItemsUseCase.ExecuteAsync()
    ↓ calls
IInventoryRepository.GetAllAsync()
    ↓ calls
IInventoryService.GetAllAsync()
    ↓ HttpClient.GetFromJsonAsync<IEnumerable<InventoryItemDto>>()
```

---

## Step 1 — Domain: UseCase Interface + Implementation

```csharp
// NEW: Features/Inventory/Domain/IGetInventoryItemsUseCase.cs
namespace DesktopLamour.Features.Inventory.Domain;

public interface IGetInventoryItemsUseCase
{
    Task<IEnumerable<InventoryItem>> ExecuteAsync(CancellationToken ct = default);
}

// NEW: Features/Inventory/Domain/GetInventoryItemsUseCase.cs
namespace DesktopLamour.Features.Inventory.Domain;

public class GetInventoryItemsUseCase : IGetInventoryItemsUseCase
{
    private readonly IInventoryRepository _repository;

    public GetInventoryItemsUseCase(IInventoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<InventoryItem>> ExecuteAsync(CancellationToken ct = default)
    {
        return await _repository.GetAllAsync(ct);
    }
}
```

---

## Step 2 — Data: Repository Interface + Implementation

Add to **existing** `IInventoryRepository.cs` and `InventoryRepository.cs`:

```csharp
// ADD to IInventoryRepository interface:
Task<IEnumerable<InventoryItem>> GetAllAsync(CancellationToken ct = default);

// ADD to InventoryRepository implementation:
public async Task<IEnumerable<InventoryItem>> GetAllAsync(CancellationToken ct = default)
{
    var dtos = await _service.GetAllAsync(ct);
    return dtos.Select(d => new InventoryItem
    {
        Id = d.Id,
        Name = d.Name,
        Stock = d.Stock,
        UnitPrice = d.UnitPrice
    });
}
```

---

## Step 3 — Data: Service Interface + Implementation + DTO

Add to **existing** `IInventoryService.cs` and `InventoryService.cs`:

```csharp
// ADD to IInventoryService interface:
Task<IEnumerable<InventoryItemDto>> GetAllAsync(CancellationToken ct = default);

// ADD to InventoryService implementation:
public async Task<IEnumerable<InventoryItemDto>> GetAllAsync(CancellationToken ct = default)
{
    return await _httpClient.GetFromJsonAsync<IEnumerable<InventoryItemDto>>(
        "/api/inventory", ct) ?? Enumerable.Empty<InventoryItemDto>();
}
```

```csharp
// NEW: Features/Inventory/Data/DTOs/InventoryItemDto.cs
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.Inventory.Data.DTOs;

public class InventoryItemDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("stock")]
    public int Stock { get; set; }

    [JsonPropertyName("unit_price")]
    public decimal UnitPrice { get; set; }
}
```

---

## Step 4 — Presentation: Update ViewModel

Add to **existing** `InventoryListViewModel.cs`:

```csharp
// ADD dependency to constructor:
private readonly IGetInventoryItemsUseCase _getInventoryItemsUseCase;

// ADD to constructor parameter list:
public InventoryListViewModel(
    IGetInventoryItemsUseCase getInventoryItemsUseCase)
{
    _getInventoryItemsUseCase = getInventoryItemsUseCase;
}

// ADD ObservableCollection property:
public ObservableCollection<InventoryItem> Items { get; } = new();

// ADD [RelayCommand] method:
[RelayCommand]
private async Task LoadItemsAsync(CancellationToken ct = default)
{
    try
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        var items = await _getInventoryItemsUseCase.ExecuteAsync(ct);
        Items.Clear();
        foreach (var item in items)
            Items.Add(item);
    }
    catch (Exception ex)
    {
        ErrorMessage = ex.Message;
    }
    finally
    {
        IsLoading = false;
    }
}
```

---

## Step 5 — DI Registration

Add to **existing** `InventoryServiceExtensions.cs`:

```csharp
// ADD to AddInventoryModule():
services.AddScoped<IGetInventoryItemsUseCase, GetInventoryItemsUseCase>();
```

---

## Completion Checklist

- [ ] UseCase interface defined in Domain layer
- [ ] UseCase implementation calls Repository
- [ ] Repository interface method added
- [ ] Repository implementation maps DTO → domain model
- [ ] Service interface method added
- [ ] Service implementation calls HttpClient
- [ ] DTO class created with `[JsonPropertyName]` attributes
- [ ] ViewModel updated with new `[RelayCommand]` method
- [ ] ViewModel properly handles `IsLoading`, `ErrorMessage`, and `try/catch/finally`
- [ ] DI registration added for UseCase
- [ ] No `.Result` or `.Wait()` used anywhere
- [ ] All methods accept `CancellationToken ct = default`

See `docs/project-overview.md` for full project context.
