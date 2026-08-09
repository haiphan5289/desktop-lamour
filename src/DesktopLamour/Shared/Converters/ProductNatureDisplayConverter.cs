// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Windows.Data;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;

namespace DesktopLamour.Shared.Converters;

[ValueConversion(typeof(ProductNature), typeof(string))]
public class ProductNatureDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => value is ProductNature nature ? nature switch
        {
            ProductNature.VatTuHangHoa => "Vật tư hàng hóa",
            ProductNature.DichVu       => "Dịch vụ",
            _                          => nature.ToString(),
        } : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
