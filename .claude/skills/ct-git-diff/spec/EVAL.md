# Eval — ct-git-diff

Use this checklist to verify output quality before finishing.

---

## Completion Checklist

- [ ] Target branch/SHA was confirmed to the user before running the diff
- [ ] All git commands were actually executed (not simulated)
- [ ] Large-diff warning shown if file count exceeded the limit
- [ ] Files correctly classified into Presentation / Domain / Data / Tests / Config layers
- [ ] Layer 1 summary table is present with correct counts and change types (M/A/D/R)
- [ ] Layer 2 analysis covers only layers that have actual changes
- [ ] Checklist items marked ❌ include a `file:line` or `filename` reference from the actual diff
- [ ] Checklist items marked ➖ are correct (no relevant file changes in that category)
- [ ] Layer 4 narrative is 3–8 sentences and covers: what changed, decisions, risks, gaps
- [ ] Final confirmation block printed with accurate file count and insertions/deletions
- [ ] Next steps are relevant to findings (no generic suggestions)
- [ ] No skills were auto-invoked — only suggested
- [ ] No fabricated file paths, line numbers, or code snippets in output
- [ ] `--path`, `--focus`, `--since`, `--limit` flags were correctly applied if provided
