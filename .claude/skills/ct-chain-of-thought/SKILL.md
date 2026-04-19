---
name: ct-chain-of-thought
description: Systematic step-by-step technical design for complex WPF/C# features in Desktop Lamour. Use when designing multi-layer features, database schema decisions, or complex business rule flows across MVVM Clean Architecture layers.
model: sonnet
effort: high
---

# WPF Chain of Thought — Technical Design Analysis

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

## Overview

Systematic Chain of Thought analysis framework for complex WPF/C# development problems in Desktop Lamour. Breaks down problems into logical steps covering requirements, architecture, data flow, edge cases, testing strategy, and implementation order before any code is written.

## When to Use This Skill

- Designing a new feature end-to-end across all 5 layers
- Analyzing technical trade-offs before committing to an approach
- Planning multi-module changes (e.g. a sale that affects both ExportInvoices and Inventory)
- Identifying business rule conflicts (stock validation, invoice immutability, role permissions)
- Planning test strategy for complex business logic

## Input Format

```
FEATURE_TO_ANALYZE: <feature or technical problem>
CONTEXT: <module name, e.g. ExportInvoices>
COMPLEXITY_LEVEL: <Simple | Medium | Complex>
FOCUS_AREAS: <optional — e.g. stock validation, async patterns, DI wiring>
```

## Analysis Structure

When the user provides input, perform a **step-by-step Chain of Thought** across these 8 phases:

---

### Phase 1 — Requirements

- List all functional assumptions about the feature
- Define business rules explicitly (e.g. stock must be > 0 before export, invoice total = sum of line items × quantity)
- Identify user roles that can perform this action (Admin / Thu ngân / Kho)
- List non-functional requirements: loading state, error handling, validation messages

### Phase 2 — Domain Model

- Define the C# model class(es) involved
- List properties with types
- Identify which fields require validation
- Note relationships (e.g. `ExportInvoice` has `ICollection<ExportInvoiceLine>`)

### Phase 3 — UseCase Contract

- Define the UseCase interface: `I[Name]UseCase` with `ExecuteAsync(input, CancellationToken)` signature
- Define input and output types
- State the business rule the UseCase enforces
- Note which other UseCases this may depend on or trigger

### Phase 4 — Repository Interface

- Define `I[Name]Repository` methods needed
- Specify return types: `Task<T>` or `Task<IEnumerable<T>>`
- Note which repository methods map to which HTTP operations

### Phase 5 — Service + DTOs

- Define `I[Name]Service` with HttpClient methods
- Define request and response DTO classes
- Map DTO fields to domain model fields
- Note JSON property names if different from C# names

### Phase 6 — ViewModel State

- List all `[ObservableProperty]` fields needed
- List all `[RelayCommand]` methods needed
- Define `ObservableCollection<T>` bindings
- Define loading / error / success state management pattern

### Phase 7 — XAML Layout

- Describe the major UI sections (header, list, form, buttons)
- Map ViewModel properties to XAML bindings
- Identify which AppStyles.xaml keys will be needed
- Note any DataTemplate or DataGrid column requirements

### Phase 8 — Edge Cases + Test Strategy

- List 4–6 edge cases (empty list, network error, stock = 0, duplicate entry, concurrent save)
- Propose handling strategy for each
- List 3–5 unit test scenarios (UseCase happy path, UseCase with invalid stock, Repository returns empty, ViewModel command fires correctly)
- Define mocking strategy (Moq interfaces)

---

## Implementation Order

After analysis, provide a step-by-step implementation order:

```
1. Domain model + UseCase interface
2. Service interface + Service implementation + DTOs
3. Repository interface + Repository implementation
4. UseCase implementation
5. ViewModel (with [ObservableProperty] + [RelayCommand])
6. XAML View (UserControl)
7. DI registration in ServiceCollectionExtensions
8. xUnit tests
```

---

## Code Standards for Desktop Lamour

- ViewModels inherit from `ObservableObject` (CommunityToolkit.Mvvm)
- Fields decorated with `[ObservableProperty]` → auto-generates PascalCase property
- Commands decorated with `[RelayCommand]` → auto-generates `XxxCommand`
- All async methods must use `CancellationToken` parameter
- No business logic in View (XAML code-behind)
- All HttpClient calls via `System.Net.Http.Json` extension methods
- Use `partial class` for source generator attributes

---

## Example Analysis

### Sample Input

```
FEATURE_TO_ANALYZE: Export invoice creation with stock deduction
CONTEXT: ExportInvoices module
COMPLEXITY_LEVEL: Complex
FOCUS_AREAS: Stock validation, invoice total calculation, concurrent save prevention
```

### Phase 1 — Requirements

**Functional:**
- User fills in customer info, selects products, enters quantities
- System validates stock ≥ requested quantity for each line item
- Invoice total = sum(line.Quantity × line.UnitPrice) with optional discount and tax
- Confirm button creates invoice + deducts stock atomically

**Business rules:**
- Stock must never go negative
- Invoice cannot be modified after confirmation
- Only Admin and Thu ngân roles can create export invoices

**Non-functional:**
- Loading overlay during save
- Inline validation error per line item
- Print/PDF export after confirmation

### Phase 2 — Domain Model

```csharp
public class ExportInvoice
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; }
    public ICollection<ExportInvoiceLine> Lines { get; set; } = new List<ExportInvoiceLine>();
    public decimal Total => Lines.Sum(l => l.Subtotal) * (1 - Discount) * (1 + TaxRate);
}

public class ExportInvoiceLine
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}
```

### Phase 3 — UseCase Contract

```csharp
public interface ICreateExportInvoiceUseCase
{
    Task<ExportInvoice> ExecuteAsync(CreateExportInvoiceRequest request, CancellationToken ct = default);
}
```

Business rule enforced: validate stock ≥ quantity for every line item before calling repository.

### Phase 6 — ViewModel State

```csharp
[ObservableProperty] private bool _isLoading;
[ObservableProperty] private string _errorMessage = string.Empty;
[ObservableProperty] private string _customerName = string.Empty;
[ObservableProperty] private decimal _discount;

public ObservableCollection<ExportInvoiceLineItem> Lines { get; } = new();

[RelayCommand]
private async Task ConfirmInvoiceAsync(CancellationToken ct) { ... }
```

### Phase 8 — Test Strategy

| Test | Scenario |
|---|---|
| `ExecuteAsync_ValidLines_CreatesInvoice` | Happy path |
| `ExecuteAsync_InsufficientStock_ThrowsException` | Stock = 0 |
| `ExecuteAsync_EmptyLines_ThrowsValidationException` | No line items |
| `ViewModel_ConfirmCommand_SetsIsLoading` | Loading state |
| `Repository_Create_CallsServiceWithCorrectDto` | Service mapping |
