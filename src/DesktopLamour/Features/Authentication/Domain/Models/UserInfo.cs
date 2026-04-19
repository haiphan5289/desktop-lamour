// UserInfo.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Features.Authentication.Domain.Models;

public class UserInfo
{
    public int    UserId          { get; set; }
    public string Phone           { get; set; } = string.Empty;
    public string? Email          { get; set; }
    public string? Name           { get; set; }
    public string? AvatarUrl      { get; set; }
    public string? AccessToken    { get; set; }
    public DateTime CreatedAt     { get; set; }
    public bool IsPhoneVerified   { get; set; }
}
