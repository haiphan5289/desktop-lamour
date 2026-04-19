# Theme Best Practices for iOS Swift UIKit - Chợ Tốt

## Overview

Hướng dẫn best practices cho việc sử dụng theme system trong iOS Swift UIKit dựa trên chuẩn mới nhất và architecture của Chợ Tốt app.

## Core Theme Architecture

### 1. Theme System Structure

```swift
// Theme hierarchy trong project
CTDesignSystem/
├── Theme/
│   ├── CMDefaultTheme.swift      // Định nghĩa các theme types
│   ├── CMTheme.swift             // Theme protocol và structure
│   └── ThemeType.swift           // Enum các loại theme

CTCommon/
├── Theme/
│   ├── CMStaticThemeLoader.swift // Static theme loader
│   ├── CMThemeChangeable.swift   // Theme changeable protocol
│   ├── CMThemeData.swift         // Theme data management
│   └── NavigationBar/            // Navigation bar theming
```

### 2. Theme Types Available

```swift
// Các theme types hiện có
public enum ThemeType {
    case `default`  // Theme chính của Chợ Tốt
    case job        // Theme cho JOB module
    case pty        // Theme cho Property module
}
```

## Essential Patterns

### 1. Static Theme Access (Recommended)

```swift
// ✅ PREFERRED - Sử dụng static theme loader
import UIKit
import CTCommon
import CTDesignSystem
import SnapKit

class MyViewController: UIViewController {
    private let theme = CMStaticThemeLoader.defaultTheme
    // private let theme = CMStaticThemeLoader.jobTheme
    // private let theme = CMStaticThemeLoader.ptyTheme
    
    private func setupUI() {
        titleLabel.setStyle(DS.TypoToken.Label.Section(color: theme.text.textPrimary.color))
        backgroundColor = theme.background.backgroundPrimary.color
    }
}
```

### 2. Dynamic Theme Support với CMThemeChangeable

```swift
// ✅ Cho ViewControllers cần dynamic theme switching
import UIKit
import CTCommon
import CTDesignSystem
import RxSwift
import SnapKit

class MyViewController: UIViewController, CMThemeChangeable {
    private let disposeBag = DisposeBag()
    
    override func viewDidLoad() {
        super.viewDidLoad()
        setupUI()
        
        // Subscribe to theme changes
        subscribeThemeChange()
            .disposed(by: disposeBag)
    }
    
    // MARK: - CMThemeChangeable
    func changeTheme(_ theme: CMTheme) {
        setupTheme(theme)
    }
    
    private func setupTheme(_ theme: CMTheme) {
        titleLabel.setStyle(DS.TypoToken.Label.Section(color: theme.text.textPrimary.color))
        subtitleLabel.setStyle(DS.TypoToken.Body.Caption(color: theme.text.textSecondary.color))
        view.backgroundColor = theme.background.backgroundPrimary.color
    }
}
```

### 3. Cell/Custom View Theming

```swift
// ✅ Theme setup cho custom cells
import CTCommon
import CTDesignSystem

class MyTableViewCell: UITableViewCell, CMThemeChangeable {
    private let theme = CMStaticThemeLoader.defaultTheme
    
    override func awakeFromNib() {
        super.awakeFromNib()
        setupTheme()
    }
    
    // MARK: - CMThemeChangeable
    func changeTheme(_ theme: CMTheme) {
        setupTheme(theme)
    }
    
    private func setupTheme(_ theme: CMTheme? = nil) {
        let currentTheme = theme ?? self.theme
        
        titleLabel.setStyle(DS.TypoToken.Label.Section(color: currentTheme.text.textPrimary.color))
        descriptionLabel.setStyle(DS.TypoToken.Body.Caption(color: currentTheme.text.textSecondary.color))
        containerView.backgroundColor = currentTheme.background.backgroundSecondary.color
    }
}
```

### 4. Module-Specific Theme Usage

