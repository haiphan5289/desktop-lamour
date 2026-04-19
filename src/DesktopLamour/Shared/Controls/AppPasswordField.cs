// AppPasswordField.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Windows;
using System.Windows.Controls;

namespace DesktopLamour.Shared.Controls;

/// <summary>
/// Design system password input with a bindable <see cref="BoundPassword"/> property.
/// WPF PasswordBox.Password is not a DependencyProperty — use BoundPassword for MVVM binding.
/// </summary>
public class AppPasswordField : PasswordBox
{
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.Register(
            nameof(BoundPassword),
            typeof(string),
            typeof(AppPasswordField),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnBoundPasswordChanged));

    private bool _isUpdating;

    static AppPasswordField()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AppPasswordField),
            new FrameworkPropertyMetadata(typeof(AppPasswordField)));
    }

    public AppPasswordField()
    {
        PasswordChanged += OnPasswordChanged;
    }

    public string BoundPassword
    {
        get => (string)GetValue(BoundPasswordProperty);
        set => SetValue(BoundPasswordProperty, value);
    }

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AppPasswordField field && !field._isUpdating)
        {
            field.Password = e.NewValue as string ?? string.Empty;
        }
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        _isUpdating = true;
        BoundPassword = Password;
        _isUpdating = false;
    }
}
