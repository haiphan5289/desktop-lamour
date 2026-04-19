---
name: ct-chain-of-thought
description: Systematic step-by-step technical design analysis for complex iOS features in Cho Tot. Use when designing a new feature or solving a complex problem that requires thorough reasoning across requirements, architecture, data flow, edge cases, testing, and implementation roadmap following MVVM + Clean Architecture.
model: sonnet
effort: high
---

# iOS Chain of Thought - Technical Design Analysis

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

## Overview

This skill provides a systematic Chain of Thought analysis framework for complex iOS development problems in the Chợ Tốt app. It breaks down problems into logical steps covering requirement analysis, architecture design, data flow, edge cases, testing strategy, and implementation roadmap.

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
CONTEXT: [Context and module in Cho Tot app]
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
- Consider Vietnamese marketplace-specific requirements

### 2. 🧩 Architecture Design (MVVM + Clean Architecture)
- Break down feature into layers: View → ViewModel → UseCase → Repository → Service → API Target
- Explain responsibility of each layer and communication patterns
- Identify dependency injection points (Swinject / CCDefaultAssembler)
- Note CTDesignSystem integration requirements (DSLabel, DSButton, DSTextField, etc.)

### 3. 🔄 Data Flow & Logic (Step-by-Step)
- Trace full lifecycle: user action → ViewModel → UseCase → Repository → API → Model → UI update
- Include loading, success, and error state handling
- Detail data transformation between layers
- Reference RxSwift patterns: BehaviorRelay, PublishRelay, flatMapLatest, observe(on:)

### 4. 🧪 Edge Cases & Failure Handling
- List 4–6 possible edge cases or error scenarios
- Propose graceful handling strategies for each
- Consider offline scenarios and data persistence
- Plan for Vietnamese localization edge cases

### 5. 🧰 Testing & Validation Plan
- Suggest 3–5 key unit tests or integration tests using Quick/Nimble
- Explain how to validate business logic and network responses
- Describe mock strategies for dependencies
- Note key UI testing scenarios

### 6. 📦 Implementation Roadmap
- Summarize the step-by-step implementation plan
- Highlight risks, technical debt, and scalability considerations
- Identify potential performance bottlenecks
- Consider future extensibility requirements

---

## Code Standards to Follow

- Use `CTDesignSystem` components (DSLabel, DSButton, DSTextField, DSImageView)
- Use `SnapKit` for all Auto Layout constraints
- Use `RxSwift` with `DisposeBag` for subscriptions
- Use `Logger.print()` instead of `print()` for logging
- Follow MVVM + Clean Architecture layer separation
- Use `Swinject` / `CCDefaultAssembler` for DI

## Output Style

Think aloud and explain reasoning before the final summary. The output should read like a **senior engineer walking through a design document** before coding — not just a list of bullet points.

❗️ **Important:** Do not jump to code immediately. Analyze first, then provide implementation details only after the full analysis is complete.

---

## Example Analysis

### Sample Input

```
FEATURE_TO_ANALYZE: Fetch and display a list of vouchers from an API with caching
CONTEXT: CTReward module - user loyalty and voucher management
COMPLEXITY_LEVEL: Medium
FOCUS_AREAS: Performance optimization, offline support
```

---

### 1. 🧭 Requirement Analysis

**Functional assumptions:**
- User opens the voucher list screen and sees all available vouchers
- Vouchers are fetched from a remote API (paginated or full list)
- Each voucher shows: title, discount value, expiry date, status (available/used/expired)
- User can tap a voucher to view details or apply it

**Non-functional assumptions:**
- Response time < 2s on 4G; skeleton loading shown while fetching
- Vouchers are cached locally so the list is visible offline (stale-while-revalidate)
- Vietnamese localization required (discount labels, date formats)

**Constraints:**
- Network: REST API, JSON response
- Caching: In-memory + disk (UserDefaults or CoreData)
- Offline: Show cached data with a "last updated" indicator

