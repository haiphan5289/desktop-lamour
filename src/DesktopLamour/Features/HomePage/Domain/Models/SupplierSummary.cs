// SupplierSummary.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Features.HomePage.Domain.Models;

public class SupplierSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
