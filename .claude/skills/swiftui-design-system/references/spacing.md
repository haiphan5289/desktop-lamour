# CT Design System Spacing

**Last synced:** 2026-01-26

Source: `${CT_DESIGN_SYSTEM_REPO}/Sources/CTDesignSystemSwiftUI/Sources/Typos/`

---

## Gap (Element Spacing)

| Token | Value | Use Case |
|-------|-------|----------|
| `DS.Gap.gapMin` | 2px | Minimum |
| `DS.Gap.gap2xSmall` | 4px | Tight |
| `DS.Gap.gapxSmall` | 8px | Small |
| `DS.Gap.gapSmall` | 12px | Compact |
| `DS.Gap.gapMedium` | 16px | Standard |
| `DS.Gap.gapLarge` | 20px | Large |

## Padding (Container Spacing)

| Token | Value |
|-------|-------|
| `DS.Padding.paddingMin` | 2px |
| `DS.Padding.padding2xSmall` | 4px |
| `DS.Padding.paddingxSmall` | 8px |
| `DS.Padding.paddingSmall` | 12px |
| `DS.Padding.paddingMedium` | 16px |
| `DS.Padding.paddingLarge` | 20px |

## StrokeLine (Border Widths)

| Token | Value |
|-------|-------|
| `DS.StrokeLine.strokeDivide` | 1px |
| `DS.StrokeLine.strokeAction` | 2px |
| `DS.StrokeLine.strokeEmphasize` | 3px |

## BorderRadius (Corner Radius)

| Token | Value | Use Case |
|-------|-------|----------|
| `DS.BorderRadius.radiusAdSmall` | 4px | Small ad cards |
| `DS.BorderRadius.radiusAd` | 6px | Ad cards |
| `DS.BorderRadius.radiusCardSmall` | 8px | Small cards |
| `DS.BorderRadius.radiusCard` | 12px | Standard cards |
| `DS.BorderRadius.radiusModal` | 20px | Modals, sheets |
| `DS.BorderRadius.radiusPill` | height/2 | Pill shapes |

## CGFloat Extensions

```swift
// Spacing
VStack(spacing: .gapMedium) { }
.padding(.horizontal, .paddingLarge)

// Border radius
.cornerRadius(DS.BorderRadius.radiusCard.value())

// Stroke width
.overlay(RoundedRectangle()
    .stroke(lineWidth: .strokeDivide))
```
