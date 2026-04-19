# Output Schema — ct-git-diff

## Layered Output Structure

Always produce output in this exact order. Never skip a layer.

---

### Layer 1 — Diff Summary

```
📊 Diff Summary
─────────────────────────────────────────
Branch:  <current> ← <target>
Commits: N commits ahead
Files:   X changed (+Y insertions, -Z deletions)
```

Followed by a file table grouped by architecture layer:

| Layer | Files Changed |
|-------|--------------|
| Presentation (ViewControllers / Views / ViewModels) | N |
| Domain (UseCases / Models) | N |
| Data (Repositories / Services / Targets) | N |
| Tests | N |
| Config / Other | N |

List each changed file under its layer heading with change type:
- `M` — modified
- `A` — added
- `D` — deleted
- `R` — renamed

---

### Layer 2 — Structured Analysis

For each architecture layer that has changes, provide findings. See [PROMPT.md](PROMPT.md) Step 3 for full per-layer rules.

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

---

### Layer 4 — Narrative Summary

3–8 sentences covering:
1. What changed (high-level feature/fix description)
2. Key architectural decisions visible in the diff
3. Risk areas that could cause regressions
4. Missing pieces (no tests, no error handling, etc.)

Suitable for use as a **PR description draft**.

---

## Final Confirmation

Print this block after all layers complete:

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ ct-git-diff COMPLETE — <current branch>
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Compared: <current> ← <target>
Files:    X changed | +Y -Z

💡 Suggested Next Steps:
  1. /review-code <highest-risk file>
  2. /ct-unittest <untested ViewModel>
  3. /ct-bugfix-skill — if checklist flagged failures
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

Only suggest steps relevant to findings. Never auto-invoke them.
