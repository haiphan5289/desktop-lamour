// AnyNotNullToVisibilityConverter.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DesktopLamour.Shared.Converters;

/// <summary>
/// MultiBinding converter: Visible nếu có ÍT NHẤT MỘT giá trị non-null trong danh sách,
/// ngược lại Collapsed. Dùng cho vạch ngăn nhóm trong <c>DocumentToolbar</c> — chỉ hiện
/// vạch khi nhóm nút phía sau nó có ít nhất 1 nút.
/// </summary>
public class AnyNotNullToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => values is not null
           && values.Any(v => v is not null && v != DependencyProperty.UnsetValue)
            ? Visibility.Visible
            : Visibility.Collapsed;

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
