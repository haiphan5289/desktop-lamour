// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text;

namespace DesktopLamour.Shared.Helpers;

// Đọc số tiền VNĐ thành chữ cho các chứng từ in cần dòng "Tổng số tiền (Viết bằng chữ)" — mẫu
// PHIẾU NHẬP KHO (01-VT theo Thông tư 200) và các mẫu tương tự. Theo đúng quy tắc đọc số tiếng
// Việt chuẩn kế toán: "mười"/"mươi", "mốt"/"tư"/"lăm" ở hàng đơn vị, "linh" khi hàng chục = 0
// nhưng có hàng trăm/nhóm phía trước.
public static class VietnameseNumberToWordsHelper
{
    private static readonly string[] Ones =
        { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };

    // Đơn vị nhóm 3 chữ số, từ thấp đến cao — đủ dùng tới "tỷ tỷ" (10^18), vượt xa nhu cầu thực tế
    // của 1 phiếu nhập kho mỹ phẩm.
    private static readonly string[] GroupUnits =
        { "", "nghìn", "triệu", "tỷ", "nghìn tỷ", "triệu tỷ", "tỷ tỷ" };

    public static string ToWords(decimal amount)
    {
        var value = (long)Math.Round(amount, MidpointRounding.AwayFromZero);
        if (value == 0) return "Không đồng.";

        var negative = value < 0;
        value = Math.Abs(value);

        var groups = new List<long>();
        while (value > 0)
        {
            groups.Add(value % 1000);
            value /= 1000;
        }

        var parts = new List<string>();
        for (var i = groups.Count - 1; i >= 0; i--)
        {
            if (groups[i] == 0) continue;

            // padHundred: nhóm không phải nhóm cao nhất (i < groups.Count - 1) luôn đọc đủ "không
            // trăm..." nếu < 100, để không bị hiểu nhầm khi ghép nối các nhóm (VD 1.005 = "một
            // nghìn không trăm linh năm", không phải "một nghìn năm").
            var padHundred = i < groups.Count - 1;
            var groupWords = ReadThreeDigits(groups[i], padHundred);
            var unit = i < GroupUnits.Length ? GroupUnits[i] : "";
            parts.Add(unit.Length > 0 ? $"{groupWords} {unit}" : groupWords);
        }

        var sb = new StringBuilder(string.Join(" ", parts).Trim());
        sb[0] = char.ToUpper(sb[0]);

        return (negative ? "Âm " : "") + sb + " đồng.";
    }

    private static string ReadThreeDigits(long n, bool padHundred)
    {
        var hundreds = n / 100;
        var tens     = (n / 10) % 10;
        var units    = n % 10;
        var words    = new List<string>();

        if (hundreds > 0 || padHundred)
            words.Add($"{Ones[hundreds]} trăm");

        if (tens == 0)
        {
            if (units > 0 && (hundreds > 0 || padHundred))
                words.Add("linh " + (units == 5 ? "lăm" : Ones[units]));
            else if (units > 0)
                words.Add(Ones[units]);
        }
        else if (tens == 1)
        {
            words.Add("mười");
            if (units == 5) words.Add("lăm");
            else if (units == 1) words.Add("mốt");
            else if (units > 0) words.Add(Ones[units]);
        }
        else
        {
            words.Add($"{Ones[tens]} mươi");
            if (units == 1) words.Add("mốt");
            else if (units == 5) words.Add("lăm");
            else if (units == 4) words.Add("tư");
            else if (units > 0) words.Add(Ones[units]);
        }

        return string.Join(" ", words);
    }
}
