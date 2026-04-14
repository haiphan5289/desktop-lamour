---
name: ct-theme
description: Best practices for using the WPF AppDesignSystem theme system (AppThemeManager, ResourceDictionary, AppTypography, AppButton styles). Use when setting up theming in a UserControl, DataTemplate, or custom control. Covers static theme access, dynamic theme switching, component styling, and anti-patterns to avoid.
---

# Theme Best Practices for WPF AppDesignSystem

Guide for using the theme system consistently across WPF components.

## Theme Types

```xml
<!-- Available themes — merge in App.xaml or window ResourceDictionary -->
<ResourceDictionary Source="/App;component/Themes/DefaultTheme.xaml"/>
<!-- or -->
<ResourceDictionary Source="/App;component/Themes/DarkTheme.xaml"/>
```

## Pattern 1: Static ResourceDictionary Styles (Recommended for Most Cases)

```xml
<!-- UserControl or Window -->
<UserControl xmlns:local="clr-namespace:App.Features.Example.Views">
    <StackPanel>
        <local:AppLabel Text="{Binding Title}"
                        Style="{StaticResource AppTypography.HeaderSection}"/>
        <local:AppLabel Text="{Binding Subtitle}"
                        Style="{StaticResource AppTypography.BodyCaption}"
                        Foreground="{StaticResource AppColor.TextSecondary}"/>
    </StackPanel>
</UserControl>
```

```csharp
// Code-behind: no hardcoded colors — rely on ResourceDictionary
// Background = (Brush)Application.Current.Resources["AppColor.BackgroundPrimary"];
```

## Pattern 2: Dynamic Theme Switching

```csharp
// AppThemeManager.cs
public static class AppThemeManager
{
    public static void ApplyTheme(string themeName)
    {
        var uri = new Uri($"/App;component/Themes/{themeName}Theme.xaml", UriKind.Relative);
        var dict = new ResourceDictionary { Source = uri };

        var existing = Application.Current.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source?.OriginalString.Contains("Theme") == true);

        if (existing != null)
            Application.Current.Resources.MergedDictionaries.Remove(existing);

        Application.Current.Resources.MergedDictionaries.Add(dict);
    }
}
```

```csharp
// Usage in ViewModel
[RelayCommand]
private void SwitchToDarkTheme()
    => AppThemeManager.ApplyTheme("Dark");
```

## Pattern 3: DataTemplate Theming

```xml
<DataTemplate x:Key="ProductItemTemplate" DataType="{x:Type vm:ProductItemViewModel}">
    <Border Padding="12,8"
            Background="{StaticResource AppColor.BackgroundSecondary}"
            BorderBrush="{StaticResource AppColor.BorderThin}"
            BorderThickness="0,0,0,1">
        <StackPanel>
            <local:AppLabel Text="{Binding Title}"
                            Style="{StaticResource AppTypography.LabelSection}"/>
            <local:AppLabel Text="{Binding Price}"
                            Style="{StaticResource AppTypography.BodyCaption}"
                            Foreground="{StaticResource AppColor.TextBrand}"/>
        </StackPanel>
    </Border>
</DataTemplate>
```

## ResourceDictionary Keys Reference

### Typography (AppLabel)

```xml
<!-- Headers -->
<local:AppLabel Style="{StaticResource AppTypography.HeaderPage}"/>      <!-- SemiBold 20px -->
<local:AppLabel Style="{StaticResource AppTypography.HeaderSection}"/>   <!-- SemiBold 16px -->

<!-- Body -->
<local:AppLabel Style="{StaticResource AppTypography.BodySection}"/>     <!-- Regular 14px -->
<local:AppLabel Style="{StaticResource AppTypography.BodyCaption}"/>     <!-- Regular 12px -->

<!-- Labels -->
<local:AppLabel Style="{StaticResource AppTypography.LabelPage}"/>       <!-- Bold 16px -->
<local:AppLabel Style="{StaticResource AppTypography.LabelSection}"/>    <!-- Bold 14px -->
```

### Buttons (AppButton)

```xml
<!-- Button variants -->
<local:AppButton Style="{StaticResource AppButton.Primary.Medium}"/>
<local:AppButton Style="{StaticResource AppButton.Secondary.Medium}"/>
<local:AppButton Style="{StaticResource AppButton.Tertiary.Small}"/>
<local:AppButton Style="{StaticResource AppButton.Destructive.Medium}"/>

<!-- Icon buttons -->
<local:AppButton Style="{StaticResource AppButton.Icon.Medium}"/>
```

### Colors (Brush Resources)

