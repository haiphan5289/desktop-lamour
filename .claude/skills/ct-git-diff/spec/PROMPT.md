# Prompt — ct-git-diff

> See [GUARDRAILS.md](GUARDRAILS.md) before executing any step.
> Input parameters are defined in [INPUT_SCHEMA.md](INPUT_SCHEMA.md).
> Output format is defined in [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md).

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
5. Confirm the resolved target to the user before proceeding.

---

## Step 2 — Run Git Commands

Run the appropriate git commands based on flags:

```bash
# Current branch name
git rev-parse --abbrev-ref HEAD

# Commit count ahead
git rev-list --count <TARGET>...HEAD

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
Combine flags as needed (e.g. `--path` + `--focus` + `--limit` all at once).

---

## Step 3 — Large Diff Guard

Check if the number of changed files exceeds the `--limit` (default: 100).

If exceeded:
1. Show warning:
   ```
   ⚠️ Large diff: X files changed. Showing first <limit>. Use --path or --limit to narrow scope.
   ```
2. List top changed directories so the user can pick a `--path`:
   ```bash
   git diff --name-only <TARGET>...HEAD | awk -F'/' '{print $1"/"$2}' | sort | uniq -c | sort -rn | head -10
   ```
3. Truncate the file list to the limit and continue.

---

## Step 4 — Classify Files by Architecture Layer

For each changed file, map it to one of these layers based on path keywords:

| Layer | Path keywords |
|-------|--------------|
| Presentation | `ViewControllers`, `Views`, `ViewModels`, `Presentation` |
| Domain | `UseCases`, `Models`, `Domain` |
| Data | `Repositories`, `Services`, `Targets`, `Data` |
| Tests | `Tests`, `Spec`, `Mock` |
| Config / Other | anything else (Assembler, Resources, AGENTS.md, etc.) |

Build the Layer 1 summary table from this classification.

---

## Step 5 — Produce Layered Output

Generate all four layers in order. See [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md) for the exact format.

### Layer 2 rules by architecture layer:

**Presentation Layer**
- Flag ViewControllers doing business logic directly (should delegate to ViewModel)
- Flag missing `disposed(by: disposeBag)` in subscription chains
- Flag raw UIKit usage: `UILabel`, `UIButton`, `UITextField`, `UIImageView`, `UIStackView` — should use DSLabel, DSButton, DSTextField, DSImageView, DSStackView
- Flag `NSLayoutConstraint` or Interface Builder usage (only SnapKit allowed)

**Domain Layer**
- Flag UseCases with direct network or database calls (must go through Repository)
- Flag mutable models (prefer immutable structs)

**Data Layer**
- Flag Services calling APIs without going through a Target
- Flag observable chains missing `observe(on: MainScheduler.instance)` before UI-bound values

**Tests Layer**
- Note which changed non-test files have corresponding test changes
- Flag changed files with **no corresponding test change** as a coverage gap

### Layer 3 checklist:

Apply the CT iOS review checklist from [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md) against the diff. Mark each item ✅ / ❌ / ➖.

For ❌ failures: include `filename:line` reference.

### Layer 4 narrative:

Write 3–8 sentences. Cover: what changed, key decisions, risk areas, missing pieces.

---

## Step 6 — Print Final Confirmation

Print the completion block from [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md).

Only include next-step suggestions that are directly relevant to findings (e.g. only suggest `/ct-unittest` if test gaps were found).
