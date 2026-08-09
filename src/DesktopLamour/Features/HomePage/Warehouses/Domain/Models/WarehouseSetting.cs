// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Shared.Controls;
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.Models;

public class WarehouseSetting : ISearchableItem
{
    public int    Id       { get; set; }
    public string Code     { get; set; } = string.Empty;
    public string Name     { get; set; } = string.Empty;
    public bool   IsActive { get; set; } = true;

    public string DisplayText => $"{Code} — {Name}";
}
