// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

// 1 dòng trong panel "Báo cáo theo Mặt hàng" nhúng trên SalesOrderListView — mỗi dòng là 1 sản
// phẩm, cộng dồn từ đầu tháng đến hôm nay, không lọc theo Nhân viên/Khách hàng/ĐVT/Nhóm VTHH.
public class ProductSalesSummaryRow
{
    public int     ProductId      { get; init; }
    public string  ProductCode    { get; init; } = "";
    public string  ProductName    { get; init; } = "";
    public string  Unit           { get; init; } = "";
    public int     QuantitySold   { get; init; }
    public decimal SalesAmount    { get; init; }
    public decimal DiscountAmount { get; init; }
    public int     ReturnQuantity { get; init; }
    public decimal ReturnValue    { get; init; }
    public decimal NetRevenue     { get; init; }
}
