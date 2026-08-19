// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Suppliers.Data.Repositories;
using DesktopLamour.Features.HomePage.Suppliers.Domain.Models;
using System.IO;

namespace DesktopLamour.Features.HomePage.Suppliers.Domain.UseCases;

public class ImportExcelSuppliersUseCase : IImportExcelSuppliersUseCase
{
    private readonly ISupplierRepository _repo;

    public ImportExcelSuppliersUseCase(ISupplierRepository repo) => _repo = repo;

    public Task<ImportSupplierResult> ExecuteAsync(Stream fileStream, string fileName, CancellationToken ct = default)
        => _repo.ImportExcelAsync(fileStream, fileName, ct);
}
