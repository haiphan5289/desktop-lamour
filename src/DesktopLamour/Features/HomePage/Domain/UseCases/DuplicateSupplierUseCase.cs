// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Data.Repositories;
using DesktopLamour.Features.HomePage.Domain.Models;
namespace DesktopLamour.Features.HomePage.Domain.UseCases;

public sealed class DuplicateSupplierUseCase : IDuplicateSupplierUseCase
{
    private readonly ISupplierRepository _repository;
    public DuplicateSupplierUseCase(ISupplierRepository repository) => _repository = repository;
    public Task<Supplier> ExecuteAsync(int supplierId, CancellationToken ct = default)
        => _repository.DuplicateAsync(supplierId, ct);
}
