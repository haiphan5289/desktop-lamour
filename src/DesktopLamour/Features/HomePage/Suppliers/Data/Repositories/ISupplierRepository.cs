// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Suppliers.Domain.Models;
using DesktopLamour.Features.HomePage.Suppliers.Domain.UseCases;
using System.IO;
namespace DesktopLamour.Features.HomePage.Suppliers.Data.Repositories;

public interface ISupplierRepository
{
    Task<IEnumerable<Supplier>> GetAllAsync(CancellationToken ct = default);
    Task DeleteAsync(int supplierId, CancellationToken ct = default);
    Task<Supplier> DuplicateAsync(int supplierId, CancellationToken ct = default);
    Task<Supplier> CreateAsync(CreateSupplierInput input, CancellationToken ct = default);
    Task<Supplier> UpdateAsync(UpdateSupplierInput input, CancellationToken ct = default);
    Task<ImportSupplierResult> ImportExcelAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}
