# Desktop Lamour — Claude Code Guide

> Full project spec: `docs/project-overview.md`

## Feature Docs (REQUIRED before implement or debug)

Each feature has a dedicated doc. **Always read the feature doc before implementing or debugging.**

| Feature | Doc path |
|---------|----------|
| Sales / Chứng từ bán hàng | `src/DesktopLamour/Features/HomePage/Sales/docs/sales.md` |
| Login | `docs/login-view.md` |

> For any other feature, check `src/DesktopLamour/Features/[Module]/docs/` first.

---

## Agent Routing

When a task matches a domain below, **spawn the appropriate agent** via the Agent tool before responding directly. Always prefer agent delegation over inline answers for non-trivial tasks.

| Task type | Agent to invoke | Trigger keywords |
|---|---|---|
| Implement feature, fix bug, scaffold layers, wire UseCase | `lamour-wpf-expert` | implement, add feature, usecase, repository, service, viewmodel, bug, crash, error, exception |
| XAML styling, AppStyles, AppTypography, ResourceDictionary | `lamour-xaml-design-expert` | xaml, style, color, font, spacing, DataTrigger, converter, resource key |
| Business rules, domain models, invoice logic, stock, VAT | `lamour-domain-expert` | business rule, domain, inventory, invoice, stock, employee, role, supplier, VAT, validate |
| Module navigation, architecture, file structure, DI context | `lamour-module-context-expert` | module, architecture, structure, folder, which layer, navigate code |
| UI spec → XAML, wireframe, screen design, new View | `lamour-ui-implementer` | design, wireframe, screen, figma, new view, UI spec, layout |

## Project Stack

- **Platform**: .NET 8, WPF, Windows (x64/ARM64)
- **MVVM**: CommunityToolkit.Mvvm 8.3.2 (`[ObservableProperty]`, `[RelayCommand]`, `ObservableObject`)
- **DI**: `Microsoft.Extensions.DependencyInjection` — constructor injection only
- **HTTP**: `Microsoft.Extensions.Http` — `HttpClient` via `AddHttpClient<TInterface, TImpl>()`
- **Tests**: xUnit + Moq

## Architecture (5 layers, strictly separated)

```
View (XAML)  ←→  ViewModel  ←→  IUseCase  ←→  IRepository  ←→  IService → API
```

- Domain layer (Models + IUseCase + UseCase) has **zero** external dependencies
- All cross-layer communication through **interfaces**
- No `new` for Services/Repositories/UseCases — always DI

## Module Structure

```
Features/[Module]/
├── Domain/Models/          # Pure C# entities
├── Domain/UseCases/        # I[Name]UseCase.cs + [Name]UseCase.cs
├── Data/Repositories/      # I[Name]Repository.cs + [Name]Repository.cs
├── Data/Services/          # I[Name]Service.cs + [Name]Service.cs
│   └── Dtos/               # [Action]RequestDto.cs + [Action]ResponseDto.cs
├── Views/                  # [Feature]View.xaml + .xaml.cs
├── ViewModels/             # [Feature]ViewModel.cs
└── [Module]ServiceCollectionExtensions.cs
```

## Business Domains

- **Authentication** — phone-based sign up/login
- **Employees** — staff profiles, roles (Admin / Cashier / Warehouse)
- **Inventory** — cosmetics products, stock levels, low-stock alerts
- **ImportInvoices** — purchase from suppliers → increases stock
- **ExportInvoices** — sales to customers → decreases stock, VAT 10%

## Mandatory Rules

1. `[ObservableProperty]` on backing fields (camelCase `_field`), never manual `OnPropertyChanged`
2. `[RelayCommand]` on `async Task` methods — generates `*Command` property automatically
3. All async public methods accept `CancellationToken ct = default`
4. Never `.Result` or `.Wait()` — always `await`
5. XAML: always `Style="{StaticResource ...}"` — no inline `Foreground`, `FontSize`, `Background`
6. Confirmed invoices are immutable — only cancellation allowed
7. Stock never goes negative — validate before confirming export invoice
