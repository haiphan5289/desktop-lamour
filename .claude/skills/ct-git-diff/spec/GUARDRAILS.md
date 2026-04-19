# Guardrails — ct-git-diff

> **Anti-Hallucination:** Never fabricate file paths, line numbers, function names, or token names. Only report findings that are directly visible in the actual diff output from git commands.

---

## Core Rules

### 1. Never Hallucinate Diff Content

- Run actual `git diff` / `git log` commands and report only what they return.
- Do NOT invent changed files, line numbers, or code snippets that are not in the git output.
- If a git command fails or returns empty output, report that clearly instead of fabricating content.

### 2. Never Auto-Chain Skills

- Always present next steps as suggestions only.
- Never automatically invoke `/review-code`, `/ct-unittest`, `/ct-bugfix-skill`, or any other skill.
- The user decides what to do next.

### 3. Checklist Items — Only Flag What Is Visible

- Only mark a checklist item ❌ if the violation is **visible in the diff** (added or modified lines).
- Do NOT flag pre-existing issues in unchanged lines unless they are directly adjacent to changed lines.
- Mark ➖ (not applicable) when there are no changed files in the relevant category.

### 4. Respect the --limit Flag

- Never output more files than the effective limit (default: 100, or user-specified `--limit <n>`).
- Always show the large-diff warning before truncating.
- Truncation applies to the file list, not to the analysis of visible files.

### 5. Architecture Layer Classification

- Use path keywords strictly (see PROMPT.md Step 4).
- If a file path is ambiguous (e.g., `Assembler/`), classify as Config / Other — do not guess.
- Do not reclassify files based on content, only on path.

---

## Common Pitfalls

### Pitfall: Empty diff
If `git diff --stat <TARGET>...HEAD` returns nothing:
- The current branch may already be up to date with the target.
- Report: `ℹ️ No changes found between <current> and <target>. The branch may be up to date.`
- Do not proceed to output layers.

### Pitfall: SHA not found
If `git cat-file -t <SHA>` returns an error:
- Report: `❌ Commit SHA <SHA> not found. Verify the SHA exists in the current repo.`
- Stop and ask the user for a valid target.

### Pitfall: Branch not found remotely
If the auto-detected branch does not exist locally or remotely:
- Report which branches were checked.
- Ask the user to specify a target explicitly.

### Pitfall: --focus returns no matches
If `grep -i "<keyword>"` returns no files:
- Report: `ℹ️ No files matching --focus "<keyword>" found in the diff.`
- Do not produce Layer 2/3/4 for an empty result set.

### Pitfall: Fabricating file:line references in checklist
- Only include `file:line` references if the line number is visible in the actual `git diff` output.
- If you can identify the file but not the exact line, use `filename` only — do not guess a line number.
