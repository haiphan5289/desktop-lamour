// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Customers.Domain.Models;
using DesktopLamour.Features.HomePage.Suppliers.Domain.Models;
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

public enum WarehouseObjectType
{
    Customer,
    Supplier,
}

/// <summary>
/// Wraps <see cref="Customer"/> or <see cref="Supplier"/> to expose a single "Đối tượng"
/// searchable dropdown that can resolve back to the correct id when saving.
/// </summary>
public sealed class WarehouseObjectItem : ISearchableItem
{
    public WarehouseObjectType Type { get; }
    public int     Id          { get; }
    public string  Code        { get; }
    public string  Name        { get; }
    public string  DisplayText { get; }
    public string? Phone       { get; }

    public WarehouseObjectItem(Customer customer)
    {
        Type        = WarehouseObjectType.Customer;
        Id          = customer.Id;
        Code        = customer.Code;
        Name        = customer.Name;
        DisplayText = customer.DisplayText;
        Phone       = customer.Phone;
    }

    public WarehouseObjectItem(Supplier supplier)
    {
        Type        = WarehouseObjectType.Supplier;
        Id          = supplier.Id;
        Code        = supplier.Code;
        Name        = supplier.Name;
        DisplayText = supplier.DisplayText;
        Phone       = supplier.Phone;
    }
}
