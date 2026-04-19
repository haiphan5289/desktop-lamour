# CDS Components API Reference

## Buttons

### Styles (`CDSButtonStyle`)
- `.primary`: Brand color background.
- `.primarySuccess`: Success color.
- `.primaryBlack`: Dark theme.
- `.secondary`: Tonal neutral.
- `.tertiary`: Bordered.
- `.ghost`: Minimal.
- `Icon` variants: `.secondaryIcon`, `.tertiaryIcon`, `.ghostIcon`.

### Usage
```swift
Button("Submit") { action() }
    .cdsButtonStyle(.primary, size: .large)
    .cdsButtonLoading(viewModel.isLoading)
    .cdsButtonPilled(true)
```

## Inputs

### CDSTextField
```swift
CDSTextField(
    text: $viewModel.state.name,
    placeholder: "Full Name",
    isError: $viewModel.state.hasError,
    errorMessage: "Required field"
)
```

## Modals & Overlays

### Popups (`.cdsPopup`)
Popups are centered modal dialogs.

```swift
// Basic boolean presentation
.cdsPopup(
    isPresented: $showPopup,
    title: "Confirm Action",
    message: "Are you sure you want to delete?",
    showCloseButton: true
) {
    Button("Cancel") { showPopup = false }.cdsButtonStyle(.secondary)
    Button("Delete") { performDelete() }.cdsButtonStyle(.primary)
}

// Item-based presentation (similar to .alert(item:))
.cdsPopup(item: $activeItem, title: { item in item.title }) { item in
    Button("OK") { activeItem = nil }.cdsButtonStyle(.primary)
}
```

### Bottom Sheets (`.cdsBottomSheet`)
Bottom sheets slide up from the bottom.

```swift
.cdsBottomSheet(
    isPresented: $showSheet,
    title: "Select Option"
) {
    MySheetContentView()
} rightView: {
    Button("Done") { showSheet = false }
}
```

## Colors
Colors are accessed via `CTColor`. In SwiftUI, always use the `.swiftUIColor` extension.

```swift
.background(CTColor.gray50.swiftUIColor)
```
