---
name: ct-alternative-approaches
description: Generate 3–5 alternative solutions for iOS development problems in Cho Tot with pros/cons analysis, code examples, comparison matrix, and decision framework. Use when you need to evaluate trade-offs between different architectural or implementation strategies before committing to one approach.
model: sonnet
effort: high
---

# iOS Alternative Approaches - Multiple Solution Analysis

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

## Overview

This skill generates **3–5 alternative solutions** for iOS development problems in the Chợ Tốt app, with comprehensive pros/cons analysis, Swift code examples, a comparison matrix, and a decision framework. It helps evaluate trade-offs before committing to an implementation strategy.

## When to Use This Skill

**Use this skill when:**
- Multiple viable approaches exist for the same problem
- Trade-offs between complexity, performance, and maintainability need evaluation
- The team needs to make an informed architectural decision
- Refactoring options need to be compared
- You want to avoid premature optimization or over-engineering

## Input Format

```
PROBLEM: [iOS development problem or feature to solve]
CONTEXT: [Module and feature context in Cho Tot app]
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
- Note Vietnamese marketplace-specific requirements

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
```swift
import UIKit
import CTDesignSystem
import CTCommon
import RxSwift
import SnapKit

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
- Error handling with `Logger.print()` (never `print()`)
- Memory management and RxSwift `DisposeBag` cleanup
- Unit test examples using Quick/Nimble
- SwiftLint compliance
- Accessibility support where applicable
- Performance optimization considerations

---

## Architecture Requirements

All solutions must follow:
- **MVVM + Clean Architecture** (Presentation → Domain → Data layers)
- **CTDesignSystem** components (DSLabel, DSButton, DSTextField, DSImageView)
- **SnapKit** for all Auto Layout constraints
- **RxSwift** for reactive programming patterns
- **Swinject / CCDefaultAssembler** for dependency injection

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
PROBLEM: Implement efficient image caching for a feed with thousands of images
CONTEXT: CTFeed module - product listing feed with high image volume
COMPLEXITY_LEVEL: Medium
FOCUS_AREAS: Performance optimization, memory management
SOLUTION_COUNT: 3
```

### Context Analysis

- Performance: High (smooth scrolling required)
- Scale: Large (10K+ images)
- Complexity: Moderate
- Timeline: 2 weeks

---

### Solution 1: NSCache + URLCache Hybrid

**Core Concept**: Use native iOS caching layers — NSCache for in-memory and URLCache for disk-level caching — without any third-party dependency.

```swift
import UIKit
import CTCommon

final class HybridImageCache {
    static let shared = HybridImageCache()
    private let memoryCache = NSCache<NSString, UIImage>()

    private init() {
        memoryCache.countLimit = 100
        memoryCache.totalCostLimit = 50 * 1024 * 1024 // 50MB
    }

    func setImage(_ image: UIImage, forKey key: String) {
        memoryCache.setObject(image, forKey: key as NSString)
    }

    func image(forKey key: String) -> UIImage? {
        return memoryCache.object(forKey: key as NSString)
    }
}
```

- ✅ Native iOS, no extra dependency, automatic memory pressure handling
- ✅ URLCache provides disk persistence automatically
- ❌ Limited customization, no placeholder/transition support
- **Best for**: Standard caching needs, quick implementation, simple feeds

**Performance**: Memory Low · CPU Low · Network Medium · Battery Low  
**Complexity**: Dev Short · Learning Easy · Testing Simple · Maintenance Low

---

### Solution 2: Custom CoreData Image Cache

**Core Concept**: Full control over caching with CoreData — supports complex metadata queries, expiration policies, and offline-first behavior.

```swift
import CoreData
import CTCommon

final class CoreDataImageCache {
    lazy var persistentContainer: NSPersistentContainer = {
        let container = NSPersistentContainer(name: "ImageCache")
        container.loadPersistentStores { _, error in
            if let error = error {
                Logger.print("CoreData load error: \(error)")
            }
        }
        return container
    }()

    func saveImage(_ data: Data, forURL url: String) {
        let context = persistentContainer.viewContext
        // Save image data with metadata
    }
}
```

- ✅ Full control, complex queries, offline support, expiration policies
- ❌ High implementation complexity, performance overhead for simple cases
- **Best for**: Offline-first apps, complex cache invalidation requirements

**Performance**: Memory Medium · CPU High · Network High · Battery Medium  
**Complexity**: Dev Long · Learning Steep · Testing Complex · Maintenance High

---

### Solution 3: Third-Party Library (Kingfisher)

**Core Concept**: Leverage Kingfisher — a battle-tested, feature-rich image loading library with built-in caching, transitions, and placeholder support.

```swift
import Kingfisher
import CTDesignSystem

final class FeedImageLoader {
    func loadImage(into imageView: DSImageView, from url: URL?) {
        imageView.kf.setImage(
            with: url,
            placeholder: R.image.placeholder(),
            options: [
                .transition(.fade(0.2)),
                .cacheOriginalImage
            ]
        )
    }
}
```

- ✅ Feature-rich, battle-tested, community support, minimal boilerplate
- ❌ External dependency, adds binary size, upgrade risk
- **Best for**: Feature-rich requirements, teams familiar with Kingfisher

**Performance**: Memory Medium · CPU Low · Network High · Battery Low  
**Complexity**: Dev Short · Learning Moderate · Testing Simple · Maintenance Medium

---

### Comparison Matrix

| Criteria | Solution 1: Hybrid | Solution 2: CoreData | Solution 3: Kingfisher |
|---|---|---|---|
| Development Time | Short | Long | Short |
| Complexity | Low | High | Low |
| Performance | Medium | Medium | High |
| Maintainability | High | Low | High |
| Scalability | Low | High | High |
| Team Learning Curve | Easy | Steep | Moderate |
| **Recommended For** | Quick MVPs | Offline-first apps | Feature-rich feeds |

### Decision Framework

```
If timeline is tight AND feed is standard → Solution 1 (Hybrid NSCache)
If offline-first is required AND metadata queries needed → Solution 2 (CoreData)
If feature-rich UX (transitions, placeholders) needed → Solution 3 (Kingfisher)
```
