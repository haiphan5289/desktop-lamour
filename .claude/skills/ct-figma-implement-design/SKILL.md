---
name: ct-figma-implement-design
description: Translate Figma designs into production-ready iOS code for Cho Tot with 1:1 visual fidelity and strict design system compliance. Use THIS SKILL whenever implementing UI from Figma files, mapping Figma colors/typography to CTDesignSystem tokens, creating SnapKit-only layouts (never XIB or NSLayoutConstraint), building custom components, or styling buttons/inputs/cards from Figma. This skill ensures proper CTDesignSystem token mapping (colors, typography, spacing), SnapKit layout implementation, MVVM-C architecture integration, and component reuse from CTAdView, CTInsertAd, CTJOB, CTVEH. Requires Figma MCP server connection. Use for ANY Figma design implementation—even small components, icons, or styling adjustments.
metadata:
  mcp-server: figma
---

# Figma-to-iOS Implementation

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

## Overview

This skill provides a structured workflow for translating Figma designs into production-ready Cho Tot iOS code with pixel-perfect accuracy. It ensures:
- Integration with the Figma MCP server
- Proper mapping of Figma colors/typography to CTDesignSystem tokens
- SnapKit-only layout (never NSLayoutConstraint or Interface Builder)
- 1:1 visual parity with designs
- MVVM-C architecture alignment

## Prerequisites

- Figma MCP server must be connected and accessible
  - Before proceeding, verify the Figma MCP server is connected by checking if Figma MCP tools (e.g., `get_design_context`) are available.
  - If the tools are not available, the Figma MCP server may not be enabled. Guide the user to enable the Figma MCP server that is included with the plugin. They may need to restart their MCP client afterward.
- User must provide a Figma URL in the format: `https://figma.com/design/:fileKey/:fileName?node-id=1-2`
  - `:fileKey` is the file key
  - `1-2` is the node ID (the specific component or frame to implement)
- Project should have an established design system or component library (preferred)

## Required Workflow

**Follow these steps in order. Do not skip steps.**

### Step 1: Get Node ID

#### Option A: Parse from Figma URL

When the user provides a Figma URL, extract the file key and node ID to pass as arguments to MCP tools.

**URL format:** `https://figma.com/design/:fileKey/:fileName?node-id=1-2`

**Extract:**

- **File key:** `:fileKey` (the segment after `/design/`)
- **Node ID:** `1-2` (the value of the `node-id` query parameter)

**Example:**

- URL: `https://figma.com/design/kL9xQn2VwM8pYrTb4ZcHjF/DesignSystem?node-id=42-15`
- File key: `kL9xQn2VwM8pYrTb4ZcHjF`
- Node ID: `42-15`

### Step 2: Fetch Design Context

Run `get_design_context` with the extracted file key and node ID.

```
get_design_context(fileKey=":fileKey", nodeId="1-2")
```

This provides the structured data including:

- Layout properties (Auto Layout, constraints, sizing)
- Typography specifications
- Color values and design tokens
- Component structure and variants
- Spacing and padding values

**If the response is too large or truncated:**

1. Run `get_metadata(fileKey=":fileKey", nodeId="1-2")` to get the high-level node map
2. Identify the specific child nodes needed from the metadata
3. Fetch individual child nodes with `get_design_context(fileKey=":fileKey", nodeId=":childNodeId")`

### Step 3: Capture Visual Reference

Run `get_screenshot` with the same file key and node ID for a visual reference.

```
get_screenshot(fileKey=":fileKey", nodeId="1-2")
```

This screenshot serves as the source of truth for visual validation. Keep it accessible throughout implementation.

### Step 4: Download Required Assets

Download any assets (images, icons, SVGs) returned by the Figma MCP server.

**IMPORTANT:** Follow these asset rules:

- If the Figma MCP server returns a `localhost` source for an image or SVG, use that source directly
- DO NOT import or add new icon packages - all assets should come from the Figma payload
- DO NOT use or create placeholders if a `localhost` source is provided
- Assets are served through the Figma MCP server's built-in assets endpoint

### Step 5: Map Figma Tokens to CTDesignSystem

Map all Figma colors, typography, and spacing to Cho Tot's CTDesignSystem tokens.

**Color Mapping:**
- Extract RGB values from Figma colors
- Match to CTTheme colors in `Libraries/CTDesignSystem/CTTheme.swift`
- Use theme properties: `theme.text.textPrimary`, `theme.background.bgSecondary`, `theme.border.borderDefault`, etc.
- Never use hardcoded UIColor or hex values
- Always use `setStyle(DS.TypoToken...)` for typography with theme colors