```xml
<!-- Backgrounds -->
Background="{StaticResource AppColor.BackgroundPrimary}"
Background="{StaticResource AppColor.BackgroundSecondary}"
Background="{StaticResource AppColor.BackgroundBrand}"
Background="{StaticResource AppColor.BackgroundWarningLight}"

<!-- Text Colors -->
Foreground="{StaticResource AppColor.TextPrimary}"
Foreground="{StaticResource AppColor.TextSecondary}"
Foreground="{StaticResource AppColor.TextDisabled}"
Foreground="{StaticResource AppColor.TextError}"
Foreground="{StaticResource AppColor.TextBrand}"

<!-- Borders -->
BorderBrush="{StaticResource AppColor.BorderThin}"
BorderBrush="{StaticResource AppColor.BorderRegular}"
```

## Merging ResourceDictionaries

```xml
<!-- App.xaml — merge all theme resources -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="/App;component/Themes/DefaultTheme.xaml"/>
            <ResourceDictionary Source="/App;component/Shared/AppConverters.xaml"/>
            <ResourceDictionary Source="/App;component/Shared/AppStyles.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

## Anti-Patterns

```xml
<!-- ❌ Hardcoded colors -->
<TextBlock Foreground="Black" Background="White"/>
<TextBlock Foreground="#FF333333"/>

<!-- ❌ Raw WPF controls without AppDesignSystem styles -->
<TextBlock FontSize="16" FontWeight="SemiBold"/>
<Button Content="Action" Background="Blue"/>

<!-- ✅ Always use AppDesignSystem styles -->
<local:AppLabel Style="{StaticResource AppTypography.HeaderSection}"/>
<local:AppButton Content="Action" Style="{StaticResource AppButton.Primary.Medium}"/>
```

```csharp
// ❌ Hardcoded colors in code-behind
label.Foreground = Brushes.Black;
label.Background = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));

// ✅ Use ResourceDictionary keys
label.SetResourceReference(ForegroundProperty, "AppColor.TextPrimary");
// or set via XAML binding / style
```

## Theme Types

```swift
// Available theme loaders
private let theme = AppThemeManager.defaultTheme  // Default App theme
private let theme = AppThemeManager.jobTheme      // JOB module
private let theme = AppThemeManager.ptyTheme      // Property module
```

## Pattern 1: Static Theme (Recommended for Most Cases)

```swift
import WPF
import AppCommon
import AppDesignSystem
import XAML layout

class MyViewController: UserControl {
    private let theme = AppThemeManager.defaultTheme

    private func setupUI() {
        titleLabel.setStyle(DS.TypoToken.Header.Section(color: theme.text.textPrimary.color))
        view.backgroundColor = theme.background.backgroundPrimary.color
    }
}
```

## Pattern 2: Dynamic Theme Subscription

```csharp
// In code-behind or ViewModel
public partial class MyView : UserControl
{
    private readonly IDisposable _themeSubscription;

    public MyView()
    {
        InitializeComponent();
        _themeSubscription = AppThemeManager.ThemeChanged
            .Subscribe(theme => ApplyTheme(theme));
        ApplyTheme(AppThemeManager.Current);
    }

    private void ApplyTheme(AppTheme theme)
    {
        // Update dynamic resource references if needed
        // Most styles resolve automatically via ResourceDictionary
    }

    protected override void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _themeSubscription?.Dispose();
    }
}
```

## Pattern 3: ItemViewModel Theming

```csharp
public partial class MyItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _titleStyle = AppTypography.LabelSection;

    [ObservableProperty]
    private Brush _titleColor;

    public MyItemViewModel()
    {
        ApplyTheme(AppThemeManager.Current);
        AppThemeManager.ThemeChanged
            .Subscribe(t => ApplyTheme(t))
            .DisposeWith(_disposables);
    }

    private void ApplyTheme(AppTheme theme)
    {
        TitleColor = theme.Text.TextPrimary;
    }
}
```

## Component Theming Reference

### Typography (AppLabel)
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

### Buttons (AppButton)
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
class MyViewController: UserControl, CTNavigationBarVeritcalizable {
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
| Default / Generic | `AppThemeManager.defaultTheme` | `.default` |
| CTJOB / Job | `AppThemeManager.jobTheme` | `.job` |
| CTPTY / Property | `AppThemeManager.ptyTheme` | `.pty` |

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

// ✅ Always use AppThemeManager
private let theme = AppThemeManager.defaultTheme

// ✅ Animate theme changes
func changeTheme(_ theme: CMTheme) {
    UIView.animate(withDuration: 0.3) {
        self.view.backgroundColor = theme.background.backgroundPrimary.color
    }
}
```