---

### 2. 🧩 Architecture Design (MVVM + Clean Architecture)

```
VoucherListViewController   → DSTableView, DSLabel, DSButton (CTDesignSystem)
        ↓ trigger (PublishRelay)
VoucherListViewModel        → Transforms domain data, manages loading/error state
        ↓ executes
FetchVouchersUseCase        → Business logic: validate, filter expired vouchers
        ↓ delegates
VoucherRepository           → Abstract interface (protocol)
        ↓ implements
VoucherRepositoryImpl       → Checks cache → fetches API → updates cache
        ↓ calls
VoucherService              → VoucherTarget.execute() → Observable<[Voucher]>
        ↓ hits
VoucherTarget               → GET /api/v1/vouchers → Requestable
```

DI: `VoucherAssembler` registers all layers via `CCDefaultAssembler`.

---

### 3. 🔄 Data Flow & Logic

1. `viewDidLoad` → ViewController emits `fetchTrigger` (PublishRelay)
2. ViewModel subscribes to `fetchTrigger`, sets `isLoadingRelay.accept(true)`
3. ViewModel calls `fetchVouchersUseCase.action?.execute(())`
4. UseCase calls `repository.fetchVouchers()` → delegates to service
5. Service calls `VoucherTarget` → network request → decodes `[VoucherModel]`
6. Repository checks cache: if fresh → return cached; else fetch → update cache
7. UseCase filters expired vouchers, maps to domain `Voucher` entities
8. `action?.elements` emits → ViewModel maps to `[VoucherCellViewModel]`
9. ViewModel calls `presenter?.vouchersRelay.accept(cellViewModels)`
10. ViewController binds relay → `tableView.reloadData()` via RxDataSources

**Error path:** `action?.underlyingError` → `presenter?.errorRelay.accept(message)`

---

### 4. 🧪 Edge Cases & Failure Handling

| Edge Case | Handling Strategy |
|---|---|
| Empty API response | Show empty state view with "Bạn chưa có voucher nào" |
| Network timeout | Show cached data + toast "Đang hiển thị dữ liệu offline" |
| All vouchers expired | Filter on UseCase layer, show "Không có voucher khả dụng" |
| Malformed JSON | Decode safely with optional fields, log with `Logger.print()` |
| Token expired (401) | Propagate auth error → redirect to login via router |
| Pagination failure mid-scroll | Retry button in footer cell |

---

### 5. 🧰 Testing & Validation Plan

```swift
// Quick/Nimble unit tests

describe("FetchVouchersUseCase") {
    context("when repository returns valid vouchers") {
        it("filters expired vouchers and emits only active ones") { }
    }
    context("when repository throws network error") {
        it("propagates error via underlyingError") { }
    }
}

describe("VoucherListViewModel") {
    context("when fetchTrigger fires") {
        it("sets isLoading to true then false after response") { }
        it("maps VoucherModel to VoucherCellViewModel correctly") { }
    }
}

describe("VoucherRepository") {
    context("when cache is fresh") {
        it("returns cached data without hitting the network") { }
    }
}
```

---

### 6. 📦 Implementation Roadmap

| Step | Task | Risk |
|---|---|---|
| 1 | Add `GET /api/v1/vouchers` to `NetworkHelper` + `VoucherTarget` | Low |
| 2 | Implement `VoucherService` + `VoucherRepository` with cache | Medium |
| 3 | Create `FetchVouchersUseCase` with expiry filter logic | Low |
| 4 | Build `VoucherListViewModel` with RxSwift bindings | Low |
| 5 | Build `VoucherListViewController` using CTDesignSystem + SnapKit | Low |
| 6 | Register all layers in `VoucherAssembler` | Low |
| 7 | Write unit tests for UseCase and ViewModel | Medium |

**Scalability note:** If voucher count grows large, switch to server-side pagination with `loadMoreTrigger` relay.
