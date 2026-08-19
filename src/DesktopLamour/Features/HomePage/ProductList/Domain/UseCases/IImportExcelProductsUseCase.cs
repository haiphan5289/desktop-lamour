// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using System.IO;

namespace DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;

public interface IImportExcelProductsUseCase
{
    Task<ImportProductResult> ExecuteAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}
