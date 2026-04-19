---
agent: agent
---

# ct-git-diff — Git Diff Analysis for Chợ Tốt iOS

Compare the **current branch** against a target branch or commit SHA and produce a structured, layered analysis tailored to the Chợ Tốt iOS codebase.

---

## Invocation Syntax

```
/ct-git-diff                                      # auto-detect base branch, summary mode
/ct-git-diff main                                 # explicit branch, summary mode
/ct-git-diff main --full                          # full diff content
/ct-git-diff abc1234                              # compare against commit SHA
/ct-git-diff main --path AppFeatures/CTPos        # narrow to a path
/ct-git-diff main --focus ViewModels              # narrow to an architecture layer/dir keyword
/ct-git-diff main --since 2024-01-01             # only commits after this date
/ct-git-diff main --limit 50                      # cap output to N files
```

Flags can be combined: `/ct-git-diff main --path AppFeatures/CTChat --focus ViewModels --limit 30`

### Flags Reference

| Flag | Default | Description |
|------|---------|-------------|
| `--full` | false | Show full `+/-` diff lines instead of summary stats |
| `--path <dir>` | none | Restrict diff to a subdirectory or file path |
| `--focus <keyword>` | none | Filter files matching keyword (e.g. `ViewModels`, `UseCase`, `Repository`, `Tests`) |
| `--since <date>` | none | Only include commits after this date (`YYYY-MM-DD`) |
| `--limit <n>` | 100 | Cap number of files shown in output |

---

## Step 1 — Resolve Target Branch

1. Check if a branch name or SHA was passed as an argument.
2. If provided, use it directly. If it is a SHA, validate it exists:
   ```bash
   git cat-file -t <SHA>   # must return "commit"
   ```
3. If omitted, auto-detect the base branch:
   ```bash
   for branch in main dev master; do
     git ls-remote --heads origin $branch | grep -q $branch && echo $branch && break
   done
   ```
4. If none found, stop and ask the user to specify a target explicitly.
5. Print a plain-language confirmation before proceeding:
   ```
   Comparing: <current-branch> → <target>   (<N> commits ahead)
   ```

---

## Step 2 — Run Git Commands

```bash
git rev-parse --abbrev-ref HEAD                          # current branch name
git rev-list --count <TARGET>...HEAD                     # commits ahead

# Summary mode (default)
git diff --stat <TARGET>...HEAD [-- <path>]

# Full mode (--full flag)
git diff <TARGET>...HEAD [-- <path>]

# With --focus: filter by keyword
git diff --name-only <TARGET>...HEAD | grep -i "<keyword>"

# With --since: filter by date
git log <TARGET>...HEAD --since="<date>" --name-only --pretty=format:"" | sort -u

# With --limit: cap file count
git diff --name-only <TARGET>...HEAD | head -<n>
```

Apply `--path` as the trailing `-- <path>` argument to every git command.

---

## Step 3 — Large Diff Guard

If the number of changed files exceeds `--limit` (default: 100):

1. Show warning:
   ```
   ⚠️ Large diff: X files changed. Showing first <limit>. Use --path or --limit to narrow scope.
   ```
2. List top changed directories:
   ```bash
   git diff --name-only <TARGET>...HEAD | awk -F'/' '{print $1"/"$2}' | sort | uniq -c | sort -rn | head -10
   ```
3. Truncate file list to limit and continue.

---

## Step 4 — Classify Files by Architecture Layer

| Layer | Path keywords |
|-------|--------------|
| Presentation | `ViewControllers`, `Views`, `ViewModels`, `Presentation` |
| Domain | `UseCases`, `Models`, `Domain` |
| Data | `Repositories`, `Services`, `Targets`, `Data` |
| Tests | `Tests`, `Spec`, `Mock` |
| Config / Other | anything else (Assembler, Resources, AGENTS.md, etc.) |

---

## Step 5 — Produce Layered Output

