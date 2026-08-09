// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Cache;
using DesktopLamour.Features.HomePage.AccountSettings.Data.Services.Dtos;
namespace DesktopLamour.Features.HomePage.AccountSettings.Data.Cache;

public interface IAccountSettingCacheStore : IEntityCacheStore<AccountSettingResponseDto>
{
}
