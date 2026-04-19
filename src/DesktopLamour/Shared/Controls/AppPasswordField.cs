// AppPasswordField.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Windows;
using System.Windows.Controls;

namespace DesktopLamour.Shared.Controls;

/// <summary>
/// Design system password input with a bindable <see cref="BoundPassword"/> property.
/// Extends Control (not PasswordBox — sealed in .NET 8) and expects a PasswordBox named
/// PART_PasswordBox in its ControlTemplate (defined in AppStyles.xaml).
/// </summary>
[TemplatePart(Name = PartPasswordBox, Type = typeof(PasswordBox))]
public class AppPasswordField : Control
{
    public const string PartPasswordBox = "PART_PasswordBox";

    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.Register(
            nameof(BoundPassword),
            typeof(string),
            typeof(AppPasswordField),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnBoundPasswordChanged));

    private PasswordBox? _passwordBox;
    private bool _isUpdating;

    static AppPasswordField()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AppPasswordField),
            new FrameworkPropertyMetadata(typeof(AppPasswordField)));
    }

    public string BoundPassword
    {
        get => (string)GetValue(BoundPasswordProperty);
        set => SetValue(BoundPasswordProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_passwordBox != null)
            _passwordBox.PasswordChanged -= OnPasswordChanged;

        _passwordBox = GetTemplateChild(PartPasswordBox) as PasswordBox;

        if (_passwordBox != null)
            _passwordBox.PasswordChanged += OnPasswordChanged;
    }

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AppPasswordField field && !field._isUpdating && field._passwordBox != null)
            field._passwordBox.Password = e.NewValue as string ?? string.Empty;
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        _isUpdating = true;
        BoundPassword = _passwordBox?.Password ?? string.Empty;
        _isUpdating = false;
    }
}
