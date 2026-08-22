// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

// Tham số điều hướng khi double-click 1 dòng ở Tổng hợp tồn kho — kế thừa đúng khoảng ngày/kho
// đang lọc ở màn tổng hợp, narrow xuống đúng 1 sản phẩm.
public class InventoryDetailFilter
{
    public int      ProductId    { get; init; }
    public string   ProductLabel { get; init; } = "";
    public DateTime FromDate     { get; init; }
    public DateTime ToDate       { get; init; }
    public IReadOnlyList<int>? WarehouseIds { get; init; }
    public string?  WarehouseLabel { get; init; }
}
