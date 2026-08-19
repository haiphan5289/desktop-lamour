// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Employees.Domain.Models;
using System.IO;

namespace DesktopLamour.Features.HomePage.Employees.Domain.UseCases;

public interface IImportExcelEmployeesUseCase
{
    Task<ImportEmployeeResult> ExecuteAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}
