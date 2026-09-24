// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DesktopLamour.Shared.Converters;

// 2026-09-24: bề rộng (px) 1 ô trong Expander.Header nhóm của SalesOrderReportView = tổng
// ActualWidth các cột DataGrid mà ô đó "phủ", chỉ tính cột đang Visible. Input là các cặp
// (ActualWidth, Visibility) của từng cột; ConverterParameter (tùy chọn) = số px cần trừ đi — dùng
// cho ô nhãn nhóm vì nó nằm SAU nút −/+ (glyph chiếm sẵn một đoạn bên trái). Bám theo ActualWidth
// thật nên vẫn thẳng hàng khi user kéo đổi độ rộng cột, thay cho bảng width cứng trước đây
// (GroupHeaderLabelWidthConverter) vốn quên trừ phần glyph nên mọi cột số bị lệch phải ~32px.
public class ColumnSpanWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var width = 0d;
        for (var i = 0; i + 1 < values.Length; i += 2)
        {
            if (values[i] is double w && values[i + 1] is Visibility.Visible && !double.IsNaN(w))
                width += w;
        }

        if (parameter is not null
            && double.TryParse(parameter.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var offset))
            width -= offset;

        return Math.Max(0, width);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
