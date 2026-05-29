// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Shared.Controls;
namespace DesktopLamour.Features.HomePage.ProductList.Domain.Models;

public class Product : ISearchableItem
{
    public int          Id               { get; set; }
    public string       Code             { get; set; } = string.Empty;
    public string       Name             { get; set; } = string.Empty;
    public string       Category         { get; set; } = string.Empty;
    public string       Unit             { get; set; } = string.Empty;
    public decimal      CostPrice        { get; set; }
    public decimal      SellingPrice     { get; set; }
    public int          StockQuantity    { get; set; }
    public bool         IsActive         { get; set; } = true;

    // Tax fields
    public VatRateType? VatRate          { get; set; }
    public TaxReductionStatus? TaxReductionType { get; set; }
    public decimal?     ImportTaxRate    { get; set; }
    public decimal?     ExportTaxRate    { get; set; }
    public string?      ExciseTaxGroup   { get; set; }

    public string DisplayText => $"{Code} — {Name}";
}
