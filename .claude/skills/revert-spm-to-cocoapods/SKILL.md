---
name: revert-spm-to-cocoapods
description: Roll back a NuGet package upgrade in Desktop Lamour. Pin a package to a previous version, resolve NU1605 downgrade conflicts, remove a problematic package entirely, and restore a clean build. Use when a package update breaks the build or causes runtime errors.
argument-hint: "[package-name] [target-version] — e.g. CommunityToolkit.Mvvm 8.2.2"
model: sonnet
effort: medium
---

# NuGet Package Rollback for Desktop Lamour

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Roll back, pin, or remove NuGet packages when an upgrade breaks the Desktop Lamour .NET 8 WPF solution.

---

## When to Use

- Build fails after `dotnet add package` or `dotnet restore` with a newer version
- `NU1605` downgrade conflict after updating a transitive dependency
- Runtime `TypeLoadException` or `MissingMethodException` after upgrading
- Source generator stops working after `CommunityToolkit.Mvvm` upgrade
- `[ObservableProperty]` / `[RelayCommand]` attributes no longer recognized

---

## Step 1 — Identify the Problematic Package

```bash
# List all installed packages with versions
dotnet list src/DesktopLamour/DesktopLamour.csproj package

# List packages with known vulnerabilities or outdated status
dotnet list src/DesktopLamour/DesktopLamour.csproj package --vulnerable
dotnet list src/DesktopLamour/DesktopLamour.csproj package --outdated
```

Check the build output for the error source:

```
error CS0246: The type or namespace name 'ObservableObject' could not be found
→ CommunityToolkit.Mvvm version mismatch or source generator not running

error NU1605: Detected package downgrade: Microsoft.Extensions.Http from 8.0.1 to 8.0.0
→ transitive dependency conflict, pin to higher version
```

---

## Step 2 — Pin to a Previous Version

```bash
# Re-add the package at the desired version (overwrites current)
dotnet add src/DesktopLamour/DesktopLamour.csproj package CommunityToolkit.Mvvm --version 8.2.2
```

Verify the `.csproj` shows the pinned version:

```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
```

If using `Directory.Packages.props`, update the version there instead:

```xml
<!-- Directory.Packages.props -->
<PackageVersion Include="CommunityToolkit.Mvvm" Version="8.2.2" />
```

---

## Step 3 — Resolve NU1605 Transitive Conflicts

When a transitive (indirect) dependency causes a downgrade warning:

```bash
# See full dependency tree
dotnet list src/DesktopLamour/DesktopLamour.csproj package --include-transitive
```

**Fix:** Explicitly pin the conflicting transitive package at the higher version:

```xml
<!-- DesktopLamour.csproj -->
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.1" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.2" />
```

This forces NuGet to use the declared version rather than the lower transitive version.

---

## Step 4 — Remove a Package Entirely

```bash
dotnet remove src/DesktopLamour/DesktopLamour.csproj package SomePackage
dotnet restore src/DesktopLamour/DesktopLamour.csproj
```

Then grep for any remaining usages in source files:

```bash
grep -r "using SomePackage" src/DesktopLamour/ --include="*.cs"
```

Fix or remove any compilation errors from the deleted package before restoring.

---

## Step 5 — Clear NuGet Cache and Restore Clean

When a package cache is corrupt or stale:

```bash
# Clear local caches
dotnet nuget locals all --clear

# Restore fresh
dotnet restore src/DesktopLamour.sln
```

---

## Step 6 — Rebuild and Validate

```bash
dotnet clean src/DesktopLamour.sln
dotnet build src/DesktopLamour.sln
dotnet test src/DesktopLamour.sln
```

---

## Common Rollback Scenarios

### CommunityToolkit.Mvvm source generator stops working

**Symptom:** `[ObservableProperty]` and `[RelayCommand]` attributes compile but generate no code. Properties missing at runtime.

**Causes:**
1. Package downgraded below 8.0
2. Class not declared `partial`
3. Incompatible .NET SDK version

**Fix:**
```bash
# Pin to known-good version
dotnet add src/DesktopLamour/DesktopLamour.csproj package CommunityToolkit.Mvvm --version 8.3.2

dotnet clean src/DesktopLamour.sln
dotnet build src/DesktopLamour.sln
```

Verify class is `partial`:
```csharp
// REQUIRED — source generators only work on partial classes
public partial class EmployeesViewModel : ObservableObject { }
```

---

### Microsoft.Extensions.Http conflict

**Symptom:** `NU1605 Detected package downgrade: Microsoft.Extensions.Http from 8.0.1 to 8.0.0`

**Fix:**
```xml
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.1" />
```

---

### HttpClient GetFromJsonAsync missing after downgrade

**Symptom:** `CS1061: 'HttpClient' does not contain a definition for 'GetFromJsonAsync'`

**Cause:** `System.Net.Http.Json` is bundled in .NET 6+ but the project may be targeting an older TFM, or the package was removed.

**Fix:** Verify `DesktopLamour.csproj` targets .NET 8:
```xml
<TargetFramework>net8.0-windows</TargetFramework>
```

If still missing, add explicitly:
```bash
dotnet add src/DesktopLamour/DesktopLamour.csproj package System.Net.Http.Json --version 8.0.1
```

---

## Validation Checklist

- [ ] `dotnet restore` completes without NU1605 warnings
- [ ] `dotnet build` succeeds with 0 errors and 0 warnings
- [ ] `[ObservableProperty]` fields generate PascalCase properties (source generators active)
- [ ] `[RelayCommand]` generates `*Command` properties
- [ ] `dotnet test` passes — no test regressions from rollback
- [ ] No `TypeLoadException` or `MissingMethodException` at runtime

See `docs/project-overview.md` for tech stack context.
