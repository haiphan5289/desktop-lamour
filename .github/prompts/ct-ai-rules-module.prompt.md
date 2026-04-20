---
description: "Generate a complete Desktop Lamour feature module — all 5 layers + DI registration."
mode: "agent"
---

# WPF Feature Module Generator — Desktop Lamour

Generate a full feature module following MVVM + Clean Architecture.

## Input

```
MODULE_NAME: <e.g. Employees>
FEATURE:     <e.g. List employees with search>
API:         <GET /api/employees>
```

## Module Structure

```
Features/[Module]/
├── Domain/
│   ├── Models/           [Module]Model.cs
│   └── UseCases/         I[Feature]UseCase.cs + [Feature]UseCase.cs
├── Data/
│   ├── Repositories/     I[Feature]Repository.cs + [Feature]Repository.cs
│   └── Services/         I[Feature]Service.cs + [Feature]Service.cs
│       └── Dtos/         [Feature]RequestDto.cs + [Feature]ResponseDto.cs
├── Views/                [Feature]View.xaml + .xaml.cs
├── ViewModels/           [Feature]ViewModel.cs
└── [Module]ServiceCollectionExtensions.cs
```

## Generation Order

1. **Domain Model** — pure C# entity, no external dependencies
2. **DTOs** — Request/Response records
3. **IService + Service** — HttpClient call, returns DTO
4. **IRepository + Repository** — calls Service, maps DTO → Model
5. **IUseCase + UseCase** — business logic, calls Repository
6. **ViewModel** — `[ObservableProperty]` + `[RelayCommand]`, calls UseCase
7. **View XAML** — binds to ViewModel, uses AppButton/AppLabel/AppTextField
8. **DI Registration** — `[Module]ServiceCollectionExtensions.cs`

## Business Rules to Enforce

- Stock never goes negative (Inventory / ExportInvoices)
- Confirmed invoices are immutable — cancellation only
- Role check: Admin can do all; Cashier handles sales; Warehouse handles stock
- VAT 10% on all ExportInvoice line items
- All async methods must accept `CancellationToken ct = default`
- Never `.Result` or `.Wait()`
