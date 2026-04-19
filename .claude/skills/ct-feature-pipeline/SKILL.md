---
name: ct-feature-pipeline
description: End-to-end feature pipeline for Desktop Lamour. Single input (feature description + module) auto-runs phases in sequence: ct-flipped-interaction (gather requirements) → ct-generate-usecase (5-layer implementation) → ct-unittest (xUnit tests) → review-code (review). Reference: docs/project-overview.md.
model: sonnet
effort: high
---

# Desktop Lamour Feature Pipeline

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Single-entry orchestrator. Input once — output: clarified requirements + 5-layer UseCase implementation + ViewModel wiring + xUnit tests + review report.

---

## Input Format

```
FEATURE: <one-line description>
MODULE: <Authentication | Employees | Inventory | ImportInvoices | ExportInvoices>
API_ENDPOINT: <optional — e.g. POST /api/employees>
NOTES: <optional — any known constraints>
```

---

## Phase Execution

### Phase 1 — Gather Requirements (ct-flipped-interaction)

Ask clarifying questions before generating any code:

**Scope**
- Which module? What entity is affected?
- Is this a new module or extending an existing one?

**API Contract**
- HTTP method + endpoint path?
- Request body fields (name, type, required)?
- Response shape (single object or array)?
- Error responses (400/404/409 shapes)?

**Business Rules**
- Stock never negative (Inventory/ExportInvoices)
- Invoice immutable after confirmation (ImportInvoices/ExportInvoices)
- Admin-only operations?
- Any uniqueness constraints?

**UI & ViewModel**
- What data does the View need to display?
- Which user actions trigger commands?
- Loading/error/empty states needed?

**Testing**
- What are the success and failure edge cases?
- Which business rules must have test coverage?

Output a confirmed summary before proceeding:

```
MODULE: [module name]
ENTITY: [domain model name]
API: [METHOD /path]
REQUEST: [fields]
RESPONSE: [fields]
BUSINESS_RULES: [list]
VIEW_STATE: [loading, error, items, selectedItem]
COMMANDS: [Load, Create, Delete, etc.]
```

---

### Phase 2 — 5-Layer Implementation (ct-generate-usecase)

Generate all 5 layers in order:

**Layer 1 — Domain**

```csharp
// src/DesktopLamour/Features/[Module]/Domain/Models/[Entity].cs
public record [Entity](
    int Id,
    string Name,
    // ... domain fields
);

// src/DesktopLamour/Features/[Module]/Domain/UseCases/I[Feature]UseCase.cs
public interface I[Feature]UseCase
{
    Task<[Result]> ExecuteAsync([Input] input, CancellationToken ct = default);
}

// src/DesktopLamour/Features/[Module]/Domain/UseCases/[Feature]UseCase.cs
public class [Feature]UseCase : I[Feature]UseCase
{
    private readonly I[Entity]Repository _repository;

    public [Feature]UseCase(I[Entity]Repository repository)
        => _repository = repository;

    public async Task<[Result]> ExecuteAsync([Input] input, CancellationToken ct = default)
    {
        // business rule validation here
        return await _repository.[Operation]Async(input, ct);
    }
}
```

**Layer 2 — Repository**

```csharp
// src/DesktopLamour/Features/[Module]/Data/Repositories/I[Entity]Repository.cs
public interface I[Entity]Repository
{
    Task<IReadOnlyList<[Entity]>> GetAllAsync(CancellationToken ct = default);
    Task<[Entity]> [Operation]Async([Input] input, CancellationToken ct = default);
}

// src/DesktopLamour/Features/[Module]/Data/Repositories/[Entity]Repository.cs
public class [Entity]Repository : I[Entity]Repository
{
    private readonly I[Entity]Service _service;

    public [Entity]Repository(I[Entity]Service service)
        => _service = service;

    public async Task<IReadOnlyList<[Entity]>> GetAllAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetAllAsync(ct);
        return dtos.Select(MapToDomain).ToList();
    }

    private static [Entity] MapToDomain([Entity]Dto dto)
        => new(dto.Id, dto.Name);
}
```

**Layer 3 — Service + DTOs**

```csharp
// src/DesktopLamour/Features/[Module]/Data/Services/DTOs/[Entity]Dto.cs
public record [Entity]Dto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name
);

// src/DesktopLamour/Features/[Module]/Data/Services/I[Entity]Service.cs
public interface I[Entity]Service
{
    Task<IReadOnlyList<[Entity]Dto>> GetAllAsync(CancellationToken ct = default);
}

// src/DesktopLamour/Features/[Module]/Data/Services/[Entity]Service.cs
public class [Entity]Service : I[Entity]Service
{
    private readonly HttpClient _http;

    public [Entity]Service(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<[Entity]Dto>> GetAllAsync(CancellationToken ct = default)
    {
        var result = await _http.GetFromJsonAsync<List<[Entity]Dto>>("/api/[entities]", ct);
        return result ?? [];
    }
}
```