```swift
// ✅ Theme specific cho module PTY
class PropertyViewController: UIViewController {
    private let theme = CMStaticThemeLoader.ptyTheme
    
    private func setupUI() {
        // Sử dụng PTY theme colors
        navigationController?.navigationBar.barTintColor = theme.background.backgroundBrand.color
        titleLabel.setStyle(DS.TypoToken.Label.Page(color: theme.text.textPrimary.color))
    }
}

// ✅ Theme specific cho module JOB
class JobViewController: UIViewController {
    private let theme = CMStaticThemeLoader.jobTheme
    
    private func setupUI() {
        // Sử dụng Job theme colors
        primaryButton.setStyle(DS.Button.primary(themeType: .job))
        titleLabel.setStyle(DS.TypoToken.Label.Page(color: theme.text.textPrimary.color))
    }
}
```

## Component Theming Best Practices

### 1. DSButton với Theme Support

```swift
// ✅ Button theming với theme type
primaryButton.setStyle(DS.Button.primary(size: .medium, themeType: .default))
secondaryButton.setStyle(DS.Button.secondary(size: .medium, themeType: .pty))

// ✅ Custom button colors từ theme
customButton.backgroundColor = theme.button.buttonPrimary.color
customButton.setTitleColor(theme.text.textInverted.color, for: .normal)
```

### 2. DSLabel/DSTextField với Theme Colors

```swift
// ✅ Typography với theme colors
titleLabel.setStyle(DS.TypoToken.Label.Page(color: theme.text.textPrimary.color))
bodyLabel.setStyle(DS.TypoToken.Body.Section(color: theme.text.textSecondary.color))
errorLabel.setStyle(DS.TypoToken.Body.Caption(color: theme.text.textError.color))

// ✅ Input fields
textField.textColor = theme.text.textPrimary.color
textField.backgroundColor = theme.background.backgroundSecondary.color
textField.layer.borderColor = theme.border.borderRegular.color.cgColor
```

### 3. Background và Border Colors

```swift
// ✅ Background theming
view.backgroundColor = theme.background.backgroundPrimary.color
containerView.backgroundColor = theme.background.backgroundSecondary.color
overlayView.backgroundColor = theme.background.backgroundOverlay.color

// ✅ Border theming
separatorView.backgroundColor = theme.border.borderThin.color
cardView.layer.borderColor = theme.border.borderRegular.color.cgColor
```

## Navigation Bar Theming

### 1. CTNavigationBarVeritcalizable Protocol

```swift
// ✅ Navigation bar theming
class MyViewController: UIViewController, CTNavigationBarVeritcalizable {
    
    // Default implementation returns .chotot
    // Override for different themes:
    var ctNavigationBarData: CTNavigationBarData {
        return .pty  // hoặc .gds, .job
    }
    
    override func viewWillAppear(_ animated: Bool) {
        super.viewWillAppear(animated)
        applyNavigationBarData()
    }
}
```

### 2. Custom Navigation Bar Styling

```swift
// ✅ Manual navigation bar theming
private func setupNavigationBar() {
    navigationController?.navigationBar.barTintColor = theme.background.backgroundBrand.color
    navigationController?.navigationBar.tintColor = theme.text.textPrimary.color
    navigationController?.navigationBar.titleTextAttributes = [
        .foregroundColor: theme.text.textPrimary.color,
        .font: DS.TypoToken.Label.Page().font
    ]
}
```

## Advanced Theme Patterns

### 1. Theme Subscription Management

```swift
// ✅ Proper theme subscription management
class MyViewController: UIViewController, CMThemeChangeable {
    private let disposeBag = DisposeBag()
    
    override func viewDidLoad() {
        super.viewDidLoad()
        setupThemeSubscription()
    }
    
    private func setupThemeSubscription() {
        // Subscribe to theme changes
        subscribeThemeChange()
            .disposed(by: disposeBag)
        
        // Or subscribe to specific theme
        subscribeTheme(theme: CMStaticThemeLoader.ptyTheme)
            .disposed(by: disposeBag)
    }
    
    func changeTheme(_ theme: CMTheme) {
        UIView.animate(withDuration: 0.3) {
            self.applyTheme(theme)
        }
    }
}
```

### 2. Theme-Aware Custom Components

