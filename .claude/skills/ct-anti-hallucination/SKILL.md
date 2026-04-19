---
name: ct-anti-hallucination
description: Anti-hallucination guardrails for Desktop Lamour C#/WPF code generation. Enforces verify-before-use for every class name, interface name, namespace, DI registration, XAML resource key, and file path. Referenced by all other lamour-* skills.
model: sonnet
effort: high
---

# Anti-Hallucination Rules for Desktop Lamour (C#/WPF)

> These rules apply to **every code generation task** in this project.
> Before writing a single line of code, complete the verification checklist below.

---

## The Core Rule

**Never reference any class name, interface name, namespace, XAML key, DI registration, or file path you have not verified exists in the current codebase.**

Memory, prior conversations, or example files are NOT proof that something exists now. The codebase is the only source of truth.

---

## Pre-Generation Verification Checklist

Complete every applicable item before generating code.

### 1. File Paths

- [ ] Use `Glob` to confirm every target file path exists before reading or referencing it
- [ ] Namespace must match folder structure: `DesktopLamour.Features.Inventory.ViewModels` → `src/DesktopLamour/Features/Inventory/ViewModels/`
- [ ] Never assume a file exists because its sibling exists
- [ ] XAML code-behind file must match its `.xaml` partner exactly

### 2. Interface Names

- [ ] All interfaces follow the `I[Name]` prefix convention: `ILoginUseCase`, `IEmployeeRepository`, `IInventoryService`
- [ ] Use `Grep` to verify `interface I[Name]` exists before referencing it
- [ ] Never invent an interface name from the implementation name alone
- [ ] Domain UseCase interfaces live in `Features/[Module]/Domain/`
- [ ] Repository interfaces live in `Features/[Module]/Data/` or `Features/[Module]/Domain/`
- [ ] Service interfaces live in `Features/[Module]/Data/`

### 3. Class Names and Namespaces

- [ ] Use `Grep` to find the exact `class [Name]` or `partial class [Name]` declaration before referencing
- [ ] ViewModel suffix is always `ViewModel`: `LoginViewModel`, `EmployeeListViewModel`
- [ ] UseCase suffix is always `UseCase`: `LoginUseCase`, `GetEmployeesUseCase`
- [ ] Repository suffix is always `Repository`: `EmployeeRepository`
- [ ] Service suffix is always `Service`: `EmployeeService`
- [ ] Namespace matches folder path exactly — verify with `Read` on the target file

### 4. CommunityToolkit.Mvvm Attributes

- [ ] `[ObservableProperty]` generates a PascalCase property from the `_camelCase` field
  - Field `_isLoading` → generated property `IsLoading`
  - Field `_errorMessage` → generated property `ErrorMessage`
  - **NEVER reference the field name (`_isLoading`) in bindings — always use the generated property (`IsLoading`)**
- [ ] `[RelayCommand]` generates a command from method name: method `LoadData()` → command `LoadDataCommand`
- [ ] `[ObservableProperty]` requires the field to be `private` and lowercase with underscore prefix
- [ ] `partial class` is required for source-generator attributes to work

### 5. XAML Resource Keys

- [ ] Verify every `StaticResource` key exists in `AppStyles.xaml` or `AppTypography.xaml` before using it
- [ ] Use `Grep` with pattern `x:Key="[KeyName]"` to verify keys exist
- [ ] **NEVER hardcode** colors, font sizes, or spacing values — always use `StaticResource`
- [ ] `AppStyles.xaml` and `AppTypography.xaml` live in `src/DesktopLamour/Themes/`
- [ ] Shared controls live in `src/DesktopLamour/Shared/`

### 6. DI Registrations (ServiceCollectionExtensions)

