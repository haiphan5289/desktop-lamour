// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Domain.Models;
using DesktopLamour.Features.HomePage.Domain.UseCases;
namespace DesktopLamour.Features.HomePage.Data.Repositories;

public interface ISupplierRepository
{
    Task<IEnumerable<Supplier>> GetAllAsync(CancellationToken ct = default);
    Task DeleteAsync(int supplierId, CancellationToken ct = default);
    Task<Supplier> DuplicateAsync(int supplierId, CancellationToken ct = default);
    Task<Supplier> CreateAsync(CreateSupplierInput input, CancellationToken ct = default);
    Task<Supplier> UpdateAsync(UpdateSupplierInput input, CancellationToken ct = default);
}
