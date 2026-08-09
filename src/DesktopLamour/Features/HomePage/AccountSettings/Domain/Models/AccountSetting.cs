// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Shared.Controls;
namespace DesktopLamour.Features.HomePage.AccountSettings.Domain.Models;

public class AccountSetting : ISearchableItem
{
    public int    Id          { get; set; }
    public string Code        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string Name        => Description;
    public string DisplayText => $"{Code} — {Description}";
}
