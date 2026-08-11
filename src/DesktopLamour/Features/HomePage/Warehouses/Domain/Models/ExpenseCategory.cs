// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.Models;

public class ExpenseCategory
{
    public int     Id             { get; set; }
    public string  Code           { get; set; } = string.Empty;
    public string  Name           { get; set; } = string.Empty;
    public int?    DepartmentId   { get; set; }
    public string? DepartmentName { get; set; }
    public string? Description    { get; set; }

    public string DisplayText => $"{Code} — {Name}";
}
