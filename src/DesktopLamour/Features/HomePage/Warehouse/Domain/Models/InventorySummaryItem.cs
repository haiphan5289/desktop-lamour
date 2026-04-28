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
}
