// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

// Loại dòng trên lưới "Sổ chi tiết": dòng giao dịch thật, hoặc dòng nhóm/số dư đầu kỳ do màn hình dựng.
public enum InventoryDetailRowKind
{
    Line,
    WarehouseHeader,   // "Mã kho : HH (1)" + tổng nhóm
    ProductHeader,     // "Mã hàng : 15 (8)" + tổng nhóm
    Opening,           // "Số dư đầu kỳ"
}

public class InventoryDetailLine
{
    public InventoryDetailRowKind RowKind { get; set; } = InventoryDetailRowKind.Line;

    public int     WarehouseId   { get; set; }
    public string  WarehouseCode { get; set; } = string.Empty;
    // Cột "Tên kho": tên kho ở dòng thường/số dư; chữ nhóm ("Mã kho : HH (1)") ở dòng tiêu đề nhóm.
    public string  WarehouseName { get; set; } = string.Empty;
    public string  ProductName   { get; set; } = string.Empty;
    public decimal UnitPrice     { get; set; }

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

    // Dòng nhóm / số dư đầu kỳ không có ngày — để trống thay vì hiện 01/01/0001.
    public string AccountingDateText => RowKind == InventoryDetailRowKind.Line ? AccountingDate.ToString("dd/MM/yyyy") : string.Empty;
    public string DocumentDateText   => RowKind == InventoryDetailRowKind.Line ? DocumentDate.ToString("dd/MM/yyyy")   : string.Empty;

    public bool IsClickable => SourceId.HasValue && (DocumentType == "Import" || DocumentType == "Export");
}

public class InventoryDetailWarehouse
{
    public int     WarehouseId   { get; set; }
    public string  WarehouseCode { get; set; } = string.Empty;
    public string  WarehouseName { get; set; } = string.Empty;
    public int     OpeningQty    { get; set; }
    public decimal OpeningValue  { get; set; }
    public int     ClosingQty    { get; set; }
    public decimal ClosingValue  { get; set; }
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

    public List<InventoryDetailWarehouse> Warehouses { get; set; } = new();
    public List<InventoryDetailLine> Lines { get; set; } = new();
}
