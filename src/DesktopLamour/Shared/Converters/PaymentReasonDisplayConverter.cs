// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Windows.Data;

namespace DesktopLamour.Shared.Converters;

[ValueConversion(typeof(string), typeof(string))]
public class PaymentReasonDisplayConverter : IValueConverter
{
    // Nhãn tiếng Việt của enum PaymentReason (BE) — dùng chung cho lưới Quỹ, Xuất khẩu, ô Lý do chi
    // và bản in phiếu chi. Giá trị lạ giữ nguyên chuỗi gốc.
    public static string Label(string? reason) => reason switch
    {
        "ThuKhac"     => "Thu khác",
        "ThuTienHang" => "Thu tiền hàng",
        "ThuCongNo"   => "Thu công nợ",
        "ThuKhachHangHangLoat" => "Phiếu thu tiền mặt khách hàng hàng loạt",
        "RutTienGuiVeNopQuy" => "Rút tiền gửi về nộp quỹ",
        "ThuHoanThueGTGT"    => "Thu hoàn thuế GTGT",
        "ThuHoanUng"         => "Thu hoàn ứng",
        "ChiKhac"     => "Chi khác",
        "ChiMuaHang"  => "Chi mua hàng",
        "ChiTraNo"    => "Chi trả nợ",
        "ChiLuong"    => "Chi lương",
        "TamUngNhanVien"  => "Tạm ứng cho nhân viên",
        "GuiTienNganHang" => "Gửi tiền vào ngân hàng",
        "ThueTNDNTamTinh" => "Thuế TNDN tạm tính",
        _             => reason ?? "",
    };

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => value is string reason ? Label(reason) : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
