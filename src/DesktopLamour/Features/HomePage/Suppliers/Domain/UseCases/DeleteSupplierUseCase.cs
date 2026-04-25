// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Suppliers.Data.Repositories;
namespace DesktopLamour.Features.HomePage.Suppliers.Domain.UseCases;

public sealed class DeleteSupplierUseCase : IDeleteSupplierUseCase
{
    private readonly ISupplierRepository _repository;
    public DeleteSupplierUseCase(ISupplierRepository repository) => _repository = repository;
    public Task ExecuteAsync(int supplierId, CancellationToken ct = default)
        => _repository.DeleteAsync(supplierId, ct);
}