- [ ] Verify the DI registration file exists: `Glob` for `ServiceCollectionExtensions.cs` in the module
- [ ] Every interface+implementation pair must be registered before use
- [ ] Registration pattern: `services.AddScoped<IFooUseCase, FooUseCase>()`
- [ ] `HttpClient` is registered via `services.AddHttpClient<IFooService, FooService>()`
- [ ] Never reference a type in a ViewModel constructor that is not already DI-registered

### 7. Binding Paths in XAML

- [ ] Every `{Binding PropertyName}` must have a matching `[ObservableProperty]`-generated property in the ViewModel
- [ ] DataContext must be set correctly — check code-behind or DI wiring
- [ ] `Command="{Binding XxxCommand}"` requires a `[RelayCommand]`-decorated method `Xxx()` in the ViewModel
- [ ] `UpdateSourceTrigger=PropertyChanged` is required for two-way TextBox bindings

### 8. HTTP / API Calls

- [ ] Use `System.Net.Http.Json` extension methods: `GetFromJsonAsync`, `PostAsJsonAsync`, `PutAsJsonAsync`
- [ ] DTO class names must exactly match what is declared in the `Data/DTOs/` folder
- [ ] Never invent an API endpoint path — use the one explicitly provided or ask the user
- [ ] All service methods must be `async Task<T>` — never use `.Result` or `.Wait()`

---

## Hallucination Red Flags — Stop and Verify

| Red flag | What to do instead |
|---|---|
| Referencing `_fieldName` in XAML binding | Use generated property name (PascalCase) |
| Writing `StaticResource SomeFontKey` without checking | Grep `x:Key="SomeFontKey"` in AppTypography.xaml |
| Using `services.AddTransient<Foo>()` without an interface | Verify interface `IFoo` exists first |
| Calling `.Result` on an async method | Use `await` properly |
| Writing `new FooViewModel()` in code-behind | Resolve from DI container |
| Using `INotifyPropertyChanged` manually | Use `[ObservableProperty]` from CommunityToolkit.Mvvm |
| Assuming namespace from folder path without verifying | Read the file's `namespace` declaration |
| Creating `ObservableCollection<T>` without proper init | Check ViewModel initialization order |
| Referencing a repository method that doesn't exist | Read the repository interface first |

---

## When Verification Fails

If a required class, interface, XAML key, or path cannot be found:

1. **Do not invent a substitute** — report what is missing
2. **Ask the user** before proceeding: _"I could not find `IFooUseCase` in the codebase. Can you confirm the correct interface name or path?"_
3. If the user confirms it does not exist yet: create it following existing patterns, and clearly flag it as a **new addition**

---

## Quick Verification Commands

```
# Verify an interface exists
Grep: pattern="interface IFooUseCase" type="cs"

# Verify a XAML resource key
Grep: pattern="x:Key=\"ButtonPrimary\"" path="src/DesktopLamour/Themes"

# Verify a namespace
Read: target file → check namespace declaration

# Verify a DI registration
Grep: pattern="AddScoped.*IFooUseCase" type="cs"

# Verify a file path
Glob: pattern="**/FooViewModel.cs"

# Verify [ObservableProperty] field name
Grep: pattern="\[ObservableProperty\]" path="Features/Foo/ViewModels/"
```

---

## Project Architecture Reference

```
src/DesktopLamour/
├── Features/
│   ├── Authentication/
│   │   ├── Domain/        # ILoginUseCase, LoginUseCase, LoginModel
│   │   ├── Data/          # IAuthService, AuthService, AuthRepository, DTOs
│   │   ├── Views/         # LoginView.xaml, LoginView.xaml.cs
│   │   └── ViewModels/    # LoginViewModel.cs
│   ├── Employees/
│   ├── Inventory/
│   ├── ImportInvoices/
│   └── ExportInvoices/
├── Core/                  # Navigation, Storage, shared base classes
├── Shared/                # AppLabel.cs, shared UserControls
├── Themes/                # AppStyles.xaml, AppTypography.xaml
└── MainWindow/            # Shell window
```

See `docs/project-overview.md` for full project context.
