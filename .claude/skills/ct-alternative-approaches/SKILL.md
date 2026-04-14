---
name: ct-alternative-approaches
description: Generate 3–5 alternative solutions for C#/.NET WPF development problems with pros/cons analysis, code examples, comparison matrix, and decision framework. Use when you need to evaluate trade-offs between different architectural or implementation strategies before committing to one approach.
model: sonnet
effort: high
---

# WPF Alternative Approaches - Multiple Solution Analysis

## Overview

This skill generates **3–5 alternative solutions** for C#/.NET WPF development problems, with comprehensive pros/cons analysis, C# code examples, a comparison matrix, and a decision framework. It helps evaluate trade-offs before committing to an implementation strategy.

## When to Use This Skill

**Use this skill when:**
- Multiple viable approaches exist for the same problem
- Trade-offs between complexity, performance, and maintainability need evaluation
- The team needs to make an informed architectural decision
- Refactoring options need to be compared
- You want to avoid premature optimization or over-engineering

## Input Format

```
PROBLEM: [C#/.NET WPF development problem or feature to solve]
CONTEXT: [Module and feature context in the application]
COMPLEXITY_LEVEL: [Simple / Medium / Complex]
FOCUS_AREAS: [Aspects to focus on, optional]
SOLUTION_COUNT: [Number of alternatives: 3-5, optional]
```

## Analysis Structure

When the user provides input, generate multiple solutions following this structure:

---

### 1. 🎯 Problem Analysis Framework
- Analyze the problem requirements and constraints
- Identify key technical challenges
- Consider performance, scale, and complexity factors
- Define success criteria for solutions
- Note Windows desktop application-specific requirements

### 2. 🔄 Solution Generation (3–5 Alternatives)
- Generate multiple viable approaches using different methodologies
- Each solution must solve the **same problem** with a different strategy
- Organize by categories: Architecture-based, Technology-based, Implementation-based
- Ensure all solutions follow MVVM + Clean Architecture patterns

---

## Required Solution Format

Each solution must include:

```markdown
## Solution [Number]: [Approach Name]

### Core Concept
Brief description of the fundamental approach and methodology.

### Implementation Strategy
Detailed explanation of how this solution works.

### Code Example
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

// Implementation example
```

### Advantages (Pros)
- ✅ Advantage 1: Explanation
- ✅ Advantage 2: Explanation

### Disadvantages (Cons)
- ❌ Disadvantage 1: Explanation
- ❌ Disadvantage 2: Explanation

### Best Use Cases
- Scenario 1: When to use this approach
- Scenario 2: Specific conditions that favor this solution

### Performance Impact
- Memory usage: [High/Medium/Low]
- CPU usage: [High/Medium/Low]
- Network efficiency: [High/Medium/Low]
- Battery impact: [High/Medium/Low]

