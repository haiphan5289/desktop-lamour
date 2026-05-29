// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Customers.Domain.Models;
using System.IO;

namespace DesktopLamour.Features.HomePage.Customers.Domain.UseCases;

public interface IImportExcelCustomersUseCase
{
    Task<ImportCustomerResult> ExecuteAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}
