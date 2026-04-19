# Prompt — ct-feature-pipeline

> See [GUARDRAILS.md](GUARDRAILS.md) before executing any phase.
> Output formats are defined in [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md).

---

## Pre-flight — Validate Inputs & Ask for Missing Info (MANDATORY, runs BEFORE Step 0)

**Before initializing the Todo list or running any phase**, validate all required inputs.

For each missing or ambiguous parameter, **ask the user one question at a time** and wait for an answer before continuing.

### Required parameters checklist

| Parameter | Required | If missing → ask |
|-----------|----------|-----------------|
| `FEATURE_NAME` | Yes | "What is the feature name? (e.g. `POS Warning Message`)" |
| `JIRA_TICKET_URL` or `PRD_CONTENT` | At least one | "Please provide either a Jira ticket URL or paste the PRD content." |
| `MODULE_PATH` | Yes | "Which module path should the generated files go to? (e.g. `AppFeatures/CTJOB/CTJOB/Features/Warning`)" |
| `COMPONENT_TYPE` | Yes (if FIGMA_URL given) | "What UI component type? (`bottom_sheet` / `full_screen` / `modal` / `view_component` / `tableview_onesection` / `tableview_multisection`)" |

### Optional parameters checklist

| Parameter | If missing → inform user |
|-----------|--------------------------|
| `REFERENCE_USECASE` | "No `REFERENCE_USECASE` provided — Phase 2 will ask you clarifying questions about the API before generating code." |
| `FIGMA_URL` | "No `FIGMA_URL` provided — Phase 3 (UI generation) will be skipped." |

> ⚠️ Do NOT proceed to Step 0 until all REQUIRED parameters are confirmed.  
> Ask ONE missing item at a time. Do not batch all questions — wait for each answer before asking the next.

---

## Step 0 — Initialize Todo List (MANDATORY, do this FIRST)

**Before any analysis or output**, call `TodoWrite` to initialize all tasks:

```
TodoWrite([
  { content: "Phase 0 — Fetch PRD from Jira (skip if no JIRA_TICKET_URL)", status: "pending" },
  { content: "Phase 1 — Semantic Filter PRD",                    status: "pending" },
  { content: "Phase 2 — Generate 6-layer UseCase",               status: "pending" },
  { content: "Phase 3a — Generate ViewController + Storyboard",  status: "pending" },
  { content: "Phase 3b — Register files in project.pbxproj",     status: "pending" },
])
```

> If `JIRA_TICKET_URL` is not provided, mark Phase 0 as `completed` immediately and skip it.

**Lifecycle rules — STRICTLY ENFORCED:**

| Moment | Action |
|--------|--------|
| Before starting a phase | Call `TodoWrite` to set that task → `in_progress` |
| After finishing a phase | Call `TodoWrite` to set that task → `completed` |
| Only one task may be `in_progress` at a time | |

**If `TodoWrite` is not called, the phase MUST NOT start.**

Run each phase completely before starting the next. Print a progress header before each phase:

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🔵 PHASE [N]/3 — [Phase Name]
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

---

## Phase 0 — Fetch PRD from Jira (conditional)

**Condition:** Only run if `JIRA_TICKET_URL` is provided.

**Steps:**

1. Use `mcp_atlassian_mcp_getJiraIssue` to fetch the ticket:
   - Parse ticket key and host from `JIRA_TICKET_URL`
     - e.g. `https://701search.atlassian.net/browse/CRE-13492` → `cloudId: "https://701search.atlassian.net"`, `issueIdOrKey: "CRE-13492"`
   - Call: `mcp_atlassian_mcp_getJiraIssue(cloudId: "<host>", issueIdOrKey: "<key>", responseContentFormat: "markdown")`
2. Extract: `summary`, `description`, acceptance criteria from the response
3. Merge with any provided `PRD_CONTENT` (manual PRD supplements Jira content)
4. Store as `JIRA_PRD` in context — pass to Phase 1 as the primary input

### Phase 0 Clarifying Checkpoint

After fetching the Jira ticket, check for completeness before proceeding to Phase 1:

