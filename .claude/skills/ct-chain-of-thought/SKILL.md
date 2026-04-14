---
name: ct-chain-of-thought
description: Systematic step-by-step technical design analysis for complex C#/.NET WPF features. Use when designing a new feature or solving a complex problem that requires thorough reasoning across requirements, architecture, data flow, edge cases, testing, and implementation roadmap following MVVM + Clean Architecture.
model: sonnet
effort: high
---

# WPF Chain of Thought - Technical Design Analysis

## Overview

This skill provides a systematic Chain of Thought analysis framework for complex C#/.NET WPF development problems. It breaks down problems into logical steps covering requirement analysis, architecture design, data flow, edge cases, testing strategy, and implementation roadmap.

## When to Use This Skill

**Use this skill when:**
- Designing a new complex feature end-to-end
- Analyzing technical trade-offs before implementation
- Conducting a design review for a feature
- Planning architecture for a multi-layer change
- Identifying risks, edge cases, and test coverage before coding

## Input Format

```
FEATURE_TO_ANALYZE: [Feature or technical problem to analyze]
CONTEXT: [Context and module in the application]
COMPLEXITY_LEVEL: [Simple / Medium / Complex]
FOCUS_AREAS: [Specific aspects to focus on, optional]
```

## Analysis Structure

When the user provides input, perform a **step-by-step Chain of Thought analysis** across these 6 phases:

---

### 1. 🧭 Requirement Analysis
- List all functional and non-functional assumptions about the feature
- Identify key user flows and expected behaviors
- Define constraints: network, caching, offline, performance, localization
- Consider Windows desktop-specific requirements

### 2. 🧩 Architecture Design (MVVM + Clean Architecture)
- Break down feature into layers: View → ViewModel → UseCase → Repository → Service → HttpClient
- Explain responsibility of each layer and communication patterns
- Identify dependency injection points (`Microsoft.Extensions.DependencyInjection`)
- Note AppDesignSystem integration requirements (`AppLabel`, `AppButton`, `AppTextField`, etc.)

### 3. 🔄 Data Flow & Logic (Step-by-Step)
- Trace full lifecycle: user action → ViewModel command → UseCase → Repository → API → Model → UI update
- Include loading (`IsLoading`), success, and error state handling
- Detail data transformation between layers
- Reference MVVM Toolkit patterns: `[ObservableProperty]`, `[RelayCommand]`, `async Task`, `CancellationToken`

### 4. 🧪 Edge Cases & Failure Handling
- List 4–6 possible edge cases or error scenarios
- Propose graceful handling strategies for each
- Consider offline scenarios and data persistence
- Plan for localization edge cases

### 5. 🧰 Testing & Validation Plan
- Suggest 3–5 key unit tests or integration tests using xUnit + FluentAssertions + Moq
- Explain how to validate business logic and HTTP responses
- Describe mock strategies for dependencies
- Note key UI automation testing scenarios

### 6. 📦 Implementation Roadmap
- Summarize the step-by-step implementation plan
- Highlight risks, technical debt, and scalability considerations
- Identify potential performance bottlenecks
- Consider future extensibility requirements

---

## Code Standards to Follow

- Use `AppDesignSystem` components (`AppLabel`, `AppButton`, `AppTextField`, `AppImage`)
- Use XAML `Grid`/`StackPanel`/`DockPanel` for all layout — no code-behind positioning
- Use `CommunityToolkit.Mvvm` with `[ObservableProperty]` and `[RelayCommand]`
- Use `ILogger<T>` instead of `Console.WriteLine` for logging
- Follow MVVM + Clean Architecture layer separation
- Use `Microsoft.Extensions.DependencyInjection` for DI

## Output Style

Think aloud and explain reasoning before the final summary. The output should read like a **senior engineer walking through a design document** before coding — not just a list of bullet points.

❗️ **Important:** Do not jump to code immediately. Analyze first, then provide implementation details only after the full analysis is complete.

---

## Example Analysis

### Sample Input

```
FEATURE_TO_ANALYZE: Fetch and display a list of orders from an API with caching
CONTEXT: Features/Orders module - order management
COMPLEXITY_LEVEL: Medium
FOCUS_AREAS: Performance optimization, offline support
```

