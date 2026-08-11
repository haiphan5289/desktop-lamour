// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

// Trạng thái filter của popup "Tổng hợp tồn kho" — được TongHopTonKhoViewModel giữ lại
// giữa các lần mở popup để không mất lựa chọn trước đó.
public class InventoryFilter
{
    public DateTime      FromDate      { get; set; } = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    public DateTime      ToDate        { get; set; } = DateTime.Today;
    public int?          CategoryId    { get; set; }
    public int?          ProductUnitId { get; set; }
    public List<int>     WarehouseIds  { get; set; } = new();
}
