# Output Schema — ct-figma-storyboard

## Generated Files

### bottom_sheet / full_screen / modal / view_component

```
✅ UI generated.
• <Name>ViewController.swift → [MODULE_PATH]/ViewControllers/
• <Name>.storyboard         → [MODULE_PATH]/
• project.pbxproj           → 5 entries added
```

### tableview_onesection / tableview_multisection

```
✅ UI generated.
• <Name>ViewController.swift  → [MODULE_PATH]/ViewControllers/
• <Name>ViewModel.swift       → [MODULE_PATH]/ViewModels/
• <Name>.storyboard           → [MODULE_PATH]/
• Cell/<Name>Cell.swift       → [MODULE_PATH]/Cell/
• Cell/<Name>Cell.xib         → [MODULE_PATH]/Cell/   (only if standalone XIB)
• project.pbxproj             → 10+ entries added
```

---

## pbxproj Entry Summary

| COMPONENT_TYPE | Files | pbxproj entries |
|---|---|---|
| `bottom_sheet`, `full_screen`, `modal`, `view_component` | 2 | 5 |
| `tableview_onesection`, `tableview_multisection` (prototype cell) | 4 | 10 |
| `tableview_onesection`, `tableview_multisection` (standalone XIB cell) | 5 | 12 |
| +1 additional cell variant | +1 or +2 | +2 or +3 |

---

## Final Confirmation

Print this after Step 6 completes:

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ ct-figma-storyboard COMPLETE — <FEATURE_NAME>
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

UI: <Name>ViewController
  • <Name>ViewController.swift → [MODULE_PATH]/ViewControllers/
  • <Name>.storyboard → [MODULE_PATH]/
  • project.pbxproj — [N] entries added

Next steps:
  1. Run: swiftlint lint --config .swiftlint.yml --strict [changed files]
  2. Wire ViewModel callbacks in ViewController (onConfirm, onCancel, etc.)
  3. Register ViewController in the module's Assembler if not already wired
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```