**Layer 4 — ViewModel**

```csharp
// src/DesktopLamour/Features/[Module]/ViewModels/[Name]ViewModel.cs
public partial class [Name]ViewModel : ObservableObject
{
    private readonly I[Feature]UseCase _useCase;

    public [Name]ViewModel(I[Feature]UseCase useCase)
        => _useCase = useCase;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;
    public ObservableCollection<[Entity]> Items { get; } = [];

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct = default)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _useCase.ExecuteAsync(ct);
            Items.Clear();
            foreach (var item in items) Items.Add(item);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsLoading = false; }
    }
}
```

**Layer 5 — DI Registration**

```csharp
// src/DesktopLamour/Features/[Module]/[Module]ServiceExtensions.cs
public static class [Module]ServiceExtensions
{
    public static IServiceCollection Add[Module]Services(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<I[Entity]Service, [Entity]Service>(client =>
            client.BaseAddress = new Uri(configuration["Api:BaseUrl"]!));

        services.AddScoped<I[Entity]Repository, [Entity]Repository>();
        services.AddScoped<I[Feature]UseCase, [Feature]UseCase>();
        services.AddTransient<[Name]ViewModel>();
        return services;
    }
}
```

---

### Phase 3 — Unit Tests (ct-unittest)

Generate xUnit + Moq tests for UseCase and ViewModel:

```csharp
// tests/DesktopLamour.Tests/Features/[Module]/[Feature]UseCaseTests.cs
public class [Feature]UseCaseTests
{
    private readonly Mock<I[Entity]Repository> _repositoryMock = new();
    private readonly [Feature]UseCase _sut;

    public [Feature]UseCaseTests()
        => _sut = new [Feature]UseCase(_repositoryMock.Object);

    [Fact]
    public async Task ExecuteAsync_ReturnsItems_WhenRepositorySucceeds()
    {
        // Arrange
        var expected = new List<[Entity]> { new(1, "Test") };
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ExecuteAsync_Propagates_RepositoryException()
    {
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        await Assert.ThrowsAsync<HttpRequestException>(() => _sut.ExecuteAsync());
    }
}
```

---

### Phase 4 — Code Review (review-code)

Review against Desktop Lamour standards:

**Architecture**
- [ ] 5-layer separation respected (no cross-layer leakage)
- [ ] No `new SomeViewModel()` — DI only
- [ ] Domain model has no `using System.Net.Http` references

**ViewModel**
- [ ] `partial class` for CommunityToolkit source generators
- [ ] `[ObservableProperty]` fields use `_camelCase` prefix
- [ ] `ObservableCollection<T>` not `List<T>`
- [ ] `finally { IsLoading = false; }` always present
- [ ] `OperationCanceledException` caught separately

**XAML**
- [ ] No inline styles (FontSize, Background, Foreground)
- [ ] `UpdateSourceTrigger=PropertyChanged` on all two-way TextBox bindings
- [ ] All `StaticResource` keys verified to exist in AppStyles.xaml or AppTypography.xaml
- [ ] Command bindings use generated name (`MethodNameCommand`)

**Business Rules**
- [ ] Stock never goes negative (ExportInvoices)
- [ ] Invoice confirmed = immutable (ImportInvoices/ExportInvoices)
- [ ] Role-based access checks in place if needed

---

## Output Summary

After all phases complete, provide:

```
PIPELINE COMPLETE — [Feature Name]

Files created:
- src/DesktopLamour/Features/[Module]/Domain/Models/[Entity].cs
- src/DesktopLamour/Features/[Module]/Domain/UseCases/I[Feature]UseCase.cs
- src/DesktopLamour/Features/[Module]/Domain/UseCases/[Feature]UseCase.cs
- src/DesktopLamour/Features/[Module]/Data/Repositories/I[Entity]Repository.cs
- src/DesktopLamour/Features/[Module]/Data/Repositories/[Entity]Repository.cs
- src/DesktopLamour/Features/[Module]/Data/Services/I[Entity]Service.cs
- src/DesktopLamour/Features/[Module]/Data/Services/[Entity]Service.cs
- src/DesktopLamour/Features/[Module]/Data/Services/DTOs/[Entity]Dto.cs
- src/DesktopLamour/Features/[Module]/ViewModels/[Name]ViewModel.cs
- src/DesktopLamour/Features/[Module]/[Module]ServiceExtensions.cs
- tests/DesktopLamour.Tests/Features/[Module]/[Feature]UseCaseTests.cs

Review: [APPROVED | NEEDS WORK]
```

See `docs/project-overview.md` for business domain context.
