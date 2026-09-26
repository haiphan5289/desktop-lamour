// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Accounting.Domain.Models;

// 2026-09-26: 1 dòng trên tab "1. Hạch toán" của popup "Phiếu thu tiền mặt khách hàng hàng loạt" —
// GỘP các BulkReceiptLineItem có cùng CustomerId thành 1 dòng, Số tiền = tổng (khớp ảnh mẫu MISA:
// 1 khách hàng có nhiều hóa đơn được chọn cùng lúc chỉ hiện 1 dòng kế toán). Tab "2. Chứng từ" vẫn
// giữ nguyên 1 dòng/1 hóa đơn (bind thẳng BulkReceiptLineItem, không qua model này) để còn sửa Số
// thu từng hóa đơn riêng — dòng gộp ở đây chỉ để HIỂN THỊ, không sửa được, và KHÔNG phải dữ liệu gửi
// lên BE (BuildEntries vẫn dùng Lines gốc, giữ đúng 1 ReceiptEntry/1 SalesOrder — xem
// BulkCustomerReceiptViewModel để biết lý do không gộp được ở tầng dữ liệu).
public sealed class BulkReceiptGroupedLine
{
    // Diễn giải luôn là chữ cố định "Thu tiền khách hàng" (khớp MISA) — không kèm số chứng từ như
    // dòng lẻ, vì 1 dòng gộp có thể đại diện cho NHIỀU hóa đơn khác nhau.
    public string Description => "Thu tiền khách hàng";

    public string  CustomerCode         { get; init; } = "";
    public string  CustomerName         { get; init; } = "";
    public decimal Amount               { get; init; }
    public string  DebitAccountDisplay  { get; init; } = "";
    public string  CreditAccountDisplay { get; init; } = "";

    public static List<BulkReceiptGroupedLine> FromLines(IEnumerable<BulkReceiptLineItem> lines) =>
        lines
            .GroupBy(l => l.CustomerId)
            .Select(g => new BulkReceiptGroupedLine
            {
                CustomerCode         = g.First().CustomerCode,
                CustomerName         = g.First().CustomerName,
                Amount               = g.Sum(l => l.Amount),
                DebitAccountDisplay  = g.First().DebitAccountDisplay,
                CreditAccountDisplay = g.First().CreditAccountDisplay,
            })
            .ToList();
}
