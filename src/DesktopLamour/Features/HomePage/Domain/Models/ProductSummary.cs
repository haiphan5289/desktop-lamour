// ProductSummary.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Features.HomePage.Domain.Models;

public class ProductSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
}
