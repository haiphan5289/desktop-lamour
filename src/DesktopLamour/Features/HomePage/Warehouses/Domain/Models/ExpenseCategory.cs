// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.Warehouses.Domain.Models;

// ISearchableItem: để ô Khoản mục CP trong lưới Phiếu chi gõ mã + lọc được (AppSearchableComboBox dạng gọn).
public class ExpenseCategory : ISearchableItem
{
    public int     Id             { get; set; }
    public string  Code           { get; set; } = string.Empty;
    public string  Name           { get; set; } = string.Empty;
    public int?    DepartmentId   { get; set; }
    public string? DepartmentName { get; set; }
    public string? Description    { get; set; }

    public string DisplayText => $"{Code} — {Name}";
}
