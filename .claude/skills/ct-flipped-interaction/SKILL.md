---
name: ct-flipped-interaction
description: Ask clarifying questions before implementing any iOS feature in Cho Tot. Use when the user provides a vague or incomplete feature request and you need to gather full requirements — scope, API contracts, UX expectations, business rules, and performance constraints — before writing any code.
model: sonnet
effort: medium
---

# iOS Flipped Interaction - Ask Before Implementing

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

## Overview

This skill implements the **Flipped Interaction Pattern** for iOS development in the Chợ Tốt app. Instead of immediately proposing solutions, the AI asks systematic clarifying questions first to fully understand the requirements before writing any code.

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
5. **Always consider Vietnamese marketplace context** when relevant

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
- How does this feature relate to Chợ Tốt's marketplace business?
- Are there Vietnamese localization requirements?
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
- **CTDesignSystem** components (DSLabel, DSButton, DSTextField, DSImageView)
- **SnapKit** for all UI layout constraints
- **RxSwift** for reactive programming (BehaviorRelay, PublishRelay, DisposeBag)
- **Swinject / CCDefaultAssembler** for dependency injection
- **Logger.print()** for logging (never `print()`)

❗️ **Important:** Do not provide any code or architecture proposal until the user confirms all requirements are understood.

---

## Example Interaction

### Sample Input

```
FEATURE_REQUEST: Add a favorites list for products
CONTEXT: Users want to save interesting items for later
PRIORITY: Low
```

---

### AI Behavior (Low Priority → thorough exploration)

**Question 1 — Scope:**

> Before I propose anything, I want to make sure I understand the full scope.
>
> When a user "favorites" a product, should that list be:
> - Stored **locally only** (disappears if the app is reinstalled), or
> - **Synced to a backend** so it persists across devices?
>
> This will determine whether we need a new API endpoint or just local persistence.

---

**User answers:** "Synced to backend, we have an existing wishlist API."

---

**Question 2 — API Contract:**

> Great. A few questions about the API:
> - What is the endpoint and HTTP method for adding/removing a favorite? (e.g. `POST /api/v1/wishlist`)
> - Does the fetch endpoint return full product objects or just product IDs (requiring a second fetch)?
> - Is there a maximum limit on favorites per user?

---

**User answers:** `POST /api/v1/wishlist/add`, `DELETE /api/v1/wishlist/remove/{id}`, fetch returns full objects, no limit.

---

**Question 3 — UX & Entry Points:**

> Where can the user add a product to favorites?
> - Only from the product detail screen, or
> - Also from feed/listing cards (inline heart button)?
>
> And should the favorites list have its own tab/screen, or is it accessible from the profile/account section?

---

**User answers:** "Heart button on feed cards AND detail screen. Accessible from profile tab."

---

**Question 4 — Offline & Sync:**

> When the user taps the heart button offline:
> - Should it **optimistically update** the UI and sync when back online, or
> - Show an error and require network connectivity?

---

**User answers:** "Optimistic update preferred."

---

### Confirmed Understanding (AI summarizes before implementing)

> Let me confirm what I'll implement:
>
> - Heart button on feed cards + product detail screen — taps call `POST /api/v1/wishlist/add` or `DELETE /api/v1/wishlist/remove/{id}`
> - Optimistic UI update on tap; sync error silently retried
> - Favorites list screen accessible from Profile tab, showing full product objects
> - `FavoritesRepository` with local cache + API sync; `FavoritesUseCase` for add/remove/fetch logic
> - CTDesignSystem components (DSButton for heart, DSTableView for list)
>
> Does this match your expectations? Shall I start implementation?

---

### Priority Impact Summary

| Priority | Questions Asked | Scope |
|---|---|---|
| **High** | 1–2 focused questions on MVP requirements only | Fastest path using existing patterns |
| **Medium** | 3–4 questions covering business logic + edge cases | Balanced implementation with some new components |
| **Low** | 4–5 thorough questions including scalability + UX | Optimal architecture, may propose refactoring |
