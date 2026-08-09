// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Features.HomePage.AccountSettings.Data.Repositories;
using DesktopLamour.Features.HomePage.AccountSettings.Domain.Models;
namespace DesktopLamour.Features.HomePage.AccountSettings.Domain.UseCases;

public sealed class CreateAccountSettingUseCase : ICreateAccountSettingUseCase
{
    private readonly IAccountSettingRepository _repository;
    public CreateAccountSettingUseCase(IAccountSettingRepository repository) => _repository = repository;

    public async Task<AccountSetting> ExecuteAsync(CreateAccountSettingInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Code))
            throw new ValidationException("Code", "Số tài khoản không được để trống.");
        if (string.IsNullOrWhiteSpace(input.Description))
            throw new ValidationException("Description", "Tên tài khoản không được để trống.");

        var existing = await _repository.GetAllAsync(ct);
        if (existing.Any(a => a.Code.Equals(input.Code.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ValidationException("Code", $"Tài khoản '{input.Code}' đã tồn tại.");

        return await _repository.CreateAsync(input, ct);
    }
}
