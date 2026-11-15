// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows;
using System.Windows.Controls;
using DesktopLamour.Shared.Models;

namespace DesktopLamour.Shared.Controls;

public partial class ValueListFilterButton : UserControl
{
    public static readonly DependencyProperty FilterProperty =
        DependencyProperty.Register(
            nameof(Filter),
            typeof(ValueListColumnFilter),
            typeof(ValueListFilterButton),
            new PropertyMetadata(null));

    public ValueListColumnFilter? Filter
    {
        get => (ValueListColumnFilter?)GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    public ValueListFilterButton() => InitializeComponent();
}
