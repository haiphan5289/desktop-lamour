// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.AccountSettings.Data.Services.Dtos;
namespace DesktopLamour.Features.HomePage.AccountSettings.Data.Services;

public interface IAccountSettingService
{
    Task<IEnumerable<AccountSettingResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<AccountSettingResponseDto> CreateAsync(CreateAccountSettingRequestDto request, CancellationToken ct = default);
    Task<AccountSettingResponseDto> UpdateAsync(int accountId, UpdateAccountSettingRequestDto request, CancellationToken ct = default);
    Task DeleteAsync(int accountId, CancellationToken ct = default);
}
