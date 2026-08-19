// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductList.Data.Repositories;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using System.IO;

namespace DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;

public class ImportExcelProductsUseCase : IImportExcelProductsUseCase
{
    private readonly IProductRepository _repo;

    public ImportExcelProductsUseCase(IProductRepository repo) => _repo = repo;

    public Task<ImportProductResult> ExecuteAsync(Stream fileStream, string fileName, CancellationToken ct = default)
        => _repo.ImportExcelAsync(fileStream, fileName, ct);
}
