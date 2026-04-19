---
name: ct-theme
description: Best practices for using the Cho Tot theme system (CMStaticThemeLoader, CMThemeChangeable, DS.TypoToken, DS.Button). Use when setting up theming in a ViewController, Cell, or custom view. Covers static theme access, dynamic theme switching, component styling, navigation bar theming, and anti-patterns to avoid.
---

# Theme Best Practices for Cho Tot iOS

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Guide for using the theme system consistently across UIKit components.

## Theme Types

```swift
// Available theme loaders
private let theme = CMStaticThemeLoader.defaultTheme  // Default Cho Tot theme
private let theme = CMStaticThemeLoader.jobTheme      // JOB module
private let theme = CMStaticThemeLoader.ptyTheme      // Property module
```

## Pattern 1: Static Theme (Recommended for Most Cases)

```swift
import UIKit
import CTCommon
import CTDesignSystem
import SnapKit

class MyViewController: UIViewController {
    private let theme = CMStaticThemeLoader.defaultTheme

    private func setupUI() {
        titleLabel.setStyle(DS.TypoToken.Header.Section(color: theme.text.textPrimary.color))
        view.backgroundColor = theme.background.backgroundPrimary.color
    }
}
```

## Pattern 2: Dynamic Theme with CMThemeChangeable

```swift
import UIKit
import CTCommon
import CTDesignSystem
import RxSwift

class MyViewController: UIViewController, CMThemeChangeable {
    private let disposeBag = DisposeBag()

    override func viewDidLoad() {
        super.viewDidLoad()
        setupUI()
        subscribeThemeChange().disposed(by: disposeBag)
    }

    // MARK: - CMThemeChangeable
    func changeTheme(_ theme: CMTheme) {
        UIView.animate(withDuration: 0.3) {
            self.applyTheme(theme)
        }
    }

    private func applyTheme(_ theme: CMTheme) {
        titleLabel.setStyle(DS.TypoToken.Header.Section(color: theme.text.textPrimary.color))
        view.backgroundColor = theme.background.backgroundPrimary.color
    }
}
```

## Pattern 3: Cell Theming

```swift
class MyTableViewCell: UITableViewCell, CMThemeChangeable {
    private let theme = CMStaticThemeLoader.defaultTheme

    override func awakeFromNib() {
        super.awakeFromNib()
        setupTheme()
    }

    func changeTheme(_ theme: CMTheme) {
        setupTheme(theme)
    }

    private func setupTheme(_ theme: CMTheme? = nil) {
        let currentTheme = theme ?? self.theme
        titleLabel.setStyle(DS.TypoToken.Label.Section(color: currentTheme.text.textPrimary.color))
        containerView.backgroundColor = currentTheme.background.backgroundSecondary.color
    }
}
```

## Component Theming Reference

### Typography (DSLabel)
```swift
// Headers
titleLabel.setStyle(DS.TypoToken.Header.Page(color: theme.text.textPrimary.color))    // SemiBold 20px
sectionLabel.setStyle(DS.TypoToken.Header.Section(color: theme.text.textPrimary.color)) // SemiBold 16px

// Body
bodyLabel.setStyle(DS.TypoToken.Body.Section(color: theme.text.textSecondary.color))  // Regular 14px
captionLabel.setStyle(DS.TypoToken.Body.Caption(color: theme.text.textSecondary.color))

// Labels
labelText.setStyle(DS.TypoToken.Label.Page(color: theme.text.textPrimary.color))      // Bold 16px
errorLabel.setStyle(DS.TypoToken.Body.Caption(color: theme.text.textError.color))
```

### Buttons (DSButton)
```swift
// Module-matched button styles
primaryButton.setStyle(DS.Button.primary(size: .medium, themeType: theme.type))
secondaryButton.setStyle(DS.Button.secondary(size: .medium, themeType: theme.type))
tertiaryButton.setStyle(DS.Button.tertiary(size: .medium, themeType: theme.type))

// Direct theme type usage
primaryButton.setStyle(DS.Button.primary(size: .medium, themeType: .default))
primaryButton.setStyle(DS.Button.primary(size: .medium, themeType: .job))
primaryButton.setStyle(DS.Button.primary(size: .medium, themeType: .pty))
```

### Backgrounds and Borders
```swift
// Backgrounds
view.backgroundColor = theme.background.backgroundPrimary.color
containerView.backgroundColor = theme.background.backgroundSecondary.color
overlayView.backgroundColor = theme.background.backgroundOverlay.color
warningBg.backgroundColor = theme.background.backgroundWarningLight.color

// Borders / Separators
separatorView.backgroundColor = theme.border.borderThin.color
cardView.layer.borderColor = theme.border.borderRegular.color.cgColor
```

### Text Colors
```swift
theme.text.textPrimary.color     // Main content
theme.text.textSecondary.color   // Supporting content
theme.text.textDisabled.color    // Disabled state
theme.text.textError.color       // Error messages
theme.text.textInverted.color    // On dark backgrounds
```

## Navigation Bar Theming

```swift
// Protocol-based (preferred)
class MyViewController: UIViewController, CTNavigationBarVeritcalizable {
    var ctNavigationBarData: CTNavigationBarData { .pty } // or .job, .gds, .chotot

    override func viewWillAppear(_ animated: Bool) {
        super.viewWillAppear(animated)
        applyNavigationBarData()
    }
}

// Manual
private func setupNavigationBar() {
    navigationController?.navigationBar.barTintColor = theme.background.backgroundBrand.color
    navigationController?.navigationBar.tintColor = theme.text.textPrimary.color
}
```

## Module → Theme Mapping

| Module | Theme Loader | Button ThemeType |
|--------|-------------|-----------------|
| Default / Generic | `CMStaticThemeLoader.defaultTheme` | `.default` |
| CTJOB / Job | `CMStaticThemeLoader.jobTheme` | `.job` |
| CTPTY / Property | `CMStaticThemeLoader.ptyTheme` | `.pty` |

## Anti-Patterns

```swift
// ❌ Hardcoded colors
titleLabel.textColor = UIColor.black
view.backgroundColor = UIColor.white

// ❌ Direct DefaultTheme access
let theme = DefaultTheme.defaultTheme

// ❌ Theme change without animation
func changeTheme(_ theme: CMTheme) {
    view.backgroundColor = theme.background.backgroundPrimary.color
}

// ✅ Always use CMStaticThemeLoader
private let theme = CMStaticThemeLoader.defaultTheme

// ✅ Animate theme changes
func changeTheme(_ theme: CMTheme) {
    UIView.animate(withDuration: 0.3) {
        self.view.backgroundColor = theme.background.backgroundPrimary.color
    }
}
```
