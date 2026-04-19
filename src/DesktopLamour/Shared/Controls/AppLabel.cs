// AppLabel.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Windows;
using System.Windows.Controls;

namespace DesktopLamour.Shared.Controls;

/// <summary>
/// Design system label. Always use instead of raw TextBlock.
/// Bind text via <see cref="Text"/> and style via AppTypography.* resources.
/// </summary>
public class AppLabel : Label
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(AppLabel),
            new FrameworkPropertyMetadata(string.Empty));

    static AppLabel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AppLabel),
            new FrameworkPropertyMetadata(typeof(AppLabel)));
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        // Content follows Text binding
        SetBinding(ContentProperty, new System.Windows.Data.Binding(nameof(Text)) { Source = this });
    }
}
