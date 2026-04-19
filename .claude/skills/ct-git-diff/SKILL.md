---
name: ct-git-diff
description: "Compare current branch against a target branch or commit SHA. Produces layered output: structured analysis by architecture layer, CT iOS review checklist, and a narrative summary. Suggests next steps without auto-chaining."
argument-hint: "[branch|SHA] [--full] [--path <dir>] [--focus <layer>] [--since <date>] [--limit <n>]"
---

# ct-git-diff — Git Diff Skill for Chợ Tốt iOS

Compare the **current branch** against a target branch or commit SHA and produce a structured, layered analysis tailored to the Chợ Tốt iOS codebase.

---

## How to Use

Invoke as a slash command from any chat. The skill always compares **your current branch** against a base branch.

```
/ct-git-diff                    # simplest — base branch auto-detected (main, then dev, then master)
/ct-git-diff main               # compare your branch against main
/ct-git-diff abc1234            # compare against a specific commit SHA
```

Before running the diff, Claude prints a one-line confirmation so you can verify it picked the right branches:

```
Comparing: your-branch → main   (3 commits ahead)
```

**Optional filters** (add any combination):

| Flag | What it does | Example |
|------|-------------|---------|
| `--full` | Show every added/removed line instead of just a summary | `/ct-git-diff main --full` |
| `--path <dir>` | Only look at files inside this folder | `/ct-git-diff main --path AppFeatures/CTPos` |
| `--focus <keyword>` | Only show files whose path contains this word | `/ct-git-diff main --focus ViewModels` |
| `--since <date>` | Only include commits made after this date | `/ct-git-diff main --since 2024-01-01` |
| `--limit <n>` | Stop after N files (default 100) | `/ct-git-diff main --limit 30` |

Combined example:
```
/ct-git-diff main --path AppFeatures/CTChat --focus ViewModels --limit 20
```

The full prompt is defined in **[.github/prompts/ct-ai-git-diff.prompt.md](../../../.github/prompts/ct-ai-git-diff.prompt.md)**.

---

## Output (4 Layers)

| Layer | Content |
|-------|---------|
| 1 — Diff Summary | Files changed per architecture layer (Presentation / Domain / Data / Tests) |
| 2 — Structured Analysis | Per-file CT iOS violations (DSLabel, SnapKit, DisposeBag, etc.) |
| 3 — Review Checklist | Architecture, CT Design System, RxSwift, Code Quality — ✅ / ❌ / ➖ |
| 4 — Narrative | Plain-language PR description draft with risk areas and missing pieces |

Followed by suggested next steps (`/review-code`, `/ct-unittest`, `/ct-bugfix-skill`) — never auto-invoked.

---

## File Structure

| File | Purpose |
|------|---------|
| [.github/prompts/ct-ai-git-diff.prompt.md](../../../.github/prompts/ct-ai-git-diff.prompt.md) | **Canonical prompt** — consolidates all spec files |
| [spec/INPUT_SCHEMA.md](spec/INPUT_SCHEMA.md) | Invocation syntax and flag reference |
| [spec/OUTPUT_SCHEMA.md](spec/OUTPUT_SCHEMA.md) | Layered output format and final confirmation |
| [spec/PROMPT.md](spec/PROMPT.md) | Step-by-step execution workflow (Steps 1–6) |
| [spec/EXAMPLES.md](spec/EXAMPLES.md) | Worked example output |
| [spec/EVAL.md](spec/EVAL.md) | Output quality checklist |
| [spec/GUARDRAILS.md](spec/GUARDRAILS.md) | Anti-hallucination rules and common pitfalls |
| [spec/POSTPROCESS.md](spec/POSTPROCESS.md) | Suggested next steps after analysis |
| [CHANGELOG.md](CHANGELOG.md) | Version history |
