// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

public class InventorySummaryItem
{
    public int     ProductId    { get; set; }
    public string  Code         { get; set; } = string.Empty;
    public string  Name         { get; set; } = string.Empty;
    public string  Unit         { get; set; } = string.Empty;
    public int     OpeningQty   { get; set; }
    public decimal OpeningValue { get; set; }
    public int     ImportQty    { get; set; }
    public decimal ImportValue  { get; set; }
    public int     ExportQty    { get; set; }
    public decimal ExportValue  { get; set; }
    public int      ClosingQty            { get; set; }
    public decimal  ClosingValue          { get; set; }
    public DateTime? LatestAccountingDate { get; set; }

    // Báo cáo chia theo kho: mỗi dòng thuộc 1 kho; dòng IsGroupHeader là dòng tổng của nhóm kho
    // ("Tên kho : Hàng Hóa (66)" nằm ở cột Tên hàng, số liệu là tổng của nhóm).
    public int     WarehouseId   { get; set; }
    public string  WarehouseCode { get; set; } = string.Empty;
    public string  WarehouseName { get; set; } = string.Empty;
    public bool    IsGroupHeader { get; set; }
}

// 1 nhóm kho của báo cáo Tổng hợp tồn kho.
public class InventoryWarehouseGroup
{
    public int    WarehouseId   { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;

    public List<InventorySummaryItem> Items { get; set; } = new();
}
