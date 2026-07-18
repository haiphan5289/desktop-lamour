// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;

namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

public partial class ProductCheckItem : ObservableObject
{
    public int    Id   { get; }
    public string Code { get; }
    public string Name { get; }

    [ObservableProperty] private bool _isSelected;

    public ProductCheckItem(Product product)
    {
        Id   = product.Id;
        Code = product.Code;
        Name = product.Name;
    }
}
