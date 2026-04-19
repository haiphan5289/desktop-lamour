# CT Design System Colors

**Last synced:** 2026-01-26

Source: `${CT_DESIGN_SYSTEM_REPO}/Sources/CTDesignSystemSwiftUI/Sources/ColorThemes/`

---

## Color Families

### Base Colors (7 families)

| Family | Shades | Description |
|--------|--------|-------------|
| Red | 50-900 | Light pink to deep maroon |
| Orange | 50-900 | Peachy to dark brown |
| Blue | 50-900 | Sky blue to navy |
| Yellow | 50-900 | Pale cream to dark gold |
| Green | 50-900 | Mint to forest green |
| Gray | 0, 25, 50-900 | Pure white to near black (11 values) |

### Brand Colors (4 families)

| Family | Based On | Brand |
|--------|----------|-------|
| Chotot | Yellow/Gold | Cho Tot marketplace |
| ViecLamTot | Blue | Job listings |
| NhaTot | Orange-red | Property |
| ChototXe | Yellow | Vehicle |

### Shade Scale

Light: 50, 75, 100, 200, 300 | Mid: 400, 500 | Dark: 600, 700, 800, 900

Gray exception: 0 (white), 25, then 50-900

---

## Sub-Protocol Access Pattern

```swift
@Environment(\.colorTheme) var theme

// Access pattern: theme.<sub-protocol>.<property>
theme.text.textPrimary
theme.background.backgroundSecondary
theme.button.buttonPrimary
theme.border.borderDivider
theme.icon.iconBrand
theme.interaction.solidHoverBrand
```

---

## CDSColorThemeTextType (15 properties)

Access: `theme.text.*`

| Property | Usage |
|----------|-------|
| `textBrand` | Brand-colored text |
| `textPrimary` | Main content text |
| `textSecondary` | Supporting text |
| `textTertiary` | Least important text |
| `textOnBackground` | Text on colored backgrounds |
| `textLink` | Hyperlinks |
| `textError` | Error messages |
| `textSuccess` | Success messages |
| `textInfo` | Info messages |
| `textWarning` | Warning messages |
| `textDisabled` | Disabled state |
| `textBlank` | White text |
| `textOverlay` | Overlay text |
| `textOverlaySecondary` | Secondary overlay text |

---

## CDSColorThemeBackgroundType (16 properties)

Access: `theme.background.*`

| Property | Usage |
|----------|-------|
| `backgroundBrand` | Brand-colored surface |
| `backgroundAppwrapper` | App wrapper background |
| `backgroundPrimary` | Main app background |
| `backgroundSecondary` | Cards, sections |
| `backgroundTertiary` | Nested containers |
| `backgroundError` | Error state surface |
| `backgroundSuccess` | Success state surface |
| `backgroundInfo` | Info state surface |
| `backgroundWarning` | Warning state surface |
| `backgroundInverted` | Inverted background |
| `backgroundOverlay` | Modal overlay |
| `backgroundBrandLight` | Light brand tint |
| `backgroundErrorLight` | Light error surface |
| `backgroundInfoLight` | Light info surface |
| `backgroundSuccessLight` | Light success surface |
| `backgroundWarningLight` | Light warning surface |
| `backgroundChotot` | Chotot brand background |

---

## CDSColorThemeButtonType (15 properties)

Access: `theme.button.*`

| Property | Usage |
|----------|-------|
| `buttonPrimary` | Primary action |
| `buttonInfo` | Info button |
| `buttonSuccess` | Success button |
| `buttonError` | Error/destructive button |
| `buttonChotot` | Chotot brand button |
| `buttonNeutral` | Neutral button |
| `buttonBlank` | White button |
| `buttonDisabled` | Disabled state |
| `buttonAppwrapper` | App wrapper button |
| `buttonSecondary` | Secondary action |
| `buttonTonalSuccess` | Tonal success |
| `buttonTonalError` | Tonal error |
| `buttonTonalInfo` | Tonal info |
| `buttonTonalNeutral` | Tonal neutral |

---

## CDSColorThemeBorderType (13 properties)

Access: `theme.border.*`

| Property | Usage |
|----------|-------|
| `borderThin` | Thin border |
| `borderRegular` | Standard border |
| `borderBold` | Bold border |
| `borderDivider` | Section divider |
| `borderActive` | Active/focused state |
| `borderDisabled` | Disabled state |
| `borderBrand` | Brand-colored border |
| `borderInfo` | Info border |
| `borderSuccess` | Success border |
| `borderError` | Error border |
| `borderWarning` | Warning border |
| `borderBlank` | White border |
| `borderOverlay` | Overlay border |
| `borderBlack` | Black border |

---

## CDSColorThemeIconType (13 properties)

Access: `theme.icon.*`

| Property | Usage |
|----------|-------|
| `iconBrand` | Brand icon |
| `iconPrimary` | Primary icon |
| `iconSecondary` | Secondary icon |
| `iconTertiary` | Tertiary icon |
| `iconChotot` | Chotot icon |
| `iconChototBold` | Chotot bold icon |
| `iconOnBackground` | Icon on colored background |
| `iconBlank` | White icon |
| `iconInfo` | Info icon |
| `iconSuccess` | Success icon |
| `iconError` | Error icon |
| `iconWarning` | Warning icon |
| `iconDisabled` | Disabled icon |

---

## CDSColorThemeInteractionType (25 properties)

Access: `theme.interaction.*`

### Solid Hover (7)
`solidHoverChotot`, `solidHoverBrand`, `solidHoverError`, `solidHoverInfo`, `solidHoverSuccess`, `solidHoverWarning`, `solidHoverNeutral`

### Solid Pressed (7)
`solidPressedChotot`, `solidPressedBrand`, `solidPressedError`, `solidPressedInfo`, `solidPressedSuccess`, `solidPressedWarning`, `solidPressedNeutral`

### Light Hover (7)
`lightHoverBrand`, `lightHoverError`, `lightHoverInfo`, `lightHoverSuccess`, `lightHoverWarning`, `lightHoverNeutral`, `lightHoverPrimary`

### Light Pressed (4)
`lightPressedBrand`, `lightPressedError`, `lightPressedInfo`, `lightPressedSuccess`, `lightPressedWarning`, `lightPressedNeutral`

---

## Usage Examples

```swift
@Environment(\.colorTheme) var theme

// Text colors
Text("Primary").foregroundColor(theme.text.textPrimary)
Text("Error").foregroundColor(theme.text.textError)
Text("Link").foregroundColor(theme.text.textLink)

// Background colors
Rectangle().fill(theme.background.backgroundSecondary)
VStack { }.background(theme.background.backgroundPrimary)

// Button colors (for custom buttons)
Button { }.background(theme.button.buttonPrimary)

// Icon colors
Image(systemName: "star").foregroundColor(theme.icon.iconBrand)
Image(systemName: "xmark").foregroundColor(theme.icon.iconError)

// Border colors
.overlay(RoundedRectangle(cornerRadius: 8)
    .stroke(theme.border.borderDivider, lineWidth: 1))
```

---

## Notes

- Dark mode: Not yet available. All themes currently light mode only.
- Total properties: 82 across 6 sub-protocols
- Source files: `CDSColorTheme*.swift` in ColorThemes/ directory
