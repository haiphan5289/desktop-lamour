// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Suppliers.Data.Repositories;
using DesktopLamour.Features.HomePage.Suppliers.Domain.Models;
namespace DesktopLamour.Features.HomePage.Suppliers.Domain.UseCases;

public sealed class GetSuppliersUseCase : IGetSuppliersUseCase
{
    private readonly ISupplierRepository _repository;
    public GetSuppliersUseCase(ISupplierRepository repository) => _repository = repository;
    public Task<IEnumerable<Supplier>> ExecuteAsync(CancellationToken ct = default)
        => _repository.GetAllAsync(ct);
}
