// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.Customers.Domain.Models;

public class Customer : ISearchableItem
{
    public int     Id                   { get; set; }
    public string  Code                 { get; set; } = string.Empty;
    public string  Name                 { get; set; } = string.Empty;
    public string  Address              { get; set; } = string.Empty;
    public string  Province             { get; set; } = string.Empty;
    public string  CustomerGroup        { get; set; } = string.Empty;
    public string  TaxCode              { get; set; } = string.Empty;
    public string  Phone                { get; set; } = string.Empty;
    public int?    SaleCareEmployeeId   { get; set; }
    public string? SaleCareEmployeeName { get; set; }
    public string  DisplayText          => $"{Code} — {Name}";
}
