// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.Employees.Domain.Models;

public class Employee : ISearchableItem
{
    public int     Id                { get; set; }
    public string  Code              { get; set; } = string.Empty;
    public string  Name              { get; set; } = string.Empty;
    public string  Phone             { get; set; } = string.Empty;
    public string  Role              { get; set; } = "Cashier";
    public string  Unit              { get; set; } = "Spa";
    public string  JobTitle          { get; set; } = "Khac";
    public string? BankAccountNumber { get; set; }
    public string? BankName          { get; set; }
    public bool    IsActive          { get; set; } = true;
    public string  DisplayText       => $"{Code} — {Name}";
}
