# Changelog — ct-git-diff

## v1.0.0 — 2026-04-19

- Initial release
- Auto-detect base branch: `main` → `dev` → `master`
- Supports explicit branch name and commit SHA as target
- Flags: `--full`, `--path`, `--focus`, `--since`, `--limit`
- Layered output: Diff Summary → Structured Analysis → CT iOS Review Checklist → Narrative
- Large diff guard: warns and truncates at 100 files by default
- Suggests next steps without auto-chaining
- Restructured to match `ct-figma-storyboard` spec/ directory pattern
