---
name: ct-semantic-filter
description: Semantically filter and clean PRD content for Cho Tot iOS development. Removes sensitive business data (revenue, internal metrics, competitor info) while preserving all technical requirements (API specs, user stories, UI/UX flows, validation rules). Use BEFORE passing a PRD to any other skill (ct-generate-usecase, ct-figma-implement-design, ct-quality-engineer) to strip confidential content first.
---

# [CT] Semantic Filter — PRD Cleaner for iOS Development

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Filters raw PRD content to remove sensitive business information while preserving everything needed for MVVM + Clean Architecture implementation.

---

## Input Format

```
📄 RAW PRD CONTENT:
"""
[Paste your complete, unfiltered PRD content here]
"""
```

---

## Filter Rules

### 🔒 REMOVE or ANONYMIZE

| Category | Examples |
|---|---|
| Sensitive business data | Revenue numbers, GMV targets, conversion rates, user counts |
| Internal team info | Employee names, team names, org structure, stakeholder lists |
| Competitive intel | Competitor names, market share data, benchmark comparisons |
| Financial details | Budget, cost breakdowns, pricing strategy, margins |
| Legal/compliance specifics | Regulatory opinions, legal entity names, specific law references |
| Internal processes | Approval workflows, review cycles, release schedules |

### ✅ PRESERVE

| Category | Examples |
|---|---|
| User stories | "As a user, I want to..." |
| API specifications | Endpoints, HTTP methods, request/response schemas |
| Functional requirements | Feature behaviors, system responses, validation rules |
| UI/UX specifications | Screen layouts, component requirements, navigation flows |
| Error handling | Error states, fallback behaviors, edge cases |
| Performance requirements | Load times, response time SLAs, pagination sizes |
| Security specifications | Auth flows (OAuth, JWT), data encryption requirements |
| Platform requirements | iOS version, device constraints, offline capability |

---

## iOS-Specific Preservation Focus

When filtering, ensure these are **always kept intact**:

### MVVM + Clean Architecture
- ViewModel input/output specifications
- Data binding requirements between layers
- Model definitions, field names, and types
- UseCase business logic descriptions

### CTDesignSystem Integration
- UI component specifications (buttons, labels, text fields, cards)
- Loading states, empty states, error states
- Theming and accessibility requirements

### API & Data Layer
- Endpoint paths and HTTP methods
- Request parameters and response schemas
- Pagination, sorting, filtering specs
- Caching and offline requirements

### User Experience Flows
- Navigation patterns (push, modal, bottom sheet)
- User input validation rules
- Loading → Success → Error state transitions
- Deep link and notification handling

---

## Output Format

After filtering, output the cleaned PRD in this structure:

```
✅ FILTERED PRD — [Feature Name]

## Summary
[1–2 sentence description of the feature, business context anonymized]

## User Stories
[Preserved user-facing requirements]

## Functional Requirements
[All technical behaviors, system responses, edge cases]

## UI/UX Specifications
[Screen layouts, component specs, navigation flows]

## API Specifications
[Endpoints, data schemas, integration points]

## Validation & Error Handling
[Input rules, error states, fallback behaviors]

## Platform & Performance
[iOS-specific constraints, load time SLAs, pagination]

---
🔒 Removed: [brief list of what was stripped — e.g., "Revenue targets, team names, competitor data"]
✅ Preserved: [brief list of what was kept for development use]
```

---

## Example

**Input:**
```
📄 RAW PRD CONTENT:
"""
# Payment Gateway Integration - Q4 Revenue Initiative

## Business Context
Our revenue team (John Smith, Sarah Lee) identified that 60% of users abandon checkout due to payment friction. Market research shows competitors like Shopee achieve 85% conversion rates. We need to increase GMV from $2M to $3.5M by Q4.

## Technical Requirements
- Users can add funds to e-wallet using credit cards
- Real-time balance updates via WebSocket connection at wss://api.chotot.com/wallet
- Payment flow uses OAuth 2.0 authentication
- Transaction history with pagination (20 items per page)
- Offline capability for viewing recent transactions (last 50 cached locally)

## UI Requirements
- Bottom sheet for payment method selection
- Loading spinner during transaction processing
- Success/failure states with retry option
"""
```

**Output:**
```
✅ FILTERED PRD — Payment Gateway Integration

## Summary
Users need a frictionless payment experience. This feature adds digital wallet (e-wallet) funding via credit card to reduce checkout drop-off.

## User Stories
- As a user, I want to add funds to my e-wallet using a credit card
- As a user, I want to see my balance update in real time after a transaction
- As a user, I want to view my transaction history while offline

## Functional Requirements
- Real-time balance updates via WebSocket connection at wss://api.chotot.com/wallet
- Transaction history with pagination (20 items per page)
- Offline capability: last 50 transactions cached locally
- Payment flow protected with OAuth 2.0 authentication

## UI/UX Specifications
- Bottom sheet for payment method selection
- Loading spinner during transaction processing
- Success state: confirmation message + updated balance
- Failure state: error message + retry button

## API Specifications
- WebSocket: wss://api.chotot.com/wallet (real-time balance)
- Authentication: OAuth 2.0
- Pagination: 20 items per page

## Validation & Error Handling
- Retry option on payment failure
- Graceful degradation to cached data when offline

## Platform & Performance
- Offline support: cache last 50 transactions locally

---
🔒 Removed: Revenue targets ($2M→$3.5M), employee names (John Smith, Sarah Lee), competitor benchmark (Shopee 85%)
✅ Preserved: All API specs, WebSocket URL, OAuth flow, pagination size, UI states, offline cache requirement
```

---

## Recommended Workflow

Use this skill **before** passing PRD content to other skills:

```
1. /ct-semantic-filter    ← strip confidential data first
2. /ct-generate-usecase   ← generate UseCase from clean PRD
3. /ct-quality-engineer   ← validate implementation against clean PRD
4. /ct-figma-implement-design ← implement UI from clean specs
```

This ensures sensitive business data never enters the generation pipeline.

---

## Notes

- If no sensitive data is found, output the PRD unchanged with a note: `✅ No sensitive data detected — PRD passed through unchanged.`
- If the PRD is ambiguous (data could be sensitive or technical), **preserve it** and flag it: `⚠️ Flagged for review: [field] — may contain sensitive data.`
- Never invent or hallucinate technical requirements. Only filter; never add.
