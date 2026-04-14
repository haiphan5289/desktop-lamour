---
description: "Best practices for using the WPF AppDesignSystem theme system (ResourceDictionary, AppThemeManager, AppColor, AppTypography)"
mode: "agent"
---

# WPF Theme Best Practices

Guidelines for using the theme system in WPF following AppDesignSystem token patterns.

## Core Theme Architecture

```
AppDesignSystem/
├── Resources/
│   ├── AppColors.xaml          — Color tokens (Light/Dark)
│   ├── AppTypography.xaml      — Typography styles
│   ├── AppButtons.xaml         — Button styles
│   ├── AppInputs.xaml          — Input styles
│   └── AppDesignSystem.xaml    — Master merged dictionary
AppCommon/
└── Theme/
    ├── AppThemeManager.cs      — Static theme loader
    └── IThemeChangeable.cs     — Dynamic theme protocol
```

## Pattern 1: Static Theme Access (Recommended)

```xml
<!-- Reference AppDesignSystem in UserControl -->
<UserControl.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="/App;component/Shared/AppDesignSystem.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</UserControl.Resources>

<StackPanel Margin="16">
    <!-- Typography tokens -->
    <local:AppLabel Text="Page Title"
                    Style="{StaticResource AppTypography.HeaderPage}"/>
    <local:AppLabel Text="Section header"
                    Style="{StaticResource AppTypography.HeaderSection}"/>
    <local:AppLabel Text="Body text"
                    Style="{StaticResource AppTypography.BodySection}"/>
    <local:AppLabel Text="Caption"
                    Style="{StaticResource AppTypography.BodyCaption}"
                    Foreground="{StaticResource AppColor.TextSecondary}"/>
    <local:AppLabel Text="Error message"
                    Style="{StaticResource AppTypography.BodyCaption}"
                    Foreground="{StaticResource AppColor.TextError}"/>

    <!-- Buttons -->
    <local:AppButton Content="Primary Action"
                     Style="{StaticResource AppButton.Primary.Medium}"/>
    <local:AppButton Content="Secondary"
                     Style="{StaticResource AppButton.Secondary.Medium}"/>
</StackPanel>
```

## Pattern 2: Runtime Theme Switching

```csharp
// AppThemeManager.cs
public static class AppThemeManager
{
    public static AppTheme Current { get; private set; } = AppTheme.Default;

    public static event Action<AppTheme>? ThemeChanged;

    public static void Apply(AppTheme theme)
    {
        Current = theme;

        var dict = Application.Current.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source?.ToString().Contains("AppColors") == true);
        if (dict != null)
        {
            Application.Current.Resources.MergedDictionaries.Remove(dict);
        }

        var newDict = new ResourceDictionary
        {
            Source = new Uri($"/App;component/Shared/AppColors.{theme}.xaml", UriKind.Relative)
        };
        Application.Current.Resources.MergedDictionaries.Add(newDict);

        ThemeChanged?.Invoke(theme);
    }
}
```

```csharp
// In MainWindow.xaml.cs or App.xaml.cs
AppThemeManager.Apply(AppTheme.Dark);
```

## Pattern 3: Theme-Aware ViewModel

```csharp
public partial class MyViewModel : ObservableObject, IDisposable
{
    [ObservableProperty]
    private Brush _titleColor = Brushes.Black;

    [ObservableProperty]
    private Brush _backgroundColor = Brushes.White;

    private readonly IDisposable _themeSubscription;

    public MyViewModel()
    {
        ApplyTheme(AppThemeManager.Current);
        _themeSubscription = AppThemeManager.ThemeChanged
            .Subscribe(t => ApplyTheme(t));
    }

    private void ApplyTheme(AppTheme theme)
    {
        // Resolve from merged ResourceDictionary
        TitleColor = (Brush)Application.Current.Resources["AppColor.TextPrimary"];
        BackgroundColor = (Brush)Application.Current.Resources["AppColor.BackgroundPrimary"];
    }

    public void Dispose()
    {
        _themeSubscription?.Dispose();
    }
}
```

## Pattern 4: Module-Specific Theme Overrides

