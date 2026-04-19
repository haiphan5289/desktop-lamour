# CT Design System Typography

**Last synced:** 2026-01-26

Source: `${CT_DESIGN_SYSTEM_REPO}/Sources/CTDesignSystemSwiftUI/Sources/Typos/`

---

## Typography Tokens (23 total)

### Display (Headlines) - 4 tokens

| Token | Size | Weight | Line Height |
|-------|------|--------|-------------|
| `.displayPage` | 32px | Bold | 40px |
| `.displaySection` | 24px | Bold | 32px |
| `.displayCaption` | 20px | Bold | 28px |
| `.displayAnnotation` | 18px | Bold | 26px |

### Header (Subheadings) - 4 tokens

| Token | Size | Weight | Line Height |
|-------|------|--------|-------------|
| `.headerPage` | 20px | SemiBold | 28px |
| `.headerSection` | 16px | SemiBold | 24px |
| `.headerCaption` | 14px | SemiBold | 20px |
| `.headerAnnotation` | 12px | SemiBold | 18px |

### Label (Strong Text) - 4 tokens

| Token | Size | Weight | Line Height |
|-------|------|--------|-------------|
| `.labelPage` | 16px | SemiBold | 24px |
| `.labelSection` | 14px | Bold | 20px |
| `.labelCaption` | 12px | Bold | 18px |
| `.labelAnnotation` | 10px | Bold | 16px |

### Body (Regular Text) - 4 tokens

| Token | Size | Weight | Line Height |
|-------|------|--------|-------------|
| `.bodyPage` | 16px | Regular | 24px |
| `.bodySection` | 14px | Regular | 20px |
| `.bodyCaption` | 12px | Regular | 18px |
| `.bodyAnnotation` | 10px | Regular | 16px |

### Note (Italic Text) - 3 tokens

| Token | Size | Weight | Line Height |
|-------|------|--------|-------------|
| `.notePage` | 14px | Regular Italic | 20px |
| `.noteSection` | 12px | Regular Italic | 20px |
| `.noteCaption` | 10px | Regular Italic | 16px |

### Tagline (Medium Weight) - 4 tokens

| Token | Size | Weight | Line Height |
|-------|------|--------|-------------|
| `.taglineSection` | 16px | Medium | 20px |
| `.taglineCaption` | 14px | Medium | 20px |
| `.taglineAnnotation` | 12px | Medium | 18px |
| `.taglineFootext` | 10px | Medium | 16px |

---

## .cdsTextStyle() Modifier

```swift
// Basic usage
Text("Title").cdsTextStyle(.headerPage)

// With custom color override
Text("Error").cdsTextStyle(.bodySection, color: theme.text.textError)

// Implementation handles: font, line spacing, foreground color
```

---

## Base T*R/B/O Tokens

Raw tokens for flexible use (R=Regular, B=Bold, O=Italic):

```swift
DS.T8R, DS.T8B, DS.T8O     // 8px
DS.T10R, DS.T10B, DS.T10O  // 10px
DS.T12R, DS.T12B, DS.T12O  // 12px
DS.T14R, DS.T14B, DS.T14O  // 14px
DS.T16R, DS.T16B, DS.T16O  // 16px
DS.T18R, DS.T18B, DS.T18O  // 18px
DS.T20R, DS.T20B, DS.T20O  // 20px
DS.T24R, DS.T24B, DS.T24O  // 24px
DS.T32R, DS.T32B, DS.T32O  // 32px
```

---

## Font Weights (8)

| Weight | Font Name | Value |
|--------|-----------|-------|
| `.regular` | RedditSans-Regular | 400 |
| `.regularItalic` | RedditSans-Italic | 400 |
| `.medium` | RedditSans-Medium | 500 |
| `.mediumItalic` | RedditSans-MediumItalic | 500 |
| `.semibold` | RedditSans-SemiBold | 600 |
| `.semiboldItalic` | RedditSans-SemiBoldItalic | 600 |
| `.bold` | RedditSans-Bold | 700 |
| `.boldItalic` | RedditSans-BoldItalic | 700 |

**Required:** Call `DS.registerFonts()` in App init.
