# Desktop Lamour — NuGet Package Management Guide

Full reference for managing NuGet packages in the Desktop Lamour .NET 8 WPF solution.

For step-by-step commands, see the parent `SKILL.md` (cocoapods-to-spm).

## Adding a Package

```bash
dotnet add src/DesktopLamour/DesktopLamour.csproj package <PackageName> --version <version>
dotnet restore src/DesktopLamour.sln
dotnet build src/DesktopLamour.sln
```

## Removing a Package

```bash
dotnet remove src/DesktopLamour/DesktopLamour.csproj package <PackageName>
dotnet restore src/DesktopLamour.sln
```

## Resolving NU1605 Conflicts

Pin the conflicting transitive package explicitly in `DesktopLamour.csproj`:

```xml
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.1" />
```

## Clearing Cache

```bash
dotnet nuget locals all --clear
dotnet restore src/DesktopLamour.sln
```