```xml
<!-- PTY module coloring override -->
<UserControl.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="/App;component/Shared/AppDesignSystem.xaml"/>
            <ResourceDictionary Source="/App;component/Modules/PTY/PTYThemeOverrides.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</UserControl.Resources>
```

## Typography Token Reference

| Style Key | Font Weight | Size | Usage |
|---|---|---|---|
| `AppTypography.DisplayPage` | SemiBold | 24px | Page display headings |
| `AppTypography.HeaderPage` | SemiBold | 20px | Page headers |
| `AppTypography.HeaderSection` | SemiBold | 16px | Section headers |
| `AppTypography.LabelPage` | Bold | 16px | Labels, pills |
| `AppTypography.LabelSection` | SemiBold | 14px | UI labels |
| `AppTypography.BodySection` | Regular | 14px | Body copy |
| `AppTypography.BodyCaption` | Regular | 12px | Captions, hints |
| `AppTypography.LabelCaption` | Medium | 12px | Small labels |

## Color Token Reference

```xml
<!-- Background -->
<SolidColorBrush x:Key="AppColor.BackgroundPrimary" .../>
<SolidColorBrush x:Key="AppColor.BackgroundSecondary" .../>
<SolidColorBrush x:Key="AppColor.BackgroundBrand" .../>

<!-- Text -->
<SolidColorBrush x:Key="AppColor.TextPrimary" .../>
<SolidColorBrush x:Key="AppColor.TextSecondary" .../>
<SolidColorBrush x:Key="AppColor.TextDisabled" .../>
<SolidColorBrush x:Key="AppColor.TextError" .../>
<SolidColorBrush x:Key="AppColor.TextBrand" .../>

<!-- Interactive -->
<SolidColorBrush x:Key="AppColor.BrandPrimary" .../>
<SolidColorBrush x:Key="AppColor.BorderDefault" .../>
<SolidColorBrush x:Key="AppColor.BorderFocus" .../>
```

## Button Style Reference

```xml
<!-- Primary buttons -->
<Style x:Key="AppButton.Primary.Large" .../>
<Style x:Key="AppButton.Primary.Medium" .../>
<Style x:Key="AppButton.Primary.Small" .../>

<!-- Secondary buttons -->
<Style x:Key="AppButton.Secondary.Large" .../>
<Style x:Key="AppButton.Secondary.Medium" .../>
<Style x:Key="AppButton.Secondary.Small" .../>

<!-- Ghost / text buttons -->
<Style x:Key="AppButton.Ghost.Medium" .../>
<Style x:Key="AppButton.Danger.Medium" .../>
```

## Anti-Patterns to Avoid

```xml
<!-- ❌ Never hardcode colors -->
<TextBlock Foreground="Black" FontSize="14" FontWeight="SemiBold"/>

<!-- ❌ Never use raw UILabel/UIColor equivalents -->
<TextBlock Text="..."/> <!-- use AppLabel -->
<Button Content="..."/> <!-- use AppButton -->

<!-- ✅ Always use AppDesignSystem tokens -->
<local:AppLabel Text="..."
                Style="{StaticResource AppTypography.BodySection}"
                Foreground="{StaticResource AppColor.TextPrimary}"/>
```

## Navigation Bar Theming

```csharp
// In MainWindow.xaml.cs
private void ApplyNavigationTheme(AppTheme theme)
{
    var navBar = FindName("NavigationBar") as FrameworkElement;
    if (navBar != null)
    {
        navBar.SetResourceReference(BackgroundProperty, "AppColor.BackgroundBrand");
    }
}
```

## Rules

- **ALWAYS** use `AppDesignSystem.xaml` merged dictionary in every UserControl
- **ALWAYS** use `{StaticResource AppTypography.*}` for text styles
- **ALWAYS** use `{StaticResource AppColor.*}` for colors
- **NEVER** hardcode `Foreground`, `Background`, `FontSize`, or `FontWeight`
- **NEVER** use raw `TextBlock`, `Button` — use `AppLabel`, `AppButton`
- For module-specific overrides, add a second `ResourceDictionary` merge after the main one
- Logger: use `ILogger<T>` — never `Console.WriteLine`