### Layer 1 — Diff Summary

```
📊 Diff Summary
─────────────────────────────────────────
Branch:  <current> ← <target>
Commits: N commits ahead
Files:   X changed (+Y insertions, -Z deletions)
```

Followed by file table grouped by layer, each file listed with change type:
`M` modified | `A` added | `D` deleted | `R` renamed

---

### Layer 2 — Structured Analysis

**Presentation Layer**
- Flag ViewControllers doing business logic directly (should delegate to ViewModel)
- Flag missing `disposed(by: disposeBag)` in subscription chains
- Flag raw UIKit: `UILabel`, `UIButton`, `UITextField`, `UIImageView`, `UIStackView` — use DS equivalents
- Flag `NSLayoutConstraint` or Interface Builder (only SnapKit allowed)

**Domain Layer**
- Flag UseCases with direct network/database calls (must go through Repository)
- Flag mutable models (prefer immutable structs)

**Data Layer**
- Flag Services calling APIs without going through a Target
- Flag observable chains missing `observe(on: MainScheduler.instance)` before UI-bound values

**Tests Layer**
- Note which changed non-test files have corresponding test changes
- Flag changed files with no corresponding test change as a coverage gap

---

### Layer 3 — Review Checklist

```
CT iOS Review Checklist
─────────────────────────────────────────
Architecture
  [ ] MVVM layers respected
  [ ] UseCases used for business logic
  [ ] Repositories abstract data access

CT Design System
  [ ] DSLabel / DSButton / DSTextField / DSImageView used
  [ ] DS.TypoToken.* / theme.* used (no UIFont / UIColor)
  [ ] SnapKit constraints only

RxSwift
  [ ] DisposeBag declared and used
  [ ] [weak self] in all closures
  [ ] observe(on: MainScheduler.instance) before UI updates
  [ ] No nested subscriptions

Code Quality
  [ ] No force unwrap (!)
  [ ] Logger.print() used (not print())
  [ ] No hardcoded strings — use CTLocalize pattern
  [ ] DRY principle followed
```

Mark each item: ✅ Pass | ❌ Fail (with `file:line`) | ➖ Not applicable

Only mark ❌ if the violation is **visible in the diff** (added or modified lines). Never flag pre-existing issues in unchanged lines.

---

### Layer 4 — Narrative Summary

3–8 sentences covering:
1. What changed (high-level feature/fix description)
2. Key architectural decisions visible in the diff
3. Risk areas that could cause regressions
4. Missing pieces (no tests, no error handling, etc.)

Suitable for use as a **PR description draft**.

---

## Step 6 — Final Confirmation

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ ct-git-diff COMPLETE — <current branch>
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Compared: <current> ← <target>
Files:    X changed | +Y -Z

💡 Suggested Next Steps:
  1. /review-code <highest-risk file>        (if CT DS / architecture violations found)
  2. /ct-unittest <untested ViewModel>       (if test gaps found)
  3. /ct-bugfix-skill                        (if force unwrap / weak self / threading issues)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

Only include next steps relevant to findings. **Never auto-invoke them.**

---

## Guardrails

- **Never hallucinate**: Only report findings from actual git command output. Never invent file paths, line numbers, or code snippets.
- **Never auto-chain**: Present next steps as suggestions only. The user decides what runs next.
- **Empty diff**: If `git diff --stat` returns nothing → `ℹ️ No changes found between <current> and <target>.` Stop.
- **SHA not found**: If `git cat-file -t <SHA>` errors → `❌ Commit SHA <SHA> not found.` Stop.
- **--focus no matches**: If grep returns no files → `ℹ️ No files matching --focus "<keyword>" found.` Skip layers 2–4.
- **Layer classification**: Use path keywords only. Ambiguous paths (e.g. `Assembler/`) → Config / Other.
- **file:line references**: Only include if the line number is visible in the actual diff. Use `filename` only if line is unknown.
