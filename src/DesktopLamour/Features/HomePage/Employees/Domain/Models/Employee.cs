// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Employees.Domain.Models;

public class Employee
{
    public int    Id       { get; set; }
    public string Name     { get; set; } = string.Empty;
    public string Phone    { get; set; } = string.Empty;
    public string Role     { get; set; } = "Cashier";
    public bool   IsActive { get; set; } = true;
}
