---
name: ct-chotot-module-context
description: "Quick reference for Cho Tot module architecture, MVVM-C patterns, CTDesignSystem usage, and DI setup. Use when working on CTInsertAd, CTJOB, CTVEH, CTChat, or any module — understanding directory structure, Assembler/DI configuration, protocol patterns ([Feature]ViewModelType, [Feature]Presentable, [Feature]PresentableListener), UseCase/Repository patterns, ECS enum handling, or module-specific conventions. Provides quick patterns, file paths, localization usage, and logging guidance."
model: sonnet
effort: medium
argument-hint: "[module name or architecture pattern]"
---

# Cho Tot Module Context

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

This skill provides quick reference for patterns and architecture you use frequently.

## How to Use This Skill

**With arguments:**
```
/ct-chotot-module-context CTInsertAd
/ct-chotot-module-context MVVM-C architecture
/ct-chotot-module-context UseCase pattern
```

**Supported argument patterns:**
- **Module names**: `CTInsertAd`, `CTJOB`, `CTChat`, `CTFeed`, any AppFeature or Library
- **Architecture patterns**: `MVVM-C`, `Coordinator`, `UseCase`, `Repository`, `RxSwift`
- **Specific components**: `ViewModel`, `ViewController`, `Presenter`, `PresentableListener`
- **File context**: When selected text is provided via `{selectedText}`, reviews code patterns directly

---

**Your focus:** $ARGUMENTS

Provide quick reference and guidance specifically for: **$ARGUMENTS**

## MVVM-C Architecture in Cho Tot

**3-Protocol Pattern** (per module):

1. **`[Feature]ViewModelType`** — ViewModel protocol (conforms to `CTViewModelType`)
2. **`[Feature]Presentable`** — ViewController protocol exposing `BehaviorRelay`/`PublishRelay` state
3. **`[Feature]PresentableListener`** — ViewController → ViewModel trigger relays (user interactions)

**Data Flow:**
```
ViewController triggers → ViewModel (via PresentableListener relays)
                       → UseCase (CTActionUseCaseType, action?.execute)
                       → Repository → Service → API Target
                       ← UseCase action?.elements
                       ← Presenter relays (BehaviorRelay.accept)
                       ← ViewController binds to UI
```

**Coordinator Pattern:**
- Navigation is handled by Coordinator, not ViewController
- ViewControllers don't know about other ViewControllers
- Coordinator is injected into ViewModel and triggered via listener

## CTInsertAd Module Structure

```
AppFeatures/CTInsertAd/
├── CTInsertAd/
│   ├── Presentation/
│   │   ├── ViewControllers/
│   │   ├── Views/
│   │   └── ViewModels/
│   ├── Domain/
│   │   ├── UseCases/
│   │   └── Models/
│   ├── Data/
│   │   ├── Repositories/
│   │   └── Services/
│   └── Assembler/
```

Common modules you work on:
- **CreateAd** — Ad creation flow
- **ReviewAd** — Ad review/publish
- **Categories** — Category selection
- **Location** — Location picker

## CTDesignSystem Component Usage

**ALWAYS use DS components**, never raw UIKit:
- `DSLabel` instead of `UILabel`
- `DSButton` instead of `UIButton`
- `DSTextField` instead of `UITextField`
- `DSImageView` instead of `UIImageView`

**Styling Example:**
```swift
label.setStyle(DS.TypoToken.Header.Section(color: theme.text.textPrimary.color))
```

**Layout: SnapKit ONLY** — no Interface Builder, no manual NSLayoutConstraint.

## RxSwift Patterns

**Input/Output in ViewModels:**
```swift
func transform(input: Input) -> Output {
    let items = input.refreshTrigger
        .flatMapLatest { [weak self] _ in
            self?.loadItems() ?? .empty()
        }
        .share(replay: 1)

    return Output(items: items, isLoading: activityIndicator.asObservable())
}
```

**Memory management:**
- Always use `DisposeBag` in ViewControllers and ViewModels
- Use weak self in closures: `[weak self]`
- Disposed by: `.disposed(by: disposeBag)`

## ECS & Enums

Use `MarketplaceECSHelper` for ECS enum mapping:
```swift
let categoryId = MarketplaceECSHelper.categoryId(from: ecsCode)
```

Regenerate enums with: `python bin/gen_ecs_enum.py`

## Common File Paths (CTInsertAd)

- **Models**: `AppFeatures/CTInsertAd/CTInsertAd/Domain/Models/`
- **UseCases**: `AppFeatures/CTInsertAd/CTInsertAd/Domain/UseCases/`
- **ViewModels**: `AppFeatures/CTInsertAd/CTInsertAd/Presentation/ViewModels/`
- **Services**: `AppFeatures/CTInsertAd/CTInsertAd/Data/Services/`
- **Tests**: `AppFeatures/CTInsertAd/CTInsertAd/Tests/`

## Localization Pattern

**Don't use**: `ctLocalize(for: "key", tableName: "Table")`
**Use instead**: `CTLocalize.module_specific_key()` from CTLocalize module

## Logging

Use `Logger.print()` from CTCommon instead of `print()`.

## File Headers

```swift
//
//  FileName.swift
//  Created by Vinh Nguyen on [date].
//  Copyright © 2024 Cho Tot. All rights reserved.
```

Use `time` MCP for the date, git config for your name.

## String Substitutions Supported

The skill can automatically detect and use:
- `{selectedText}` — Code snippet for pattern analysis and architectural review
- `{fileName}` — Current file name for module context detection
- `{filePath}` — Full path for accurate module identification

## When You're Stuck

- Check project memory at `~/.claude/projects/[project-name]/memory/`
- Refer to `AGENTS.md` for module-specific rules
- Look at examples in `Libraries/CTDesignSystem/CTDesignSystemExampleApp/SampleApp`
- Use ruler files in `.ruler/` for detailed guidance on architecture, code style, testing
