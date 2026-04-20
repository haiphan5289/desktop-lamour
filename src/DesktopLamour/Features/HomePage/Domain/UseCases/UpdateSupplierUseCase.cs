// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Features.HomePage.Data.Repositories;
using DesktopLamour.Features.HomePage.Domain.Models;
namespace DesktopLamour.Features.HomePage.Domain.UseCases;

public sealed class UpdateSupplierUseCase : IUpdateSupplierUseCase
{
    private readonly ISupplierRepository _repository;
    public UpdateSupplierUseCase(ISupplierRepository repository) => _repository = repository;

    public async Task<Supplier> ExecuteAsync(UpdateSupplierInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new ValidationException("Name", "Tên nhà cung cấp không được để trống.");

        return await _repository.UpdateAsync(input, ct);
    }
}