- If the ticket **description is empty or very short** (< 3 sentences): Ask the user —
  > "The Jira ticket `<KEY>` has a short description. Should I proceed with just this content, or would you like to paste additional PRD details?"
- If **no acceptance criteria** are found: Ask the user —
  > "No acceptance criteria found in the ticket. Can you provide them, or shall I derive them from the description?"
- If the ticket **cannot be fetched** (auth error, not found): Stop and ask —
  > "Could not fetch Jira ticket `<KEY>`. Please paste the PRD content manually."

> Wait for the user's answer before proceeding to Phase 1.

**Output:** See [OUTPUT_SCHEMA.md → Phase 0](OUTPUT_SCHEMA.md)

---

## Phase 1 — Semantic Filter PRD

**Goal:** Remove sensitive business data from PRD. Output a clean PRD for AI processing.

Apply the complete rules from:
@.claude/skills/ct-semantic-filter/SKILL.md

**Input:**
- `JIRA_PRD` from Phase 0 (if Jira was fetched), otherwise `PRD_CONTENT` from user input

**Output:** `FILTERED_PRD` — stored in context for Phase 2.

### Phase 1 Clarifying Checkpoint

After producing `FILTERED_PRD`, **present a structured summary** to the user and ask for confirmation before entering Phase 2:

```
📋 Phase 1 Summary — Filtered PRD
──────────────────────────────────
Feature: <FEATURE_NAME>
API Endpoints detected: <list or "none detected">
Input/Output types detected: <list or "unclear">
Key user stories: <bullet list>
Acceptance criteria: <bullet list or "none found">
Removed (sensitive): <count> items stripped

──────────────────────────────────
Before proceeding to UseCase generation, please confirm:
1. Are the API endpoints and data types above correct? (or provide corrections)
2. Anything missing from the acceptance criteria I should know?

Reply "ok" to continue, or correct any items above.
```

> ⚠️ Do NOT start Phase 2 until the user confirms this summary.

**End of Phase 1:** Print summary per [OUTPUT_SCHEMA.md → Phase 1](OUTPUT_SCHEMA.md).

---

## Phase 2 — UseCase Generation (6-Layer Implementation)

**Goal:** Generate complete 6-layer UseCase implementation based on the filtered PRD.

Apply the complete rules from:
@.claude/skills/ct-generate-usecase/SKILL.md

**Input:**
- `FILTERED_PRD` from Phase 1 as the feature specification
- Extract `USECASE_NAME` from `FEATURE_NAME` or PRD summary (e.g. "WarningMessage" → `CRWarningMessageUseCase`)
- `REFERENCE_USECASE` from user input
- `MODULE_PATH` from user input
- Auto-detect all other parameters following the auto-detection steps in the UseCase skill

### Phase 2 Clarifying Checkpoint

Before writing any code, **auto-detect all 6-layer parameters**, then present them to the user for confirmation:

```
🔧 Phase 2 Plan — UseCase Generation
──────────────────────────────────────
UseCase name:     <detected or derived>
API endpoint:     <detected from PRD or "❓ unclear">
HTTP method:      <GET / POST / PUT / DELETE or "❓ unclear">
Input type:       <detected or "❓ unclear">
Output type:      <detected or "❓ unclear">
Generic wrapper:  <detected (e.g. CRModelCommon) or "none">
Target class:     <detected file path>
Service class:    <detected file path>
Repository class: <detected file path>
ViewModel class:  <detected file path>
──────────────────────────────────────
```

For each item marked **❓ unclear**, ask the user ONE question at a time:

- `API endpoint unclear` → "What is the API endpoint for this feature? (e.g. `POST /api/v1/warning/message`)"
- `HTTP method unclear` → "Is this a GET (fetch) or POST/PUT/DELETE (mutation) operation?"
- `Input type unclear` → "What input does this API take? (e.g. `String adId`, a model object, or no input?)"
- `Output type unclear` → "What does the API return? (e.g. a list, a single object, or just a success/failure bool?)"
- `Target/Service/Repository unclear` → "Could not find the existing Target/Service/Repository files. Please confirm the module folder — e.g. `AppFeatures/CTCorePayment/CTCorePayment/Data/`"

