// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Features.HomePage.Data.Repositories;
using DesktopLamour.Features.HomePage.Domain.Models;
namespace DesktopLamour.Features.HomePage.Domain.UseCases;

public sealed class CreateSupplierUseCase : ICreateSupplierUseCase
{
    private readonly ISupplierRepository _repository;
    public CreateSupplierUseCase(ISupplierRepository repository) => _repository = repository;

    public async Task<Supplier> ExecuteAsync(CreateSupplierInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Code))
            throw new ValidationException("Code", "Mã nhà cung cấp không được để trống.");
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new ValidationException("Name", "Tên nhà cung cấp không được để trống.");

        var existing = await _repository.GetAllAsync(ct);
        if (existing.Any(s => s.Code.Equals(input.Code.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ValidationException("Code", $"Mã '{input.Code}' đã tồn tại.");

        return await _repository.CreateAsync(input, ct);
    }
}
