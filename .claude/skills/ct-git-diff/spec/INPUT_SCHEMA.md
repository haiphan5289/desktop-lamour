# Input Schema — ct-git-diff

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

---

## Positional Argument

| Argument | Required | Description |
|----------|----------|-------------|
| `branch\|SHA` | No | Target branch name or commit SHA to compare against. If omitted, auto-detected in order: `main` → `dev` → `master`. |

---

## Flags Reference

| Flag | Type | Default | Description |
|------|------|---------|-------------|
| `--full` | boolean | false | Show full `+/-` diff lines instead of summary stats |
| `--path <dir>` | string | none | Restrict diff to a subdirectory or file path |
| `--focus <keyword>` | string | none | Filter files matching keyword (e.g. `ViewModels`, `UseCase`, `Repository`, `Tests`) |
| `--since <date>` | string | none | Only include commits after this date (`YYYY-MM-DD`) |
| `--limit <n>` | integer | 100 | Cap number of files shown in output |

---

## Auto-Detect Base Branch Logic

If no branch/SHA is provided, resolve in this priority order:

```bash
for branch in main dev master; do
  if git ls-remote --heads origin $branch | grep -q $branch; then
    BASE=$branch
    break
  fi
done
echo "Auto-detected base: $BASE"
```

If none of the three branches exist remotely, stop and ask the user to specify a branch explicitly.

Always confirm the resolved target to the user before proceeding with the diff.
