// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DesktopLamour.Shared.Converters;

// Nút mũi tên mở popup lọc value-list (ValueListFilterButton): tô cam khi cột đang được lọc bớt giá
// trị (ValueListColumnFilter.IsActive=true), xám bình thường khi chưa lọc (đang hiện "Tất cả"). Hex
// phải khớp đúng AppColor.TextBrand/AppColor.TextSecondary (Themes/DefaultTheme.xaml) — không bịa
// sắc độ gần giống, converter C# không gọi được StaticResource nên copy nguyên giá trị đã xác minh.
[ValueConversion(typeof(bool), typeof(Brush))]
public class FilterActiveBrushConverter : IValueConverter
{
    private static readonly Brush Active   = new SolidColorBrush(Color.FromRgb(0xF2, 0x8A, 0x00)); // AppColor.TextBrand
    private static readonly Brush Inactive = new SolidColorBrush(Color.FromRgb(0x6B, 0x6B, 0x6B)); // AppColor.TextSecondary

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Active : Inactive;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
