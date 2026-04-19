---
name: cocoapods-to-spm
description: NuGet package management for Desktop Lamour. Add, update, or remove NuGet packages via dotnet CLI. Covers Directory.Packages.props central version management, version conflict resolution, and transitive dependency troubleshooting. Use when adding a new library or resolving package restore errors.
argument-hint: "[package-name] [version] — e.g. CommunityToolkit.Mvvm 8.3.2"
model: sonnet
effort: medium
---

# NuGet Package Management for Desktop Lamour

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Add, update, remove, and troubleshoot NuGet packages in the Desktop Lamour .NET 8 WPF solution.

---

## Key Files

| File | Purpose |
|------|---------|
| `src/DesktopLamour/DesktopLamour.csproj` | Main project — package references |
| `src/DesktopLamour.sln` | Solution file |
| `Directory.Packages.props` | Central version management (if present) |
| `NuGet.config` | Feed configuration (private feeds, fallback) |

---

## Adding a Package

```bash
# Add to the main project
dotnet add src/DesktopLamour/DesktopLamour.csproj package CommunityToolkit.Mvvm --version 8.3.2

# Add to the test project
dotnet add tests/DesktopLamour.Tests/DesktopLamour.Tests.csproj package Moq --version 4.20.72
```

After adding, verify the entry in `DesktopLamour.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.2" />
</ItemGroup>
```

---

## Removing a Package

```bash
dotnet remove src/DesktopLamour/DesktopLamour.csproj package SomePackage
```

Then run `dotnet restore` to verify no remaining references.

---

## Updating a Package

```bash
# Update to specific version
dotnet add src/DesktopLamour/DesktopLamour.csproj package Microsoft.Extensions.DependencyInjection --version 8.0.1

# Check for outdated packages
dotnet list src/DesktopLamour/DesktopLamour.csproj package --outdated
```

---

## Central Version Management (Directory.Packages.props)

If `Directory.Packages.props` exists at the solution root, all versions are declared there and project files use `Version`-less references:

```xml
<!-- Directory.Packages.props -->
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="CommunityToolkit.Mvvm" Version="8.3.2" />
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="8.0.1" />
    <PackageVersion Include="Microsoft.Extensions.Http" Version="8.0.1" />
    <PackageVersion Include="xunit" Version="2.9.0" />
    <PackageVersion Include="Moq" Version="4.20.72" />
  </ItemGroup>
</Project>
```

```xml
<!-- DesktopLamour.csproj — no Version attribute needed -->
<PackageReference Include="CommunityToolkit.Mvvm" />
```

To add a new package with central versioning:
1. Add `<PackageVersion Include="PackageName" Version="x.y.z" />` to `Directory.Packages.props`
2. Add `<PackageReference Include="PackageName" />` (no version) to the `.csproj`

---

## Core Desktop Lamour Packages

| Package | Version | Purpose |
|---------|---------|---------|
| `CommunityToolkit.Mvvm` | 8.3.2 | `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]` |
| `Microsoft.Extensions.DependencyInjection` | 8.x | DI container (`IServiceCollection`) |
| `Microsoft.Extensions.Http` | 8.x | `AddHttpClient<>`, `IHttpClientFactory` |
| `Microsoft.Extensions.Configuration` | 8.x | `IConfiguration`, `appsettings.json` |
| `System.Net.Http.Json` | 8.x | `GetFromJsonAsync`, `PostAsJsonAsync` (built-in .NET 8) |
| `xunit` | 2.9.x | Unit test framework |
| `xunit.runner.visualstudio` | 2.8.x | VS Test Explorer integration |
| `Moq` | 4.20.x | Interface mocking in tests |
| `coverlet.collector` | 6.x | Code coverage |

---

## Restore and Build

```bash
# Restore all packages
dotnet restore src/DesktopLamour.sln

# Build solution
dotnet build src/DesktopLamour.sln

# Run tests
dotnet test src/DesktopLamour.sln
```

---

## Version Conflict Resolution

### Symptom: NU1605 — Detected package downgrade

```
error NU1605: Detected package downgrade: Microsoft.Extensions.Logging from 8.0.1 to 8.0.0.
```

**Fix:** Pin the conflicting package explicitly in the affected `.csproj`:

```xml
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.1" />
```

Or in `Directory.Packages.props`:
```xml
<PackageVersion Include="Microsoft.Extensions.Logging" Version="8.0.1" />
```

### Symptom: Ambiguous reference after package upgrade

Two packages export the same type (e.g., `ILogger`).

**Fix:** Use a fully qualified name or an alias:

```csharp
using ILogger = Microsoft.Extensions.Logging.ILogger;
```

### Symptom: Package restore fails — feed not found

**Fix:** Check `NuGet.config` for correct feed URLs. For nuget.org:

```xml
<!-- NuGet.config -->
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

---

## Source Generator Packages (CommunityToolkit.Mvvm)

`CommunityToolkit.Mvvm` uses Roslyn source generators. If `[ObservableProperty]` or `[RelayCommand]` attributes are not recognized:

1. Verify the package is installed and `Version >= 8.0`
2. Verify the class is declared `partial`
3. Clean and rebuild: `dotnet clean && dotnet build`
4. In Visual Studio: Build → Rebuild Solution, then restart VS if needed

```xml
<!-- Required project setting for source generators -->
<PropertyGroup>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>
```

---

## Validation Checklist

- [ ] `dotnet restore` completes without errors
- [ ] `dotnet build` succeeds with 0 errors
- [ ] No NU1605 downgrade warnings
- [ ] No duplicate `PackageReference` entries for the same package
- [ ] `[ObservableProperty]` and `[RelayCommand]` generate properties (partial class confirmed)
- [ ] Test project references `xunit` + `Moq` only (not app packages like WPF assemblies)

See `docs/project-overview.md` for tech stack context.
