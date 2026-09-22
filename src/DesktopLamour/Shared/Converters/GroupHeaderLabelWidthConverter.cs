// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Windows.Data;

namespace DesktopLamour.Shared.Converters;

// 2026-09-22: tính bề rộng (px) của ô nhãn nhóm trong Expander.Header của SalesOrderReportView —
// PHẢI khớp đúng tổng bề rộng các cột văn bản ĐANG HIỂN THỊ thật ở đầu DataGrid (Mã NV/Tên NV/Tên
// khách hàng giữa/Mã hàng/Tên hàng/ĐVT) để đường viền phân cách + 7 cột số liệu bên phải căn đúng
// với dòng dữ liệu bên dưới. Mã hàng/Tên hàng (ProductCode/ProductName) không có input vì 2 cột đó
// không bao giờ bị ẩn (UpdateColumnVisibility trong SalesOrderReportView.xaml.cs chỉ toggle
// Outer/Middle/Unit/Profit/Address/CustomerGroup, không toggle ProductCode/ProductName).
public class GroupHeaderLabelWidthConverter : IMultiValueConverter
{
    private const double ProductCodeWidth = 95;
    private const double ProductNameWidth = 220;
    private const double OuterCodeWidth   = 95;
    private const double OuterNameWidth   = 180;
    private const double MiddleNameWidth  = 180;
    private const double UnitWidth        = 65;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var outerVisible  = values.Length > 0 && values[0] is true;
        var middleVisible = values.Length > 1 && values[1] is true;
        var unitVisible   = values.Length > 2 && values[2] is true;

        var width = ProductCodeWidth + ProductNameWidth;
        if (outerVisible)  width += OuterCodeWidth + OuterNameWidth;
        if (middleVisible) width += MiddleNameWidth;
        if (unitVisible)   width += UnitWidth;
        return width;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
