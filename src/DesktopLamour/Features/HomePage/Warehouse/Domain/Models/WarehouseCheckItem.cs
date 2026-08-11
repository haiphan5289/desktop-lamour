// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

public partial class WarehouseCheckItem : ObservableObject
{
    public int    Id   { get; }
    public string Code { get; }
    public string Name { get; }

    [ObservableProperty] private bool _isSelected;

    public WarehouseCheckItem(ISearchableItem warehouse)
    {
        Id   = warehouse.Id;
        Code = warehouse.Code;
        Name = warehouse.Name;
    }
}
