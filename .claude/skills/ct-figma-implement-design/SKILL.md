---
name: ct-figma-implement-design
description: Translate Figma designs into production-ready WPF XAML code with 1:1 visual fidelity and strict design system compliance. Use THIS SKILL whenever implementing UI from Figma files, mapping Figma colors/typography to AppDesignSystem tokens, creating XAML-only layouts (Grid, StackPanel, DockPanel — never manual code-behind positioning), building custom UserControls, or styling buttons/inputs/cards from Figma. This skill ensures proper AppDesignSystem token mapping (colors, typography, spacing), XAML layout implementation, MVVM architecture integration, and component reuse from Features/ProductList, Features/InsertAd, Features/Jobs, Features/Vehicles. Requires Figma MCP server connection. Use for ANY Figma design implementation—even small components, icons, or styling adjustments.
metadata:
  mcp-server: figma
---

# Figma-to-WPF Implementation

## Overview

This skill provides a structured workflow for translating Figma designs into production-ready WPF XAML code with pixel-perfect accuracy. It ensures:
- Integration with the Figma MCP server
- Proper mapping of Figma colors/typography to AppDesignSystem ResourceDictionary tokens
- XAML-only layout (Grid, StackPanel, DockPanel, Border — never code-behind manual positioning)
- 1:1 visual parity with designs
- MVVM architecture alignment

## Prerequisites

- Figma MCP server must be connected and accessible
  - Before proceeding, verify the Figma MCP server is connected by checking if Figma MCP tools (e.g., `get_design_context`) are available.
  - If the tools are not available, the Figma MCP server may not be enabled. Restart VS Code MCP connection.
- User must provide a Figma URL in the format: `https://figma.com/design/:fileKey/:fileName?node-id=1-2`
  - `:fileKey` is the file key
  - `1-2` is the node ID (the specific component or frame to implement)
- Project should have an established AppDesignSystem ResourceDictionary (preferred)

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

### Step 5: Map Figma Tokens to AppDesignSystem

Map all Figma colors, typography, and spacing to the project's AppDesignSystem ResourceDictionary tokens.

**Color Mapping:**
- Extract RGB values from Figma colors
- Match to AppTheme colors in `Themes/AppColors.xaml` ResourceDictionary
- Use named brushes: `{StaticResource TextPrimaryBrush}`, `{StaticResource BackgroundSecondaryBrush}`, `{StaticResource BorderDefaultBrush}`, etc.
- Never use hardcoded `#RRGGBB` hex values directly in XAML
- Always use `Style="{StaticResource ...}"` for typography

**Typography Mapping:**
- Map Figma font size/weight/line-height to AppDesignSystem text styles
- Examples: `{StaticResource HeaderSectionStyle}`, `{StaticResource LabelCaptionStyle}`, `{StaticResource BodyParagraphStyle}`
- Use consistent scaling (follow AppDesignSystem's scale, not Figma's exact values if they conflict)

**Spacing & Sizing:**
- Use AppDesignSystem spacing tokens (8, 12, 16, 20, 24 dp grid)
- All XAML `Margin` and `Padding` use these tokens as `StaticResource` values
- Avoid hardcoded spacing values

**Layout & Components:**
- **ALWAYS use AppDesignSystem components** (AppLabel, AppButton, AppTextField, AppImage, AppCard)
- Never use raw WPF controls with inline styling (TextBlock with raw colors, etc.)
- Reuse existing components from Features/ProductList, Features/InsertAd, Features/Jobs, Features/Vehicles
- Check `AppDesignSystem/Samples/` for component usage examples

**Layout System:**
- Use **XAML layout panels ONLY**: Grid, StackPanel, DockPanel, WrapPanel, Border
- Never use Canvas except for custom drawing
- Never set `Width`/`Height` explicitly unless required by design—prefer `*` sizing in Grid rows/columns
- All layout adapts via `HorizontalAlignment`, `VerticalAlignment`, `Margin`, `Padding`

