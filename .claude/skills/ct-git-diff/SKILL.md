---
name: ct-git-diff
description: Compare current branch against a target branch or commit SHA for Desktop Lamour. Produces layered output — structured analysis by architecture layer, WPF MVVM review checklist, and a narrative summary. Suggests next steps without auto-chaining.
argument-hint: "[branch|SHA] [--full] [--path <dir>] [--focus <layer>] [--limit <n>]"
model: sonnet
effort: medium
---

# ct-git-diff — Git Diff for Desktop Lamour

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Compare the **current branch** against a target branch or commit SHA and produce a structured, layered analysis tailored to the Desktop Lamour codebase.

---

## How to Use

```
/ct-git-diff                    # auto-detect base branch (main, then dev)
/ct-git-diff main               # compare against main
/ct-git-diff abc1234            # compare against a specific commit SHA
/ct-git-diff dev --focus Domain # only show Domain layer changes
/ct-git-diff main --path src/DesktopLamour/Features/Employees
```

Before running the diff, print a one-line confirmation:

```
Comparing: your-branch → main   (3 commits ahead)
```

**Optional filters:**

| Flag | What it does | Example |
|------|-------------|---------|
| `--full` | Show every added/removed line instead of summary | `/ct-git-diff main --full` |
| `--path <dir>` | Only look at files inside this folder | `/ct-git-diff main --path src/DesktopLamour/Features/Employees` |
| `--focus <layer>` | Only show files matching: Domain / Data / ViewModels / Views | `/ct-git-diff main --focus ViewModels` |
| `--limit <n>` | Stop after N files (default 100) | `/ct-git-diff main --limit 30` |

---

## Execution Steps

### Step 1 — Detect Base Branch

```bash
git branch -r | grep -E "origin/(main|dev|master)" | head -1
git rev-parse --abbrev-ref HEAD        # current branch
git rev-list --count HEAD...origin/main  # commits ahead
```

### Step 2 — List Changed Files

```bash
git diff --name-only origin/main...HEAD
```

Group files by layer:

| Layer | Path pattern |
|-------|-------------|
| Domain — Models | `*/Domain/Models/*.cs` |
| Domain — UseCases | `*/Domain/UseCases/*.cs` |
| Data — Repositories | `*/Data/Repositories/*.cs` |
| Data — Services | `*/Data/Services/*.cs` |
| Data — DTOs | `*/Data/Services/DTOs/*.cs` |
| Presentation — ViewModels | `*/ViewModels/*.cs` |
| Presentation — Views | `*/Views/*.xaml` or `*/Views/*.xaml.cs` |
| DI | `*ServiceExtensions.cs` |
| Tests | `tests/**/*.cs` |
| Shared | `src/DesktopLamour/Shared/**` |
| Themes | `src/DesktopLamour/Themes/**` |

### Step 3 — Per-Layer Structured Analysis

For each changed file, report:

```
[Layer] src/DesktopLamour/Features/Employees/Domain/UseCases/CreateEmployeeUseCase.cs
  + Added: ICreateEmployeeUseCase interface and implementation
  ~ Modified: constructor injection pattern
  Issues: [none | list violations]
```

### Step 4 — Review Checklist

**Architecture**
- [ ] Every UseCase depends on a Repository interface (not concrete class)
- [ ] Repository depends on a Service interface (not concrete class)
- [ ] No `using` import from a higher layer in a lower layer (no ViewModel in Domain)
- [ ] New module registers services in `[Module]ServiceExtensions.cs`
- [ ] `AddHttpClient<IService, Service>()` used (not `AddScoped<HttpClient>`)

**ViewModel (CommunityToolkit.Mvvm)**
- [ ] `partial class` declared — required for source generators
- [ ] Fields use `_camelCase` prefix with `[ObservableProperty]`
- [ ] `ObservableCollection<T>` used for bindable lists (not `List<T>`)
- [ ] `[RelayCommand]` async methods end in `Async` suffix
- [ ] `OperationCanceledException` caught separately from `Exception`
- [ ] `finally { IsLoading = false; }` always present in async commands
- [ ] No `new SomeViewModel()` — DI injection only

