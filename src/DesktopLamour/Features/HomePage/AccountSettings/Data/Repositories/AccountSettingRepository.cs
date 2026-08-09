// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.AccountSettings.Data.Services;
using DesktopLamour.Features.HomePage.AccountSettings.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.AccountSettings.Domain.Models;
using DesktopLamour.Features.HomePage.AccountSettings.Domain.UseCases;
namespace DesktopLamour.Features.HomePage.AccountSettings.Data.Repositories;

public sealed class AccountSettingRepository : IAccountSettingRepository
{
    private readonly IAccountSettingService _service;
    public AccountSettingRepository(IAccountSettingService service) => _service = service;

    public async Task<IEnumerable<AccountSetting>> GetAllAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetAllAsync(ct);
        return dtos.Select(MapToModel);
    }

    public async Task<AccountSetting> CreateAsync(CreateAccountSettingInput input, CancellationToken ct = default)
    {
        var request = new CreateAccountSettingRequestDto { Code = input.Code, Description = input.Description };
        var d = await _service.CreateAsync(request, ct);
        return MapToModel(d);
    }

    public async Task<AccountSetting> UpdateAsync(UpdateAccountSettingInput input, CancellationToken ct = default)
    {
        var request = new UpdateAccountSettingRequestDto { Code = input.Code, Description = input.Description };
        var d = await _service.UpdateAsync(input.Id, request, ct);
        return MapToModel(d);
    }

    public Task DeleteAsync(int accountId, CancellationToken ct = default)
        => _service.DeleteAsync(accountId, ct);

    private static AccountSetting MapToModel(AccountSettingResponseDto d) => new()
    {
        Id          = d.Id,
        Code        = d.Code,
        Description = d.Description,
    };
}
