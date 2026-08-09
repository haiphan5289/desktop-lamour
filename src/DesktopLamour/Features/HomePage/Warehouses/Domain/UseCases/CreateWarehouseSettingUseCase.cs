// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Features.HomePage.Warehouses.Data.Repositories;
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;

public sealed class CreateWarehouseSettingUseCase : ICreateWarehouseSettingUseCase
{
    private readonly IWarehouseSettingRepository _repository;
    public CreateWarehouseSettingUseCase(IWarehouseSettingRepository repository) => _repository = repository;

    public async Task<WarehouseSetting> ExecuteAsync(CreateWarehouseSettingInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Code))
            throw new ValidationException("Code", "Mã kho không được để trống.");
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new ValidationException("Name", "Tên kho không được để trống.");

        var existing = await _repository.GetAllAsync(ct);
        if (existing.Any(w => w.Code.Equals(input.Code.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ValidationException("Code", $"Kho '{input.Code}' đã tồn tại.");

        return await _repository.CreateAsync(input, ct);
    }
}
