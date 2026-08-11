// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Features.HomePage.Warehouses.Data.Repositories;
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;

public sealed class UpdateDepartmentUseCase : IUpdateDepartmentUseCase
{
    private readonly IDepartmentRepository _repository;
    public UpdateDepartmentUseCase(IDepartmentRepository repository) => _repository = repository;

    public async Task<Department> ExecuteAsync(UpdateDepartmentInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new ValidationException("Name", "Tên phòng ban không được để trống.");

        var existing = await _repository.GetAllAsync(ct);
        if (existing.Any(d => d.Id != input.Id && d.Name.Equals(input.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ValidationException("Name", $"Phòng ban '{input.Name}' đã tồn tại.");

        return await _repository.UpdateAsync(input, ct);
    }
}