**Architecture:**
- Place UserControls in appropriate MVVM layer (`Features/[Feature]/Views/`)
- Respect View → ViewModel → UseCase data flow
- Use `[ObservableProperty]` for state, `[RelayCommand]` for events
- Integrate with existing `IViewModel` pattern

### Step 6: Achieve 1:1 Visual Parity

Strive for pixel-perfect visual parity with the Figma design.

**Guidelines:**

- Prioritize Figma fidelity to match designs exactly
- Avoid hardcoded values — use design tokens from AppDesignSystem where available
- When conflicts arise between design system tokens and Figma specs, prefer design system tokens but adjust spacing or sizes minimally to match visuals
- Follow WCAG requirements for accessibility
- Add AutomationProperties for screen reader support

### Step 7: Validate Against Figma

Before marking complete, validate the final UI against the Figma screenshot.

**Validation checklist:**

- [ ] Layout matches (spacing, alignment, sizing)
- [ ] Typography matches (font, size, weight, line height)
- [ ] Colors match exactly
- [ ] Interactive states work as designed (hover, active, disabled, focused)
- [ ] Responsive behavior follows Figma constraints
- [ ] Assets render correctly
- [ ] Accessibility standards met (AutomationProperties.Name, keyboard navigation)

## Implementation Rules for WPF

### Component Organization

- Place custom UserControls in `Features/[Feature]/Views/` directory
- Follow naming convention: `[Feature]ItemView.xaml` (e.g., `ProductListItemView.xaml`)
- Register new singleton dependencies in `DI/ServiceCollectionExtensions.cs` if needed
- If creating reusable components used across features, place in `Shared/Controls/`

### AppDesignSystem Integration (MANDATORY)

**Component Usage:**
- `AppLabel` (or `Style="{StaticResource LabelStyle}"`) instead of raw `TextBlock`
- `AppButton` (or `Style="{StaticResource PrimaryButtonStyle}"`) instead of raw `Button`
- `AppTextField` (or `Style="{StaticResource TextFieldStyle}"`) instead of raw `TextBox`
- `AppImage` instead of raw `Image` with inline styling
- `AppCard` (or `Style="{StaticResource CardStyle}"`) for card containers

**Styling Pattern:**
```xml
<TextBlock Style="{StaticResource LabelCaptionStyle}"
           Foreground="{StaticResource TextPrimaryBrush}"
           Text="{Binding Title}" />
<!-- Never: <TextBlock Foreground="#FF5733" FontSize="12" FontWeight="Bold" ...> -->
```

**Layout Pattern (XAML Grid ONLY):**
```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>
    <TextBlock Grid.Row="0" Margin="16,16,16,8" Style="{StaticResource HeaderSectionStyle}" Text="{Binding Title}"/>
    <ContentControl Grid.Row="1" Margin="16,0,16,16" Content="{Binding Body}"/>
</Grid>
<!-- Never: Canvas.Left/Top, manual Width/Height positioning -->
```

### Code Quality

- Avoid hardcoded values — extract to AppDesignSystem tokens or app constants
- Keep UserControls composable and reusable
- Add proper `x:Name` only when needed for code-behind; prefer data binding
- Include brief comments for complex layout logic
- Use `{x:Bind}` compiled bindings over `{Binding}` where possible for performance
- Run Roslyn/StyleCop analyzers on code-behind files

### Reference Examples

Check these existing components for patterns:
- **Button styles:** `AppDesignSystem/Themes/ButtonStyles.xaml`
- **Card layouts:** `Features/ProductList/Views/`
- **List items:** `Features/InsertAd/Views/`, `Features/Jobs/Views/`
- **Form inputs:** `Features/Authentication/Views/`

## Examples

### Example 1: Implementing a Button Component

User says: "Implement this Figma button: https://figma.com/design/kL9xQn2VwM8pYrTb4ZcHjF/DesignSystem?node-id=42-15"

