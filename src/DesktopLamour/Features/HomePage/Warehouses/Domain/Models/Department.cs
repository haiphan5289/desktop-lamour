// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Shared.Controls;
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.Models;

public class Department : ISearchableItem
{
    public int    Id   { get; set; }
    public string Name { get; set; } = string.Empty;

    public string Code        => string.Empty;
    public string DisplayText => Name;
}