### Implementation Complexity
- Development time: [Short/Medium/Long]
- Learning curve: [Easy/Moderate/Steep]
- Testing complexity: [Simple/Moderate/Complex]
- Maintenance effort: [Low/Medium/High]
```

---

### 3. 📊 Evaluation & Comparison Matrix

After all solutions, provide a side-by-side comparison:

```markdown
| Criteria | Solution A | Solution B | Solution C |
|----------|------------|------------|------------|
| Development Time | ... | ... | ... |
| Complexity | ... | ... | ... |
| Performance | ... | ... | ... |
| Maintainability | ... | ... | ... |
| Scalability | ... | ... | ... |
| Team Learning Curve | ... | ... | ... |
| Recommended For | ... | ... | ... |
```

Score each criterion 1–5 for objective comparison.

### 4. 🎯 Decision Framework

Provide a decision tree or framework to help choose between solutions:
- Consider: timeline, team experience, complexity requirements
- Offer specific recommendations for different scenarios
- Include risk assessment for each approach

### 5. ✅ Code Quality Standards for Every Solution

Every solution must address:
- Error handling with `ILogger<T>` (never `Console.WriteLine` without logging)
- Memory management and `IDisposable` cleanup for event handlers
- Unit test examples using xUnit + FluentAssertions + Moq
- Roslyn / StyleCop.Analyzers compliance
- Accessibility support where applicable (AutomationProperties)
- Performance optimization considerations (UI thread dispatch, virtualization)

---

## Architecture Requirements

All solutions must follow:
- **MVVM + Clean Architecture** (Presentation → Domain → Data layers)
- **AppDesignSystem** components (`AppLabel`, `AppButton`, `AppTextField`, `AppImage`)
- **XAML** for all UI layout (`Grid`, `StackPanel`, `DockPanel`, `Border`)
- **CommunityToolkit.Mvvm** for data binding
- **Microsoft.Extensions.DependencyInjection** for dependency injection

## Customization Options

- **Solution Count**: 3–5 (default 3 for Simple, 4–5 for Complex)
- **Detail Level**: High-level concepts vs. full implementation
- **Focus Areas**: Performance, maintainability, testability, etc.
- **Team Context**: Adjust recommendations to team skill level

❗️ **Important:** Each solution must be a **viable alternative for the same problem** — not different problems. The goal is to explore different strategies to solve the exact same requirement.

---

## Example Problem Analysis

### Sample Input

```
PROBLEM: Implement efficient data caching for a list view with thousands of items
CONTEXT: Features/ProductListing module - product listing with high data volume
COMPLEXITY_LEVEL: Medium
FOCUS_AREAS: Performance optimization, memory management
SOLUTION_COUNT: 3
```

### Context Analysis

- Performance: High (smooth scrolling via VirtualizingStackPanel)
- Scale: Large (10K+ items)
- Complexity: Moderate
- Timeline: 2 weeks

---

### Solution 1: MemoryCache (Microsoft.Extensions.Caching.Memory)

**Core Concept**: Use `IMemoryCache` from Microsoft.Extensions.Caching — in-process, fast, configurable with size/expiry — no extra NuGet dependency.

```csharp
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public sealed class ProductCache
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProductCache> _logger;

    public ProductCache(IMemoryCache cache, ILogger<ProductCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public void Set(string key, ProductModel product)
    {
        var options = new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(10),
            Size = 1
        };
        _cache.Set(key, product, options);
    }

    public bool TryGet(string key, out ProductModel? product)
        => _cache.TryGetValue(key, out product);
}
```

- ✅ No extra dependency (part of .NET), automatic memory pressure eviction via GC
- ✅ Thread-safe, configurable expiry, simple API
- ❌ In-process only (lost on app restart), no distributed caching
- **Best for**: Standard caching needs, quick implementation, simple lists

**Performance**: Memory Low · CPU Low · Network Medium  
**Complexity**: Dev Short · Learning Easy · Testing Simple · Maintenance Low

---

### Solution 2: SQLite / Entity Framework Core Local Cache

**Core Concept**: Persist data locally with EF Core + SQLite — supports complex queries, offline-first, and cross-session caching.

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed class ProductDbContext : DbContext
{
    public DbSet<ProductEntity> Products => Set<ProductEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=products.db");
}

public sealed class LocalProductRepository : IProductRepository
{
    private readonly ProductDbContext _db;
    private readonly ILogger<LocalProductRepository> _logger;

    public LocalProductRepository(ProductDbContext db, ILogger<LocalProductRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProductModel>> GetAllAsync(CancellationToken ct)
        => await _db.Products.AsNoTracking()
            .Select(e => new ProductModel { Id = e.Id, Name = e.Name })
            .ToListAsync(ct);
}
```

- ✅ Full control, complex queries, offline support, persistent across sessions
- ❌ Higher implementation complexity, schema migrations needed
- **Best for**: Offline-first apps, complex data invalidation requirements

**Performance**: Memory Medium · CPU Medium · Network High  
**Complexity**: Dev Long · Learning Steep · Testing Complex · Maintenance High

---

### Solution 3: Distributed Cache (StackExchange.Redis / IDistributedCache)

**Core Concept**: Use `IDistributedCache` backed by Redis or SQL Server — suitable for multi-instance deployments and shared state.

```csharp
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

public sealed class DistributedProductCache
{
    private readonly IDistributedCache _cache;

    public DistributedProductCache(IDistributedCache cache) => _cache = cache;

    public async Task SetAsync(string key, ProductModel product, CancellationToken ct)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(product);
        await _cache.SetAsync(key, json,
            new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(30) }, ct);
    }

    public async Task<ProductModel?> GetAsync(string key, CancellationToken ct)
    {
        var bytes = await _cache.GetAsync(key, ct);
        return bytes is null ? null : JsonSerializer.Deserialize<ProductModel>(bytes);
    }
}
```

- ✅ Works across multiple instances, battle-tested, abstracted behind `IDistributedCache`
- ❌ External Redis dependency, higher latency than in-process, added infrastructure
- **Best for**: Multi-instance deployments, microservice architectures

**Performance**: Memory Low · CPU Low · Network High  
**Complexity**: Dev Short · Learning Moderate · Testing Simple · Maintenance Medium

---

### Comparison Matrix

| Criteria | Solution 1: MemoryCache | Solution 2: EF Core SQLite | Solution 3: Distributed |
|---|---|---|---|
| Development Time | Short | Long | Short |
| Complexity | Low | High | Low |
| Performance | High | Medium | Medium |
| Maintainability | High | Low | High |
| Scalability | Low | High | High |
| Team Learning Curve | Easy | Steep | Moderate |
| **Recommended For** | Quick MVPs | Offline-first apps | Multi-instance apps |

### Decision Framework

```
If timeline is tight AND feed is standard → Solution 1 (Hybrid NSCache)
If offline-first is required AND metadata queries needed → Solution 2 (CoreData)
If feature-rich UX (transitions, placeholders) needed → Solution 3 (Kingfisher)
```
