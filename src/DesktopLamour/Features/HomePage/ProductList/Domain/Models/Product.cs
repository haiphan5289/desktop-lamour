// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Shared.Controls;
namespace DesktopLamour.Features.HomePage.ProductList.Domain.Models;

public class Product : ISearchableItem
{
    public int     Id            { get; set; }
    public string  Code          { get; set; } = string.Empty;
    public string  Name          { get; set; } = string.Empty;
    public string  Category      { get; set; } = string.Empty;
    public string  Unit          { get; set; } = string.Empty;
    public decimal CostPrice     { get; set; }
    public decimal SellingPrice  { get; set; }
    public int     StockQuantity { get; set; }
    public bool    IsActive      { get; set; } = true;

    public string DisplayText => $"{Code} — {Name}";
}
