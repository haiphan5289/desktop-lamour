# Guardrails — ct-feature-pipeline

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code.
> See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

---

## Phase Ordering

- **Never skip Phase 1** — always filter PRD before passing to UseCase generation
- Phase 0 is conditional — only run if `JIRA_TICKET_URL` is provided
- Phases must execute in order: 0 → 1 → 2 → 3
- If `FIGMA_URL` is not provided, skip Phase 3 and note it in the final summary
- If `REFERENCE_USECASE` is not provided, Phase 2 will ask clarifying questions per `ct-generate-usecase` rules

## Context Carry-Forward

- `FILTERED_PRD` from Phase 1 is the **source of truth** for Phase 2 — do not use raw `PRD_CONTENT`
- ViewModel class name from Phase 2 feeds into Phase 3 for IBAction/callback wiring hints
- `USECASE_NAME` derived in Phase 2 is used in Phase 3 for naming consistency

## TodoWrite — Non-Negotiable

- `TodoWrite` **MUST** be called before each phase starts (set to `in_progress`)
- `TodoWrite` **MUST** be called after each phase ends (set to `completed`)
- Only one task may be `in_progress` at a time
- **If `TodoWrite` is not called, the phase MUST NOT start**

## File Modification Rules

- Only modify existing files — do not create new Swift files unless explicitly approved
- Do not create new module directories or AGENTS.md files without explicit approval
- Follow existing naming and patterns in the target module

## Design System Tokens — NEVER Invent (MANDATORY)

**Do NOT fabricate DS token names.** Always verify every token against the actual codebase before using it.

- **Before using any `theme.*` property**, grep `CMBorder`, `CMBackground`, `CMText`, etc. in `Libraries/CTDesignSystem/CTDesignSystem/Theme/CMDefaultTheme.swift` to confirm the exact member name.
- **Before using any `DS.TypoToken.*` or `DS.Button.*`**, search the CTDesignSystem source to confirm it exists.
- **Never guess** token names like `borderPrimary`, `textPrimary`, `backgroundPrimary` without verification — they may not exist.

Verified `CMBorder` members (as of last check):
`borderDivider`, `borderThin`, `borderOverlay`, `borderRegular`, `borderBold`, `borderActive`, `borderBlank`, `borderBrand`, `borderInfo`, `borderSuccess`, `borderError`, `borderWarning`, `borderDisabled`, `borderBlack`

If unsure → grep the member before writing. A compile error from a fake token wastes a full build cycle.

---

## Safe Array Subscript — Use `.safe[index]` NOT `[safe: index]` (MANDATORY)

The codebase provides a `.safe` property extension on `Collection` for safe index access. Always use the property form, never the labeled subscript form.

```swift
// ❌ BAD — compile error: "Extraneous argument label 'safe:' in subscript"
let option = sources.value[safe: indexPath.row]

// ✅ GOOD — correct pattern
let option = sources.value.safe[indexPath.row]
```

This applies everywhere safe array access is needed. The labeled form `[safe:]` does not exist in this codebase.

---

## String Encoding — Vietnamese UTF-8 (MANDATORY)

Always write Vietnamese strings as direct UTF-8 literals. **Never use `\u{XXXX}` escape sequences** for human-readable text.

```swift
// ❌ BAD — \u{1eedi} is invalid: 'i' is not a hex digit
submitButton.setTitle("G\u{1eedi} th\u{00f4}ng tin", for: .normal)

// ✅ GOOD — direct UTF-8 literal, no escaping needed
submitButton.setTitle("Gửi thông tin", for: .normal)
```

`\u{XXXX}` is only acceptable for invisible/control characters (e.g., `\u{200B}` zero-width space) where the literal form is not legible.

## Jira MCP Fallback

If `JIRA_TICKET_URL` is unreachable:
- Fall back to manual `PRD_CONTENT`
- Print the warning message defined in [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md)
- Do not block Phase 1 — continue with whatever PRD content is available
