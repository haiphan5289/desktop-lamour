// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

public partial class ProductCheckItem : ObservableObject
{
    public int    Id          { get; }
    public string Code        { get; }
    public string Name        { get; }
    public string DisplayText { get; }

    [ObservableProperty] private bool _isSelected;

    public ProductCheckItem(ISearchableItem product)
    {
        Id          = product.Id;
        Code        = product.Code;
        Name        = product.Name;
        DisplayText = product.DisplayText;
    }
}
