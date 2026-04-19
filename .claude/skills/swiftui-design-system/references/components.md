# CT Design System Components

**Last synced:** 2026-01-26

Source: `${CT_DESIGN_SYSTEM_REPO}/CTDesignSystemSwiftUIApp/`

---

## Overview

47 components across 13 categories. Each section shows category example.

---

## Buttons (8 components)

### Styles
| Style | Usage |
|-------|-------|
| `.primary` | Main CTA, highest emphasis |
| `.primarySuccess` | Positive confirmation |
| `.primaryBlack` | Dark primary action |
| `.secondary` | Secondary/outlined action |
| `.tertiary` | Low emphasis action |
| `.ghost` | Minimal/text-only action |

### Sizes
| Size | Height |
|------|--------|
| `.large` | 40pt |
| `.medium` | 32pt |
| `.small` | 24pt |

### Example
```swift
// Primary button
Button("Save") { }
    .cdsButtonStyle(.primary, size: .large, sizeToFit: false)

// Secondary with loading
Button("Cancel") { }
    .cdsButtonStyle(.secondary, size: .medium)
    .cdsButtonLoading(isLoading)

// Ghost button
Button("Skip") { }
    .cdsButtonStyle(.ghost)

// Pill shape
Button("Tag") { }
    .cdsButtonStyle(.primary)
    .cdsButtonPilled(true)
```

Demo: `${CT_DESIGN_SYSTEM_REPO}/CTDesignSystemSwiftUIApp/Button/ButtonStylesExampleView.swift`

---

## Icon Buttons (2 components)

| Component | Usage |
|-----------|-------|
| `CDSIconButton` | Icon-only button |
| `CDSIconTextButton` | Icon with text |

### Example
```swift
// Icon button
CDSIconButton(icon: .system("plus")) { }
    .cdsIconButtonStyle(.primary)

// Icon with text
CDSIconTextButton(icon: .system("star"), title: "Favorite") { }
```

Demo: `${CT_DESIGN_SYSTEM_REPO}/CTDesignSystemSwiftUIApp/Button/IconButtonExampleView.swift`

---

## Input Fields (6 components)

| Component | Usage |
|-----------|-------|
| `CDSTextField` | Single-line input |
| `CDSTextView` | Multi-line input |
| `CDSSearchInput` | Search field |
| `CDSDropdown` | Dropdown selector |
| `CDSRangeTextField` | Range input (min-max) |
| `CDSInputGroup` | Grouped inputs |

### Modifiers
| Modifier | Effect |
|----------|--------|
| `.fieldRequired(true)` | Mark as required |
| `.fieldValid(false)` | Show validation state |
| `.fieldClearButtonMode(.whileEditing)` | Clear button |
| `.fieldCharacterLimit(100)` | Character limit |
| `.helpText("Helper")` | Help text below |

### Example
```swift
// Text field with validation
CDSTextField("Email", placeholder: "user@example.com", text: $email)
    .fieldRequired(true)
    .fieldValid(isEmailValid)
    .fieldCharacterLimit(100)

// Multi-line text view
CDSTextView("Description", placeholder: "Enter description", text: $description)
    .fieldRequired(true)
    .fieldCharacterLimit(500)

// Search input
CDSSearchInput("Search products", text: $query)
    .showsLeftImage(true)

// Dropdown
CDSDropdown(selection: $selected, options: options) { item in
    Text(item.title)
}
```

Demo: `${CT_DESIGN_SYSTEM_REPO}/CTDesignSystemSwiftUIApp/Input/TextFieldView.swift`

---

## Selection Controls (3 components)

| Style | Usage |
|-------|-------|
| `.checkbox` | Multi-select, checkmark |
| `.switch` | On/off toggle |
| `.radio` | Single-select from group |

### Checkbox Styles
| Style | Look |
|-------|------|
| `.brand` | Brand color checkmark |
| `.success` | Green checkmark |

### Layout Styles
| Style | Order |
|-------|-------|
| `.labelThenImage` | Label first |
| `.imageThenLabel` | Checkbox first |

