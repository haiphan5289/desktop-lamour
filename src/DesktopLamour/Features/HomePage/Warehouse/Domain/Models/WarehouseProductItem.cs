// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

/// <summary>
/// Wraps <see cref="Product"/> to expose <see cref="ISearchableItem"/> for dropdowns.
/// </summary>
public sealed class WarehouseProductItem : ISearchableItem
{
    private readonly Product _product;

    public WarehouseProductItem(Product product) => _product = product;

    public int     Id          => _product.Id;
    public string  Code        => _product.Code;
    public string  Name        => _product.Name;
    public string  Unit        => _product.Unit;
    public decimal CostPrice   => _product.CostPrice;
    public string  DisplayText => $"{_product.Code} — {_product.Name}";
}
