# Input Schema — ct-figma-storyboard

## Input Format

The user must provide:

```
FIGMA_URL: <Figma node URL, dev mode preferred>
MODULE_PATH: <Target folder path, e.g. ChoTot/Features/Job/VerticalizePos/Presentation/Ver2/Pos>
COMPONENT_TYPE: <bottom_sheet | full_screen | modal | view_component | tableview_onesection | tableview_multisection>
```

---

## Parameter Reference

| Parameter | Required | Description |
|-----------|----------|-------------|
| `FIGMA_URL` | Yes | Figma design URL (dev mode preferred). Used to fetch design context and screenshot via Figma MCP. |
| `MODULE_PATH` | Yes | Target folder path (relative to project root) where generated files will be placed. |
| `COMPONENT_TYPE` | Yes | UI component type — determines storyboard template, reference files, and pbxproj entry count. |

---

## COMPONENT_TYPE Values

| Value | Description | Reference | pbxproj entries |
|-------|-------------|-----------|-----------------|
| `bottom_sheet` | Modal bottom sheet with drawer header + body + footer | `CRNoticeShareAd.storyboard` + `CRNoticeShareAdViewController.swift` | 5 (2 files) |
| `full_screen` | Full-screen ViewController | Search sibling storyboard in same module folder | 5 (2 files) |
| `modal` | Modal presentation ViewController | Search sibling storyboard in same module folder | 5 (2 files) |
| `view_component` | Reusable UIView subclass with storyboard | Search sibling storyboard in same module folder | 5 (2 files) |
| `tableview_onesection` | TableView with a single section | `CRAdOptimizationAll.storyboard` + `CRAdOptimizationAll.swift` | 10+ (4+ files) |
| `tableview_multisection` | TableView with multiple sections (RxDataSources) | `PTSubscriptionSK.storyboard` + `PTSubscriptionSKViewController.swift` | 10+ (4+ files) |