**XAML / Styles**
- [ ] No inline `FontSize`, `Background`, `Foreground`, `FontWeight`
- [ ] All StaticResource keys exist in `AppStyles.xaml` or `AppTypography.xaml`
- [ ] `UpdateSourceTrigger=PropertyChanged` on every two-way TextBox binding
- [ ] DataTemplate commands use `RelativeSource AncestorType` pattern
- [ ] No hardcoded color values (`#FFFFFF`, `Red`, etc.)

**Async / Error Handling**
- [ ] All UseCase/Repository/Service methods accept `CancellationToken ct = default`
- [ ] `EnsureSuccessStatusCode()` called after POST/PUT/DELETE HTTP calls
- [ ] No `.Result` or `.Wait()` (deadlock risk on UI thread)

**Business Rules**
- [ ] Stock quantity never set below 0 (Inventory/ExportInvoices)
- [ ] Confirmed invoices cannot be modified (ImportInvoices/ExportInvoices)
- [ ] Role-based access enforced where required (Admin-only operations)

**Tests**
- [ ] New UseCase has corresponding xUnit test file
- [ ] Tests use Moq for all interface dependencies
- [ ] Business rule edge cases covered (e.g. stock = 0, duplicate name)

### Step 5 — Narrative Summary

Plain-language PR description draft:

```
## What Changed
[1–3 sentences describing the feature/fix]

## Layers Affected
- Domain: [yes/no — what changed]
- Data: [yes/no — what changed]
- Presentation: [yes/no — what changed]
- Tests: [yes/no]

## Risk Areas
[List any concerns: missing error handling, untested edge case, business rule gap]

## Missing Pieces
[List anything that should exist but doesn't: DI registration, test file, etc.]
```

### Step 6 — Suggested Next Steps

After analysis, suggest (never auto-invoke):

```
Suggested next steps:
- /review-code          — full architecture + business rule review
- /ct-unittest          — generate missing xUnit tests
- /ct-bugfix-skill      — diagnose any flagged issues
- /ct-quality-engineer  — QE validation against project-overview.md
```

---

## Output Format

```
Comparing: feature/employees-crud → main   (5 commits ahead)

FILES CHANGED: 12

── Domain (2 files) ──────────────────────────────
✅ Domain/Models/Employee.cs — new record type
✅ Domain/UseCases/CreateEmployeeUseCase.cs — interface + implementation

── Data (4 files) ────────────────────────────────
✅ Data/Repositories/EmployeeRepository.cs
✅ Data/Services/EmployeeService.cs
✅ Data/Services/DTOs/EmployeeDto.cs
⚠️  Data/Services/EmployeeService.cs — missing EnsureSuccessStatusCode() on POST

── Presentation (4 files) ────────────────────────
✅ ViewModels/EmployeesViewModel.cs — partial class ✓
⚠️  Views/EmployeesView.xaml — inline FontSize="16" found (use TextBodyStyle)
✅ Views/CreateEmployeeWindow.xaml
✅ Views/CreateEmployeeWindow.xaml.cs

── DI (1 file) ───────────────────────────────────
✅ EmployeesServiceExtensions.cs

── Tests (1 file) ────────────────────────────────
⚠️  CreateEmployeeUseCaseTests.cs — missing test for duplicate name business rule

REVIEW CHECKLIST: 18/20 passed   ⚠️ 2 issues

NARRATIVE:
This PR adds the Employees CRUD feature — create, list, and delete employees.
Risk areas: POST call missing EnsureSuccessStatusCode, inline style in EmployeesView.xaml.
Missing: test coverage for duplicate employee name validation.
```

See `docs/project-overview.md` for business domain context.
