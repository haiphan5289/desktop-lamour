// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;

namespace DesktopLamour.Shared.Helpers;

// 2026-09-26: 1 chuẩn hiển thị số tiền DUY NHẤT cho toàn app, khớp MISA — dấu chấm phân cách hàng
// nghìn (vi-VN, "1.600.000") và số âm kiểu kế toán trong ngoặc ("(1.458.000)"). Trước đây binding
// StringFormat=N0 trong XAML chạy theo culture mặc định của WPF (en-US → "1,600,000", "-1,458,000")
// trong khi vài chỗ C# lại format vi-VN → cùng 1 màn hình có 2 kiểu số. Mọi chỗ HIỂN THỊ số tiền
// (converter XAML, dòng tổng nhóm, bản in báo cáo) gọi qua đây.
public static class MoneyFormat
{
    public static readonly CultureInfo ViVn = CultureInfo.GetCultureInfo("vi-VN");

    public static string Format(decimal value, string format = "N0")
        => value < 0m
            ? $"({Math.Abs(value).ToString(format, ViVn)})"
            : value.ToString(format, ViVn);

    // Chuỗi đã format có phải số âm kiểu kế toán "(1.458.000)" hay "-1.458.000" không — dùng để tô đỏ
    // mà không cần biết ô đó bind vào property nào (xem NegativeAmountHighlight).
    public static bool IsNegativeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        var t = text.Trim();
        if (t.Length >= 3 && t[0] == '(' && t[^1] == ')' && char.IsDigit(t[1])) return true;
        return t.Length >= 2 && t[0] == '-' && char.IsDigit(t[1]);
    }
}