> Ask ONE question at a time. Wait for the user's answer before asking the next.  
> Once all items are resolved, show the updated plan and ask: "Does this look correct? Reply 'ok' to generate the 6-layer UseCase."

> ⚠️ Do NOT write any code until the user confirms the plan.

**Output:** Modified/created files across 6 layers (NetworkHelper → Target → Service → Repository → UseCase → ViewModel).

**End of Phase 2:** Print summary per [OUTPUT_SCHEMA.md → Phase 2](OUTPUT_SCHEMA.md).

---

## Phase 3 — UI Generation (ViewController + Storyboard)

**Goal:** Generate production-ready ViewController + Storyboard from the Figma design.

Apply the complete rules from:
@.claude/skills/ct-figma-storyboard/SKILL.md

**Input:**
- `FIGMA_URL` from user input
- `MODULE_PATH` from user input
- `COMPONENT_TYPE` from user input
- ViewModel class name from Phase 2 output (for IBAction/callback wiring hints)

---

### ⚠️ Phase 3 Checkpoint — MANDATORY PAUSE BEFORE CODE GENERATION

This phase **MUST** follow the steps below in order. Do NOT skip any step.

#### Step 3.0 — Fetch & Display Figma Design (always run)

1. Extract `file_key` and `node_id` from `FIGMA_URL`
2. Call **both in parallel**:
   - `mcp__figma__get_design_context(file_key, node_id, depth=4)`
   - `mcp__figma__get_screenshot(file_key, node_id)`
3. **Display the screenshot** and a summary of the design context (sections, components, colors, typography) to the user

#### Step 3.1 — Clarifying Questions (MANDATORY, ask BEFORE writing code)

After displaying the Figma content, **stop and ask the user** these required questions:

```
Before generating the ViewController and Storyboard, please confirm:

1. **Button actions**: What does each button/close icon do? (dismiss, navigate, callback to ViewModel?)
2. **File name**: What should the ViewController and storyboard be named? (e.g. `JBWarningMessage`)
3. **Subfolder**: Which subfolder within MODULE_PATH? (confirm or clarify if ambiguous)

Reply to proceed with code generation.
```

> ⚠️ Do NOT generate any Swift or storyboard code before the user answers Step 3.1.

> **Pipeline mode overrides:**
> - Skip Step 1c from `ct-figma-storyboard` ("Would you like me to search for existing components?") — proceed directly to creating a new component from scratch
> - Step 3.1 above REPLACES Step 2 of `ct-figma-storyboard` — do not run Step 2 separately

---

**Mandatory pre-code step — Read Reference Swift File:**

Before writing any ViewController code, use the `Read` tool to read the canonical reference Swift ViewController for the given `COMPONENT_TYPE` (see the table in `ct-figma-storyboard/SKILL.md → Step 3`). Extract and apply the code patterns documented in **Step 3a** of that skill (data binding approach, section model, relay type, cell enum, selection handling, imports). Do NOT generate code before this read is complete.

**Output:**
- `<Name>ViewController.swift`
- `<Name>.storyboard`
- 5 `project.pbxproj` entries

**End of Phase 3:** Print summary per [OUTPUT_SCHEMA.md → Phase 3](OUTPUT_SCHEMA.md).

---

## Final Summary

Print the complete pipeline summary per [OUTPUT_SCHEMA.md → Final Summary](OUTPUT_SCHEMA.md).

---

## Fallback: If @-references Do Not Resolve

If the `@.claude/skills/...` lines above did not expand inline, use the `Read` tool to load each sub-skill before executing that phase:

```
Phase 0: Use mcp_atlassian_mcp_getJiraIssue directly (no sub-skill needed)
Phase 1: Read /Users/hai.phan/Desktop/haiphan/ct-ios-app--v3/.claude/skills/ct-semantic-filter/SKILL.md
Phase 2: Read /Users/hai.phan/Desktop/haiphan/ct-ios-app--v3/.claude/skills/ct-generate-usecase/SKILL.md
Phase 3: Read /Users/hai.phan/Desktop/haiphan/ct-ios-app--v3/.claude/skills/ct-figma-storyboard/SKILL.md
```

Then apply each skill's rules exactly as written.
