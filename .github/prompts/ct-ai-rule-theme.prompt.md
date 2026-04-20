---
description: "Design system usage guide for Desktop Lamour — AppStyles, AppTypography, AppColor tokens."
mode: "agent"
---

# Design System Guide — Desktop Lamour

All WPF controls MUST use design system components and resource keys. Never use raw `TextBlock`, `Button`, `TextBox`, or inline styling.

## Component Mapping

| Raw WPF (FORBIDDEN) | Design System (USE THIS) |
|---|---|
| `TextBlock` | `controls:AppLabel` |
| `Button` | `controls:AppButton` |
| `TextBox` | `controls:AppTextField` |
| `PasswordBox` | `controls:AppPasswordField` |

## AppLabel — Typography Keys

```xml
xmlns:controls="clr-namespace:DesktopLamour.Shared.Controls"

<!-- Display -->
<controls:AppLabel Text="Page Title"    Style="{StaticResource AppTypography.DisplayPage}"/>
<controls:AppLabel Text="Section"       Style="{StaticResource AppTypography.DisplaySection}"/>

<!-- Header -->
<controls:AppLabel Text="Page Header"   Style="{StaticResource AppTypography.HeaderPage}"/>
<controls:AppLabel Text="Section Header" Style="{StaticResource AppTypography.HeaderSection}"/>

<!-- Label -->
<controls:AppLabel Text="Field Label"   Style="{StaticResource AppTypography.LabelPage}"/>
<controls:AppLabel Text="Field Label"   Style="{StaticResource AppTypography.LabelSection}"/>
<controls:AppLabel Text="Caption"       Style="{StaticResource AppTypography.LabelCaption}"/>

<!-- Body -->
<controls:AppLabel Text="Body text"     Style="{StaticResource AppTypography.BodySection}"/>
<controls:AppLabel Text="Caption text"  Style="{StaticResource AppTypography.BodyCaption}"/>
<controls:AppLabel Text="Note"          Style="{StaticResource AppTypography.NoteSection}"/>
```

## AppButton — Style Keys

```xml
<!-- Primary -->
<controls:AppButton Content="Save"   Style="{StaticResource AppButton.Primary.Large}"   Command="{Binding SaveCommand}"/>
<controls:AppButton Content="Save"   Style="{StaticResource AppButton.Primary.Medium}"  Command="{Binding SaveCommand}"/>
<controls:AppButton Content="Save"   Style="{StaticResource AppButton.Primary.Small}"   Command="{Binding SaveCommand}"/>

<!-- Secondary (outlined) -->
<controls:AppButton Content="Cancel" Style="{StaticResource AppButton.Secondary.Medium}" Command="{Binding CancelCommand}"/>

<!-- Tertiary (text-only) -->
<controls:AppButton Content="← Back" Style="{StaticResource AppButton.Tertiary.Medium}"  Command="{Binding GoBackCommand}"/>

<!-- Destructive -->
<controls:AppButton Content="Delete" Style="{StaticResource AppButton.Destructive.Medium}" Command="{Binding DeleteCommand}"/>
```

## AppTextField

```xml
<controls:AppTextField Text="{Binding PhoneNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                       Placeholder="e.g. 0912345678"/>

<!-- Error state -->
<controls:AppTextField Style="{StaticResource AppTextField.Error}"
                       Text="{Binding Value, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>
```

## AppPasswordField

```xml
<controls:AppPasswordField BoundPassword="{Binding Password, Mode=TwoWay}"/>
```

## Color Tokens (use only via StaticResource)

```xml
<!-- Text -->
Foreground="{StaticResource AppColor.TextPrimary}"
Foreground="{StaticResource AppColor.TextSecondary}"
Foreground="{StaticResource AppColor.TextDisabled}"
Foreground="{StaticResource AppColor.TextError}"
Foreground="{StaticResource AppColor.TextBrand}"
Foreground="{StaticResource AppColor.TextInverted}"
Foreground="{StaticResource AppColor.TextSuccess}"

<!-- Background -->
Background="{StaticResource AppColor.BackgroundPrimary}"
Background="{StaticResource AppColor.BackgroundSecondary}"
Background="{StaticResource AppColor.BackgroundErrorLight}"
Background="{StaticResource AppColor.BackgroundOverlay}"
```

## Spacing Tokens

```xml
Margin="{StaticResource AppSpacing.XSmall}"   <!-- 4 -->
Margin="{StaticResource AppSpacing.Small}"    <!-- 8 -->
Margin="{StaticResource AppSpacing.Medium}"   <!-- 12 -->
Margin="{StaticResource AppSpacing.Large}"    <!-- 16 -->
Margin="{StaticResource AppSpacing.XLarge}"   <!-- 20 -->
Margin="{StaticResource AppSpacing.XXLarge}"  <!-- 24 -->
```

## Loading Overlay Pattern

```xml
<Grid>
    <Grid Panel.ZIndex="100"
          Background="{StaticResource AppColor.BackgroundOverlay}"
          Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}">
        <ProgressBar IsIndeterminate="True" Width="48" Height="48"
                     HorizontalAlignment="Center" VerticalAlignment="Center"/>
    </Grid>
    <!-- main content below -->
</Grid>
```

## Error Message Pattern

```xml
<controls:AppLabel Text="{Binding ErrorMessage}"
                   Style="{StaticResource AppTypography.BodyCaption}"
                   Foreground="{StaticResource AppColor.TextError}"
                   Visibility="{Binding ErrorMessage, Converter={StaticResource StringToVisibilityConverter}}"/>
```

## Adding a New Component

1. Create `Shared/Controls/App[Name].cs`
2. Create `Shared/Styles/App[Name]Styles.xaml`
3. Add one line to `Shared/ComponentLibrary.xaml`