### Example
```swift
// Switch toggle
Toggle("Enable notifications", isOn: $isEnabled)
    .cdsToggleStyle(.switch)

// Checkbox with brand color
Toggle("Agree to terms", isOn: $agreed)
    .cdsToggleStyle(.checkbox(style: .brand, layoutStyle: .labelThenImage))

// Radio button
Toggle("Option A", isOn: $optionA)
    .cdsToggleStyle(.radio())
```

Demo: `${CT_DESIGN_SYSTEM_REPO}/CTDesignSystemSwiftUIApp/Selection/CheckboxDemoView.swift`

---

## Navigation (2 components)

| Component | Usage |
|-----------|-------|
| `CDSTabView` | Tab bar container |
| `CDSTab` | Individual tab item |

### Example
```swift
CDSTabView(selection: $selectedTab) {
    CDSTab(icon: .system("house"), title: "Home", tag: 0)
    CDSTab(icon: .system("magnifyingglass"), title: "Search", tag: 1)
    CDSTab(icon: .system("person"), title: "Profile", tag: 2)
}
```

---

## Sliders (2 components)

| Component | Usage |
|-----------|-------|
| `CDSSlider` | Single value slider |
| `CDSRangeSlider` | Min-max range slider |

### Example
```swift
// Single slider
CDSSlider(value: $price, in: 0...1000)

// Range slider
CDSRangeSlider(lowValue: $minPrice, highValue: $maxPrice, in: 0...1000)
```

---

## Steppers (5 components)

| Component | Usage |
|-----------|-------|
| `CDSStepperView` | Step progress indicator |
| `CDSStep` | Individual step |
| `CDSHorizontalStepper` | Horizontal layout |
| `CDSVerticalStepper` | Vertical layout |
| `CDSStepConnector` | Line between steps |

### Example
```swift
CDSStepperView(currentStep: $currentStep) {
    CDSStep(index: 0, title: "Details")
    CDSStep(index: 1, title: "Payment")
    CDSStep(index: 2, title: "Confirm")
}
```

---

## Containers (9 components)

| Component | Usage |
|-----------|-------|
| `CDSBottomSheet` | Bottom modal sheet |
| `CDSTooltip` | Tooltip popup |
| `CDSCard` | Card container |
| `CDSSection` | Section grouping |
| `CDSDivider` | Section divider |
| `CDSExpandableSection` | Collapsible section |
| `CDSListItem` | List row item |
| `CDSEmptyState` | Empty state view |
| `CDSSkeleton` | Loading skeleton |

### Example
```swift
// Bottom sheet
CDSBottomSheet(isPresented: $showSheet) {
    VStack {
        Text("Sheet Content").cdsTextStyle(.headerSection)
        Button("Close") { showSheet = false }
            .cdsButtonStyle(.primary)
    }
}

// Tooltip
Text("Hover me")
    .cdsTooltip("This is helpful info")

// Card with proper styling
VStack {
    Text("Card Title").cdsTextStyle(.headerSection)
    Text("Card content").cdsTextStyle(.bodySection)
}
.padding(DS.Padding.paddingMedium)
.background(theme.background.backgroundSecondary)
.cornerRadius(DS.BorderRadius.radiusCard.value())
```

Demo: `${CT_DESIGN_SYSTEM_REPO}/CTDesignSystemSwiftUIApp/Container/BottomSheetDemoView.swift`

---

## Popups (3 components)

| Component | Usage |
|-----------|-------|
| `CDSPopupView` | Popup container |
| `CDSPopupModifier` | Popup as modifier |
| `CDSPopupButton` | Button that shows popup |

### Example
```swift
// Popup view
CDSPopupView(isPresented: $showPopup) {
    VStack {
        Text("Confirm Action").cdsTextStyle(.headerSection)
        HStack {
            Button("Cancel") { showPopup = false }
                .cdsButtonStyle(.secondary)
            Button("Confirm") { performAction() }
                .cdsButtonStyle(.primary)
        }
    }
}

// Popup button
CDSPopupButton("Options") {
    Button("Edit") { }
    Button("Delete") { }
}
```

