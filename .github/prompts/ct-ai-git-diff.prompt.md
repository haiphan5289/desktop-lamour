---
description: "Analyze a git diff for Desktop Lamour — review by architecture layer, MVVM checklist, and business rule impacts."
mode: "agent"
---

# Git Diff Analysis — Desktop Lamour

## Input

```
BASE_BRANCH:  <main | develop>
FOCUS_AREAS:  <optional — e.g. business rules, DI, XAML styles>
```

## Analysis Structure

### 1. Layer Impact Summary

| Layer | Files Changed | Risk |
|---|---|---|
| Presentation (Views/ViewModels) | | |
| Domain (UseCases/Models) | | |
| Data (Repositories/Services/DTOs) | | |
| DI Registration | | |
| Shared/Design System | | |

### 2. MVVM Checklist

- [ ] [ObservableProperty] used — no manual OnPropertyChanged
- [ ] [RelayCommand] on async Task methods
- [ ] No business logic in Views or code-behind
- [ ] All async methods have CancellationToken
- [ ] No .Result or .Wait()
- [ ] IsLoading = false always in finally

### 3. XAML Style Checklist

- [ ] No inline Foreground, FontSize, or Background
- [ ] All controls use Style="{StaticResource ...}"
- [ ] AppButton used (not Button)
- [ ] AppLabel used (not TextBlock)
- [ ] AppTextField used (not TextBox)

### 4. Business Rule Checklist

- [ ] Stock never goes negative
- [ ] Confirmed invoices are immutable
- [ ] Role permissions respected (Admin / Cashier / Warehouse)
- [ ] VAT 10% applied on ExportInvoice line items

### 5. DI Registration Checklist

- [ ] New services registered in [Module]ServiceCollectionExtensions.cs
- [ ] AddHttpClient<IService, Service>() for HTTP services
- [ ] AddScoped for Repositories and UseCases
- [ ] AddTransient for ViewModels

### 6. Summary and Next Steps

List issues found and recommended actions before merging.
