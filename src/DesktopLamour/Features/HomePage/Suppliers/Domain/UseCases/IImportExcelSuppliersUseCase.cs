// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Suppliers.Domain.Models;
using System.IO;

namespace DesktopLamour.Features.HomePage.Suppliers.Domain.UseCases;

public interface IImportExcelSuppliersUseCase
{
    Task<ImportSupplierResult> ExecuteAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}