---

### 1. 🧭 Requirement Analysis

**Functional assumptions:**
- User opens the order list View and sees all their orders
- Orders are fetched from a remote REST API (paginated)
- Each order shows: order ID, status, total amount, created date
- User can click an order to view details

**Non-functional assumptions:**
- Response time < 2s; `AppProgressRing` shown while fetching
- Orders are cached locally so the list is visible offline (stale-while-revalidate)
- Localization required (currency formats, date formats via `CultureInfo`)

**Constraints:**
- Network: REST API, JSON response (`System.Text.Json`)
- Caching: `IMemoryCache` for in-process
- Offline: Show cached data with a "last updated" indicator

---

### 2. 🧩 Architecture Design (MVVM + Clean Architecture)

```
OrderListView.xaml           → AppLabel, AppButton (AppDesignSystem)
        ↓ [RelayCommand]
OrderListViewModel           → [ObservableProperty] items/isLoading/errorMessage
        ↓ awaits
FetchOrdersUseCase           → Business logic: validate, sort orders
        ↓ delegates
IOrderRepository             → Abstract interface
        ↓ implements
OrderRepository              → Checks cache → fetches API → updates cache
        ↓ calls
IOrderService (HttpClient)   → GET /api/v1/orders → Task<List<OrderDto>>
```

DI: Registered in `ServiceCollectionExtensions.AddOrderModule(services)`.

---

### 3. 🔄 Data Flow & Logic

1. `OnLoaded` event or `[RelayCommand]` triggers `LoadOrdersAsync`
2. ViewModel sets `IsLoading = true`
3. ViewModel calls `await _fetchOrdersUseCase.ExecuteAsync(ct)`
4. UseCase calls `await _repository.GetOrdersAsync(ct)`
5. Repository checks `IMemoryCache`; if miss → calls `IOrderService.GetOrdersAsync(ct)`
6. Service uses typed `HttpClient` → `GET /api/v1/orders` → deserializes `List<OrderDto>`
7. Repository maps DTOs to domain `OrderModel`, updates cache
8. UseCase validates/sorts, returns `IReadOnlyList<OrderModel>`
9. ViewModel maps to `ObservableCollection<OrderItemViewModel>`
10. XAML `ListView` binding refreshes automatically via `INotifyPropertyChanged`

**Error path:** `catch (Exception ex)` → `_logger.LogError(ex, "...")` → `ErrorMessage = "..."`

---

### 4. 🧪 Edge Cases & Failure Handling

| Edge Case | Handling Strategy |
|---|---|
| Empty API response | Show empty state with `AppEmptyState` control |
| Network timeout | Show cached data + snackbar "Showing offline data" |
| All orders completed | Filter on UseCase layer, show "No pending orders" |
| Malformed JSON | `JsonException` caught, logged via `ILogger`, user-friendly error |
| Token expired (401) | Propagate auth error → navigate to login via `INavigationService` |
| Pagination failure | Retry button in footer `DataTemplate` |

---

### 5. 🧰 Testing & Validation Plan

```csharp
// xUnit + FluentAssertions + Moq

public class FetchOrdersUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnSortedOrders_WhenRepositorySucceeds() { }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenRepositoryFails() { }
}

public class OrderListViewModelTests
{
    [Fact]
    public async Task LoadCommand_SetsIsLoading_ThenFalseAfterCompletion() { }

    [Fact]
    public async Task LoadCommand_SetsErrorMessage_OnException() { }
}
```

---

### 6. 📦 Implementation Roadmap

1. Create `IOrderRepository` + `OrderRepository` (Data layer)
2. Create `IOrderService` + `OrderService` with typed `HttpClient`
3. Create `FetchOrdersUseCase` (`IUseCase<CancellationToken, IReadOnlyList<OrderModel>>`)
4. Create `OrderListViewModel` with `[ObservableProperty]` + `[RelayCommand]`
5. Create `OrderListView.xaml` with `ListView` + `DataTemplate` referencing `OrderItemDataTemplate`
6. Register all in `ServiceCollectionExtensions.AddOrderModule`
7. Write unit tests for ViewModel and UseCase
8. Test pagination, empty state, offline behavior