Demo: `${CT_DESIGN_SYSTEM_REPO}/CTDesignSystemSwiftUIApp/Container/PopupExampleView.swift`

---

## Feedback (4 components)

| Component | Usage |
|-----------|-------|
| `CDSAnnouncerView` | Banner notification |
| `CDSSnackBarView` | Bottom snackbar |
| `CDSToast` | Brief toast message |
| `CDSProgressView` | Progress indicator |

### Announcer Types
| Type | Color |
|------|-------|
| `.success` | Green |
| `.error` | Red |
| `.warning` | Orange |
| `.info` | Blue |
| `.neutral` | Gray |

### Example
```swift
// Announcer banner
CDSAnnouncerView(
    title: "Success",
    message: "Your file has been uploaded.",
    onDismiss: { showAnnouncer = false }
)
.cdsAnnouncerType(.success)
.showCloseButton(true)

// Snackbar
.cdsSnackBar(
    isPresented: $showSnackBar,
    message: "Item saved",
    actionText: "Undo",
    action: { undoSave() },
    position: .bottom
)

// Toast
.cdsToast(isPresented: $showToast, message: "Copied!")
```

Demo: `${CT_DESIGN_SYSTEM_REPO}/CTDesignSystemSwiftUIApp/Feedback/SnackBarDemoView.swift`

---

## Badges & Chips (3 components)

| Component | Usage |
|-----------|-------|
| `CDSBadge` | Status badge |
| `CDSChip` | Selectable chip |
| `CDSTag` | Label tag |

### Badge Sizes
| Size | Usage |
|------|-------|
| `.small` | Compact |
| `.medium` | Default |
| `.large` | Prominent |

### Example
```swift
// Badge
CDSBadge("New")
    .badgeSize(.medium)
    .isPill(true)

// Chip (selectable)
CDSChip("Category", isSelected: $isSelected)

// Multiple chips
HStack(spacing: DS.Gap.gapSmall) {
    ForEach(categories, id: \.self) { category in
        CDSChip(category, isSelected: selectedCategories.contains(category)) {
            toggleCategory(category)
        }
    }
}
```

Demo: `${CT_DESIGN_SYSTEM_REPO}/CTDesignSystemSwiftUIApp/Badge/BadgeExampleView.swift`

---

## Images & Media (3 components)

| Component | Usage |
|-----------|-------|
| `CDSAsyncImage` | Async image loading |
| `CDSAvatar` | User avatar |
| `CDSImageCarousel` | Image gallery |

### Example
```swift
// Avatar
CDSAvatar(url: user.avatarURL, size: .medium)

// Async image
CDSAsyncImage(url: imageURL) {
    ProgressView()
} image: { image in
    image.resizable().aspectRatio(contentMode: .fill)
}
```

---

## Component Mapping Guide

| Visual Element | Implementation |
|----------------|----------------|
| Primary filled button | `Button(...).cdsButtonStyle(.primary)` |
| Outlined button | `Button(...).cdsButtonStyle(.secondary)` |
| Text-only button | `Button(...).cdsButtonStyle(.ghost)` |
| Card with shadow | `VStack { }.background(theme.background.backgroundSecondary).shadow(radius: 2).cornerRadius(DS.BorderRadius.radiusCard.value())` |
| Single-line input | `CDSTextField(...)` |
| Multi-line input | `CDSTextView(...)` |
| Search field | `CDSSearchInput(...)` |
| Toggle switch | `Toggle(...).cdsToggleStyle(.switch)` |
| Checkbox | `Toggle(...).cdsToggleStyle(.checkbox(style: .brand))` |
| Status badge | `CDSBadge(...)` |
| Bottom sheet | `CDSBottomSheet(...)` |
| Toast message | `.cdsToast(...)` |
| Snackbar | `.cdsSnackBar(...)` |

---

## Notes

- All components support theme injection via `.environment(\.colorTheme, ...)`
- Most components have corresponding demo views in CTDesignSystemSwiftUIApp
- Prefer modifier-based APIs (e.g., `.cdsButtonStyle()`) over initializer parameters
- Total: 47 components across 13 categories
