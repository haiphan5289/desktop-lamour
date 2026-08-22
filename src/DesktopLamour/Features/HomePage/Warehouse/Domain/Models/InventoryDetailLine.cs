// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

public class InventoryDetailLine
{
    public DateTime AccountingDate { get; set; }
    public DateTime DocumentDate   { get; set; }
    public string   DocumentNumber { get; set; } = string.Empty;

    // "Import" | "Export" | "SalesReturn" — xem GetTransactionLinesByProductAsync phía BE.
    public string DocumentType { get; set; } = string.Empty;

    // Id của WarehouseReceipt (Import) hoặc SalesOrder (Export) để mở lại chứng từ gốc khi click
    // Số chứng từ — null cho SalesReturn (chưa hỗ trợ xem lại từ màn này).
    public int?    SourceId    { get; set; }
    public string? Description { get; set; }
    public string  Unit        { get; set; } = string.Empty;

    public int     ImportQty   { get; set; }
    public decimal ImportValue { get; set; }
    public int     ExportQty   { get; set; }
    public decimal ExportValue { get; set; }

    public int     RunningQty   { get; set; }
    public decimal RunningValue { get; set; }

    public bool IsClickable => SourceId.HasValue && (DocumentType == "Import" || DocumentType == "Export");
}

public class InventoryDetail
{
    public int     ProductId    { get; set; }
    public string  Code         { get; set; } = string.Empty;
    public string  Name         { get; set; } = string.Empty;
    public string  Unit         { get; set; } = string.Empty;
    public int     OpeningQty   { get; set; }
    public decimal OpeningValue { get; set; }
    public int     ClosingQty   { get; set; }
    public decimal ClosingValue { get; set; }

    public List<InventoryDetailLine> Lines { get; set; } = new();
}