**Typography Mapping:**
- Map Figma font size/weight/line-height to CTDesignSystem TypoToken
- Examples: `DS.TypoToken.Header.Section`, `DS.TypoToken.Label.Caption`, `DS.TypoToken.Body.Paragraph`
- Use consistent scaling (follow CTDesignSystem's scale, not Figma's exact values if they conflict)

**Spacing & Sizing:**
- Use CTDesignSystem spacing tokens (8px, 12px, 16px, 20px, 24px grid)
- All SnapKit constraints use these tokens as `offset()` and `inset()` values
- Avoid hardcoded spacing values

**Layout & Components:**
- **ALWAYS use CTDesignSystem components** (DSLabel, DSButton, DSTextField, DSImageView, DSStackView)
- Never use raw UIKit (UILabel, UIButton, UITextField, UIImageView, UIStackView)
- Reuse existing components (buttons, inputs, cards) from CTAdView, CTInsertAd, CTJOB, CTVEH
- Check `Libraries/CTDesignSystem/CTDesignSystemExampleApp/SampleApp` for component usage examples

**Layout System:**
- Use **SnapKit ONLY** for all Auto Layout constraints
- Never use NSLayoutConstraint or Interface Builder (XIB/Storyboard)
- Never use frame-based or manual layout
- All layout code: `label.snp.makeConstraints { make in ... }`

**Architecture:**
- Place components in appropriate MVVM-C layer (Presentation/ViewControllers, Presentation/Views)
- Respect ViewController → ViewModel → UseCase data flow
- Use BehaviorRelay for state, PublishRelay for events
- Integrate with existing presenter protocol and listener pattern

### Step 6: Achieve 1:1 Visual Parity

Strive for pixel-perfect visual parity with the Figma design.

**Guidelines:**

- Prioritize Figma fidelity to match designs exactly
- Avoid hardcoded values - use design tokens from Figma where available
- When conflicts arise between design system tokens and Figma specs, prefer design system tokens but adjust spacing or sizes minimally to match visuals
- Follow WCAG requirements for accessibility
- Add component documentation as needed

### Step 7: Validate Against Figma

Before marking complete, validate the final UI against the Figma screenshot.

**Validation checklist:**

- [ ] Layout matches (spacing, alignment, sizing)
- [ ] Typography matches (font, size, weight, line height)
- [ ] Colors match exactly
- [ ] Interactive states work as designed (hover, active, disabled)
- [ ] Responsive behavior follows Figma constraints
- [ ] Assets render correctly
- [ ] Accessibility standards met

## Implementation Rules for Cho Tot iOS

### Component Organization

- Place custom UI components in the module's `Presentation/Views/` directory
- Follow Cho Tot naming: `[Feature]CustomView.swift` (e.g., `CTInsertAdListItemView.swift`)
- Register new components in module's Assembler if needed
- If creating reusable components used across modules, place in `Libraries/CTComponent/`

### CTDesignSystem Integration (MANDATORY)

**Component Usage:**
- `DSLabel` instead of `UILabel`
- `DSButton` instead of `UIButton`
- `DSTextField` instead of `UITextField`
- `DSImageView` instead of `UIImageView`
- `DSStackView` instead of `UIStackView`
- `DSScrollView` instead of `UIScrollView`

**Styling Pattern:**
```swift
let label = DSLabel()
label.setStyle(DS.TypoToken.Label.Caption(color: theme.text.textPrimary.color))
// Never: label.textColor = UIColor(hex: 0xFF5733)
```

**Layout Pattern (SnapKit ONLY):**
```swift
label.snp.makeConstraints { make in
    make.top.equalTo(containerView.snp.top).offset(16)
    make.leading.trailing.equalTo(containerView).inset(20)
}
// Never: NSLayoutConstraint(...).isActive = true
// Never: Interface Builder / XIB
```

### Code Quality

- Avoid hardcoded values - extract to DS tokens or constants
- Keep views composable and reusable
- Add proper MARK sections (`// MARK: - Properties`, `// MARK: - Setup`)
- Include brief comments for complex layout logic
- Use weak references in closures to prevent retain cycles
- Run SwiftLint: `swiftlint lint --config .swiftlint.yml`

### Reference Examples

Check these existing Cho Tot components for patterns:
- **Button styles:** `Libraries/CTDesignSystem/CTDesignSystemExampleApp/SampleApp`
- **Card layouts:** CTAdView module
- **List cells:** CTInsertAd, CTJOB modules
- **Form inputs:** CTAuthentication module

## Examples

### Example 1: Implementing a Button Component

User says: "Implement this Figma button from the Cho Tot design system: https://figma.com/design/kL9xQn2VwM8pYrTb4ZcHjF/ChoTotDesignSystem?node-id=42-15"

**Actions:**

