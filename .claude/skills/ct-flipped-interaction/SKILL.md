---
name: ct-flipped-interaction
description: Ask clarifying questions before implementing any C#/.NET WPF feature. Use when the user provides a vague or incomplete feature request and you need to gather full requirements — scope, API contracts, UX expectations, business rules, and performance constraints — before writing any code.
model: sonnet
effort: medium
---

# WPF Flipped Interaction - Ask Before Implementing

## Overview

This skill implements the **Flipped Interaction Pattern** for C#/.NET WPF development. Instead of immediately proposing solutions, the AI asks systematic clarifying questions first to fully understand the requirements before writing any code.

## When to Use This Skill

**Use this skill when:**
- The feature request is vague or underspecified
- Requirements, API contracts, or data models are unclear
- Integration points with existing modules are unknown
- UX expectations or design mockups haven't been shared
- Business rules and validation logic need confirmation
- You want to avoid rework from incorrect assumptions

## Input Format

```
FEATURE_REQUEST: [Feature description]
CONTEXT: [Context and reason for this feature]
PRIORITY: [High / Medium / Low]
```

## Priority Field Behavior

The **PRIORITY** field shapes how the AI asks questions and proposes solutions:

- **High**: Focus on fastest, lowest-risk solutions. Target minimum viable requirements. Prefer existing components and patterns. Suggest incremental implementation.
- **Medium**: Balance speed vs. quality. Cover complete business logic and edge cases. May suggest new component creation if needed.
- **Low**: Explore optimal, future-proof solutions. Include scalability and optimization details. May propose comprehensive refactoring.

## Flipped Interaction Rules

**🚨 CRITICAL: Follow these rules strictly**

1. **Ask clarifying questions FIRST** — do not propose any implementation
2. **DO NOT assume** requirements not explicitly stated
3. **DO NOT provide code** until all requirements are crystal clear
4. **DO NOT start implementation** until confirmed understanding is 100%

## Information Categories to Gather

Systematically ask about these areas before implementing:

### 1. Feature Scope & Requirements
- What is the exact functionality expected?
- What are the user stories and acceptance criteria?
- What are the edge cases and error scenarios?

### 2. Technical Integration
- Which existing modules or components need integration?
- What are the API contracts and data models?
- Are there authentication or permission requirements?

### 3. User Experience
- What is the expected user flow?
- Are there specific design requirements or Figma mockups?
- What accessibility considerations are needed?

### 4. Business Context
- How does this feature relate to the application's core business domain?
- Are there localization requirements (`.resx` resource files)?
- What are the business rules and validation logic?

### 5. Performance & Constraints
- What are the performance expectations?
- Are there data volume or caching considerations?
- What are the timeline and resource constraints?

## Questioning Strategy

- Ask **one essential question at a time** to avoid overwhelming the user
- After each answer, determine if more clarification is needed or if implementation can begin
- Adjust question depth based on PRIORITY: High → fewer, focused questions; Low → thorough exploration
- Confirm your understanding before writing any code

## Architecture Requirements for Implementation

Once requirements are confirmed, all implementations must follow:
- **MVVM + Clean Architecture** (Presentation → Domain → Data layers)
- **AppDesignSystem** components (`AppLabel`, `AppButton`, `AppTextField`, `AppImage`)
- **XAML** for all UI layout (`Grid`, `StackPanel`, `DockPanel`, `Border`)
- **CommunityToolkit.Mvvm** for reactive binding (`[ObservableProperty]`, `[RelayCommand]`)
- **Microsoft.Extensions.DependencyInjection** for dependency injection
- **`ILogger<T>`** for all logging (never `Console.WriteLine` directly)

❗️ **Important:** Do not provide any code or architecture proposal until the user confirms all requirements are understood.

---

## Example Interaction

### Sample Input

```
FEATURE_REQUEST: Add a bookmarks list for records
CONTEXT: Users want to save interesting items for later review
PRIORITY: Low
```

---

### AI Behavior (Low Priority → thorough exploration)

**Question 1 — Scope:**

> Before I propose anything, I want to make sure I understand the full scope.
>
> When a user "bookmarks" a record, should that list be:
> - Stored **locally only** (using `IMemoryCache` or SQLite), or
> - **Synced to a backend** REST API so it persists across machines/sessions?
>
> This determines whether we need a new API call or just local-only persistence.

---

**User answers:** "Synced to backend — we have an existing bookmarks API."

---

**Question 2 — API Contract:**

> A few questions about the API:
> - What is the endpoint and HTTP method for adding/removing a bookmark? (e.g. `POST /api/v1/bookmarks`)
> - Does the fetch endpoint return full record objects or just IDs (requiring a second fetch)?
> - Is there a maximum limit on bookmarks per user?

---

**User answers:** `POST /api/v1/bookmarks/add`, `DELETE /api/v1/bookmarks/remove/{id}`, fetch returns full objects, no limit.

---

**Question 3 — UX & Entry Points:**

> Where can the user add a record to bookmarks?
> - Only from the record detail view, or
> - Also from list items (inline bookmark button in a `DataTemplate`)?
>
> And should the bookmarks list be a separate navigation page, or accessible from a sidebar/account section?

---

**User answers:** "Bookmark button on list items AND detail view. Accessible from the sidebar."

---

**Question 4 — Offline & Sync:**

> When the user clicks the bookmark button with no network connection:
> - Should it **optimistically update** the UI and sync when connectivity is restored, or
> - Show an error dialog requiring network connectivity?

---

**User answers:** "Optimistic update preferred."

---

### Confirmed Understanding (AI summarizes before implementing)

> Let me confirm what I'll implement:
>
> - Bookmark button in list `DataTemplate` + record detail `UserControl` — clicks call `POST /api/v1/bookmarks/add` or `DELETE /api/v1/bookmarks/remove/{id}`
> - Optimistic UI update on click; sync error silently retried with background `Task`
> - Bookmarks page accessible from sidebar, displaying full record objects in a `ListView`
> - `BookmarksRepository` with `IMemoryCache` + API sync; `BookmarksUseCase` for add/remove/fetch logic
> - AppDesignSystem: `AppButton` for bookmark toggle, `AppLabel` for list items
> - xUnit + Moq unit tests for `BookmarksViewModel` and `BookmarksUseCase`
>
> Does this match your expectations? Shall I start implementation?

---

### Priority Impact Summary

| Priority | Questions Asked | Scope |
|---|---|---|
| **High** | 1–2 focused questions on MVP requirements only | Fastest path using existing patterns |
| **Medium** | 3–4 questions covering business logic + edge cases | Balanced implementation with some new components |
| **Low** | 4–5 thorough questions including scalability + UX | Optimal architecture, may propose refactoring |
