// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Customers.Domain.Models;

public class Customer
{
    public int    Id            { get; set; }
    public string Code          { get; set; } = string.Empty;
    public string Name          { get; set; } = string.Empty;
    public string Address       { get; set; } = string.Empty;
    public string Province      { get; set; } = string.Empty;
    public string CustomerGroup { get; set; } = string.Empty;
    public string TaxCode       { get; set; } = string.Empty;
    public string Phone         { get; set; } = string.Empty;
}