1. Parse URL to extract fileKey=`kL9xQn2VwM8pYrTb4ZcHjF` and nodeId=`42-15`
2. Run `get_design_context(fileKey="kL9xQn2VwM8pYrTb4ZcHjF", nodeId="42-15")`
3. Run `get_screenshot(fileKey="kL9xQn2VwM8pYrTb4ZcHjF", nodeId="42-15")` for visual reference
4. Extract Figma: background color (e.g., #007AFF), text color (e.g., #FFFFFF), padding (e.g., 12px), corner radius (e.g., 8px)
5. Map to CTDesignSystem: `theme.primary.primaryDefault.color`, `DS.TypoToken.Label.Medium`, etc.
6. Check existing CTDesignSystem buttons (DSButton, DSButton2 variants)
7. If new variant needed, extend DSButton in module's custom view
8. Create buttonView with: `DSButton()` + `setStyle(DS.TypoToken....)` + SnapKit constraints
9. Validate against screenshot: padding, corner radius, font, colors

**Result:** iOS button component using DSButton and CTDesignSystem, integrated with SnapKit layout.

### Example 2: Building an Ad Listing Screen

User says: "Build this CTInsertAd listing screen from Figma: https://figma.com/design/pR8mNv5KqXzGwY2JtCfL4D/CTInsertAd?node-id=10-5"

**Actions:**

1. Parse URL to extract fileKey and nodeId
2. Run `get_metadata(fileKey, nodeId)` to understand structure
3. Identify main sections: header, list cells, empty state, footer and their node IDs
4. Run `get_design_context()` for each section to extract styling details
5. Run `get_screenshot(fileKey, nodeId)` for full screen visual reference
6. Download all icons/images from assets endpoint
7. Create ViewController + Views following MVVM-C:
   - Extract header section → HeaderView (DSLabel + SnapKit)
   - Extract list cell → AdListItemView (DSLabel, DSImageView, DSButton, SnapKit)
   - Extract empty state → EmptyStateView
8. Map all colors/typography to CTDesignSystem tokens
9. Use SnapKit for ALL layout constraints (never NSLayoutConstraint)
10. Integrate with ViewController → ViewModel → Repository data flow
11. Validate against screenshot: spacing, colors, typography, cell heights

**Result:** Complete ad listing screen following Cho Tot MVVM-C architecture, CTDesignSystem, and SnapKit layout patterns.

## Best Practices

### Always Start with Context

Never implement based on assumptions. Always fetch `get_design_context` and `get_screenshot` first.

### Incremental Validation

Validate frequently during implementation, not just at the end. This catches issues early.

### Document Deviations

If you must deviate from the Figma design (e.g., for accessibility or technical constraints), document why in code comments.

### Reuse Over Recreation

Always check for existing components before creating new ones. Consistency across the codebase is more important than exact Figma replication.

### Design System First

When in doubt, prefer the project's design system patterns over literal Figma translation.

## Common Issues and Solutions

### Issue: Figma output is truncated

**Cause:** The design is too complex or has too many nested layers to return in a single response.
**Solution:** Use `get_metadata` to get the node structure, then fetch specific nodes individually with `get_design_context`.

### Issue: Design doesn't match after implementation

**Cause:** Visual discrepancies between the implemented code and the original Figma design.
**Solution:** Compare side-by-side with the screenshot from Step 3. Check spacing, colors, and typography values in the design context data.

### Issue: Assets not loading

**Cause:** The Figma MCP server's assets endpoint is not accessible or the URLs are being modified.
**Solution:** Verify the Figma MCP server's assets endpoint is accessible. The server serves assets at `localhost` URLs. Use these directly without modification.

### Issue: Design token values differ from Figma

**Cause:** The project's design system tokens have different values than those specified in the Figma design.
**Solution:** When project tokens differ from Figma values, prefer project tokens for consistency but adjust spacing/sizing to maintain visual fidelity.

## Understanding Design Implementation

The Figma implementation workflow establishes a reliable process for translating designs to code:

**For designers:** Confidence that implementations will match their designs with pixel-perfect accuracy.
**For developers:** A structured approach that eliminates guesswork and reduces back-and-forth revisions.
**For teams:** Consistent, high-quality implementations that maintain design system integrity.

By following this workflow, you ensure that every Figma design is implemented with the same level of care and attention to detail.

## Additional Resources

- [Figma MCP Server Documentation](https://developers.figma.com/docs/figma-mcp-server/)
- [Figma MCP Server Tools and Prompts](https://developers.figma.com/docs/figma-mcp-server/tools-and-prompts/)
- [Figma Variables and Design Tokens](https://help.figma.com/hc/en-us/articles/15339657135383-Guide-to-variables-in-Figma)
