// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

public enum SummaryDimension { Product, Customer, Employee }

public class ReportDisplayRow
{
    public bool      IsGroupHeader  { get; init; }
    public string?   GroupKey       { get; set; }
    public string?   GroupLabel     { get; set; }
    public string    ProductCode    { get; init; } = "";
    public string    ProductName    { get; init; } = "";
    public string    Unit           { get; init; } = "";
    public int       QuantitySold   { get; init; }
    public decimal   SalesAmount    { get; init; }
    public decimal   DiscountAmount { get; init; }
    public int       ReturnQuantity { get; init; }
    public decimal   ReturnValue    { get; init; }
    public decimal   DiscountValue  { get; init; }
    public decimal   NetRevenue     { get; init; }
    public decimal   CostAmount     { get; init; }
    public decimal   GrossProfit    { get; init; }
    public decimal   GrossProfitRate { get; init; }
    public string    CustomerGroupName { get; init; } = "";

    // Mã/Tên của dimension NGOÀI cho report 2 chiều — khớp bảng phẳng kiểu MISA: mọi dòng hiện
    // đủ cả 2 dimension làm cột thật (vd. Mã NV/Tên NV NGOÀI cột Mã hàng/Tên hàng vốn mang danh
    // tính dimension TRONG), không co gọn theo Expander như trước. Rỗng khi report chỉ 1 chiều.
    public string    OuterCode      { get; set; } = "";
    public string    OuterName      { get; set; } = "";

    // Identity of the row's own dimension (drill-down target) + the outer dimension for
    // 2-dimension report types (set alongside GroupKey/GroupLabel) — null when not applicable.
    public int? ProductId  { get; set; }
    public int? CustomerId { get; set; }
    public int? EmployeeId { get; set; }

    public static int? IdFor(SummaryDimension field, SalesOrderSummaryLineItem item) => field switch
    {
        SummaryDimension.Product  => item.ProductId,
        SummaryDimension.Customer => item.CustomerId,
        SummaryDimension.Employee => item.EmployeeId,
        _ => null,
    };

    private static (string Code, string Name) CodeNameFor(SummaryDimension field, SalesOrderSummaryLineItem item) => field switch
    {
        SummaryDimension.Product  => (item.ProductCode, item.ProductName),
        SummaryDimension.Customer => (item.CustomerCode, item.CustomerName),
        SummaryDimension.Employee => (item.EmployeeCode, item.EmployeeName),
        _ => ("", ""),
    };

    private void SetId(SummaryDimension field, int? id)
    {
        switch (field)
        {
            case SummaryDimension.Product:  ProductId  = id; break;
            case SummaryDimension.Customer: CustomerId = id; break;
            case SummaryDimension.Employee: EmployeeId = id; break;
        }
    }

    /// <summary>
    /// "Mã hàng"/"Tên hàng" always show the row's OWN identity — whichever dimension this row
    /// represents (product, customer, or employee). For 2-dimension types the OUTER dimension is
    /// shown once in the group header, so every leaf row's identity here is the INNER dimension —
    /// no separate "Khách hàng"/"Nhân viên" column is needed at all.
    /// </summary>
    public static ReportDisplayRow Aggregate(
        IReadOnlyCollection<SalesOrderSummaryLineItem> items,
        SummaryDimension identityField, bool showUnit)
    {
        var first = items.First();
        var (code, name) = CodeNameFor(identityField, first);

        var netRevenue  = items.Sum(i => i.NetRevenue);
        var costAmount  = items.Sum(i => i.CostAmount);
        var grossProfit = netRevenue - costAmount;

        var row = new ReportDisplayRow
        {
            ProductCode  = code,
            ProductName  = name,
            Unit         = showUnit ? first.Unit : "",
            QuantitySold   = items.Sum(i => i.QuantitySold),
            SalesAmount    = items.Sum(i => i.SalesAmount),
            DiscountAmount = items.Sum(i => i.DiscountAmount),
            ReturnQuantity = items.Sum(i => i.ReturnQuantity),
            ReturnValue    = items.Sum(i => i.ReturnValue),
            NetRevenue     = netRevenue,
            CostAmount     = costAmount,
            GrossProfit    = grossProfit,
            // Tính lại từ tổng đã cộng dồn (netRevenue/grossProfit), không cộng dồn % của từng
            // dòng lẻ — cộng % trực tiếp sẽ ra sai số học.
            GrossProfitRate   = netRevenue == 0 ? 0 : grossProfit / netRevenue * 100,
            // Chỉ có ý nghĩa khi report có dimension Khách hàng — vẫn gán an toàn ở đây (lấy theo
            // dòng đầu tiên trong nhóm), cột chỉ thật sự hiển thị khi IsCustomerGroupColumnVisible.
            CustomerGroupName = first.CustomerGroupName,
        };
        row.SetId(identityField, IdFor(identityField, first));
        return row;
    }

    // Called for 2-dimension report types so a leaf row also carries the OUTER dimension's id
    // + Mã/Tên thật (bảng phẳng, không co gọn) — dimension TRONG (own identity) đã có sẵn từ Aggregate.
    public void SetOuterId(SummaryDimension field, SalesOrderSummaryLineItem sample)
    {
        SetId(field, IdFor(field, sample));
        (OuterCode, OuterName) = CodeNameFor(field, sample);
    }
}
