// AppTextField.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Windows;
using System.Windows.Controls;

namespace DesktopLamour.Shared.Controls;

/// <summary>
/// Design system text input. Always use instead of raw TextBox.
/// Bind via <see cref="TextBox.Text"/> with Mode=TwoWay, UpdateSourceTrigger=PropertyChanged.
/// </summary>
public class AppTextField : TextBox
{
    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(AppTextField),
            new PropertyMetadata(string.Empty));

    static AppTextField()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AppTextField),
            new FrameworkPropertyMetadata(typeof(AppTextField)));
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }
}
