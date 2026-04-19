# CT Design System (CDS) SwiftUI Components

## Typography
Use `.cdsTextStyle()` modifier on native `Text` or `CDSText`.

- `.cdsTextStyle(.displayPage)`
- `.cdsTextStyle(.headerSection)`
- `.cdsTextStyle(.bodySection)`
- `.cdsTextStyle(.labelPage)`

## Buttons
Use native `Button` with `.cdsButtonStyle()`.

```swift
Button("Action") { /* ... */ }
    .cdsButtonStyle(.primary, size: .medium)

Button(action: {}) {
    Image(systemName: "plus")
}
.cdsButtonStyle(.secondaryIcon, size: .iconMedium)
```

## Text Fields
Use `CDSTextField` for standard input forms.

```swift
CDSTextField(
    text: $text,
    placeholder: "Enter name",
    isError: $isError
)
```

## Layout & Containers
- `ListView`: A wrapper around `List` or `ScrollView` with built-in loading and empty state handling.
- `LinearGradientDivider`: Standard separator between sections.
- `RoundedSection`: A container with rounded corners and standard padding.

## Colors
Access colors via `CTColor`. In SwiftUI, use `.swiftUIColor`.

```swift
Text("Hello")
    .foregroundColor(CTColor.gray900.swiftUIColor)
```