**Actions:**

1. Parse URL → fileKey=`kL9xQn2VwM8pYrTb4ZcHjF`, nodeId=`42-15`
2. Run `get_design_context(fileKey, nodeId)`
3. Run `get_screenshot(fileKey, nodeId)` for visual reference
4. Extract Figma: background color (#007AFF), text color (#FFFFFF), padding (12dp), corner radius (8dp)
5. Map to AppDesignSystem: `{StaticResource PrimaryBrush}`, `{StaticResource PrimaryButtonStyle}`, `CornerRadius="8"`
6. Check existing AppButton variants in `AppDesignSystem/Themes/ButtonStyles.xaml`
7. If new variant needed, add a `<Style>` in the feature's ResourceDictionary
8. Create XAML: `<Button Style="{StaticResource PrimaryButtonStyle}" Content="Submit" Command="{Binding SubmitCommand}"/>`
9. Validate against screenshot: padding, corner radius, font, colors

### Example 2: Building a Product Listing Screen

User says: "Build this product listing screen from Figma: https://figma.com/design/pR8mNv5KqXzGwY2JtCfL4D/Features?node-id=10-5"

**Actions:**

1. Parse URL → fileKey and nodeId
2. Run `get_metadata(fileKey, nodeId)` to understand structure
3. Identify main sections: header, list items, empty state, footer — note their node IDs
4. Run `get_design_context()` for each section
5. Run `get_screenshot(fileKey, nodeId)` for full screen visual reference
6. Download icons/images from assets endpoint
7. Create View + ViewModel following MVVM:
   - Extract header section → `Grid` with `AppLabel` + search bar
   - Extract list cell → `ProductListItemView.xaml` (`AppImage`, `AppLabel`, `AppButton`)
   - Extract empty state → `EmptyStateView.xaml`
8. Map colors/typography to AppDesignSystem tokens
9. Use `ItemsControl` or `ListView` with `DataTemplate` for the list
10. Integrate via `{Binding Products}` ObservableCollection → ViewModel → UseCase
11. Validate against screenshot: spacing, colors, typography, item layout

## Best Practices

### Always Start with Context

Never implement based on assumptions. Always fetch `get_design_context` and `get_screenshot` first.

### Incremental Validation

Validate frequently during implementation. Compare XAML previews against Figma screenshot.

### Document Deviations

If you must deviate from the Figma design (e.g., for accessibility or WPF constraints), document why in XAML comments.

### Reuse Over Recreation

Check for existing AppDesignSystem styles and controls before creating new ones. Consistency is more important than exact Figma replication.

### Design System First

When in doubt, prefer existing AppDesignSystem ResourceDictionary patterns over literal Figma translation.

## Common Issues and Solutions

### Issue: Figma output is truncated

**Solution:** Use `get_metadata` to get the node structure, then fetch specific nodes individually.

### Issue: Design doesn't match after implementation

**Solution:** Compare side-by-side with the Figma screenshot. Check Grid row/column definitions, Margin/Padding values, and `StaticResource` brush names.

### Issue: Assets not loading

**Solution:** Verify the Figma MCP server's assets endpoint is accessible. Use `localhost` URLs directly without modification.

### Issue: Design token values differ from Figma

**Solution:** Prefer AppDesignSystem tokens for consistency; adjust spacing/sizing minimally to maintain visual fidelity.

## Understanding Design Implementation

The Figma-to-WPF workflow establishes a reliable process for translating designs to code:

**For designers:** Confidence that implementations will match their designs with pixel-perfect accuracy.
**For developers:** A structured approach that eliminates guesswork and reduces revision cycles.
**For teams:** Consistent, high-quality implementations that maintain AppDesignSystem integrity.

## Additional Resources

- [Figma MCP Server Documentation](https://developers.figma.com/docs/figma-mcp-server/)
- [WPF Layout Documentation — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/layout/)
- [CommunityToolkit.Mvvm Documentation](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