```swift
// ✅ Custom component với theme support
class ThemedCardView: UIView, CMThemeChangeable {
    private var currentTheme: CMTheme = CMStaticThemeLoader.defaultTheme
    
    override init(frame: CGRect) {
        super.init(frame: frame)
        setupUI()
        setupTheme(currentTheme)
    }
    
    func changeTheme(_ theme: CMTheme) {
        currentTheme = theme
        setupTheme(theme)
    }
    
    private func setupTheme(_ theme: CMTheme) {
        backgroundColor = theme.background.backgroundSecondary.color
        layer.borderColor = theme.border.borderRegular.color.cgColor
        
        // Update child views
        titleLabel.setStyle(DS.TypoToken.Label.Section(color: theme.text.textPrimary.color))
        subtitleLabel.setStyle(DS.TypoToken.Body.Caption(color: theme.text.textSecondary.color))
    }
}
```

### 3. Theme Context Passing

```swift
// ✅ Pass theme context to child components
class ParentViewController: UIViewController {
    private let theme = CMStaticThemeLoader.defaultTheme
    
    private func setupChildViewController() {
        let childVC = ChildViewController(theme: theme)
        addChild(childVC)
        view.addSubview(childVC.view)
        childVC.didMove(toParent: self)
    }
}

class ChildViewController: UIViewController {
    private let theme: CMTheme
    
    init(theme: CMTheme) {
        self.theme = theme
        super.init(nibName: nil, bundle: nil)
    }
}
```

## Common Anti-Patterns

### ❌ Avoid Hardcoded Colors

```swift
// ❌ BAD - Hardcoded colors
titleLabel.textColor = UIColor.black
backgroundColor = UIColor.white
button.backgroundColor = UIColor.blue

// ✅ GOOD - Theme colors
titleLabel.setStyle(DS.TypoToken.Label.Section(color: theme.text.textPrimary.color))
backgroundColor = theme.background.backgroundPrimary.color
button.backgroundColor = theme.button.buttonPrimary.color
```

### ❌ Avoid Direct Theme Access Without Context

```swift
// ❌ BAD - Accessing theme without proper context
let theme = DefaultTheme.defaultTheme // Direct access

// ✅ GOOD - Use static loader
let theme = CMStaticThemeLoader.defaultTheme
```

### ❌ Avoid Theme Switching Without Animation

```swift
// ❌ BAD - Abrupt theme change
func changeTheme(_ theme: CMTheme) {
    view.backgroundColor = theme.background.backgroundPrimary.color
}

// ✅ GOOD - Animated theme change
func changeTheme(_ theme: CMTheme) {
    UIView.animate(withDuration: 0.3) {
        self.view.backgroundColor = theme.background.backgroundPrimary.color
    }
}
```

## Testing Theme Implementation

### 1. Theme Testing Pattern

```swift
// ✅ Unit testing với themes
class MyViewControllerTests: XCTestCase {
    
    func testThemeApplication() {
        let sut = MyViewController()
        let testTheme = CMStaticThemeLoader.ptyTheme
        
        sut.changeTheme(testTheme)
        
        XCTAssertEqual(sut.view.backgroundColor, testTheme.background.backgroundPrimary.color)
    }
    
    func testThemeSubscription() {
        let sut = MyViewController()
        let expectation = XCTestExpectation(description: "Theme changed")
        
        // Test theme subscription
        CMThemeData.shared.updateTheme(themeType: .pty)
        
        DispatchQueue.main.asyncAfter(deadline: .now() + 0.1) {
            expectation.fulfill()
        }
        
        wait(for: [expectation], timeout: 1.0)
    }
}
```

## Performance Considerations

### 1. Theme Caching

```swift
// ✅ Cache theme objects
class ThemeCacheManager {
    private static var cachedThemes: [ThemeType: CMTheme] = [:]
    
    static func theme(for type: ThemeType) -> CMTheme {
        if let cached = cachedThemes[type] {
            return cached
        }
        
        let theme = DefaultTheme.themeWithType(type: type)
        cachedThemes[type] = theme
        return theme
    }
}
```

## Summary

1. **Always use `CMStaticThemeLoader`** cho static theme access
2. **Implement `CMThemeChangeable`** cho dynamic theme support  
3. **Use proper theme types** (.default, .job, .pty) based on module
4. **Leverage CTDesignSystem components** với theme support
5. **Animate theme transitions** for better UX
6. **Test theme implementations** thoroughly
7. **Avoid hardcoded colors** - always use theme properties
8. **Cache themes** for performance optimization

Tuân thủ những best practices này sẽ đảm bảo theme system được sử dụng một cách consistent và maintainable trong toàn bộ iOS app.