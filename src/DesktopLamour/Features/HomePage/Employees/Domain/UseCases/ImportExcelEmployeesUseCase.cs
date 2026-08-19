// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Employees.Data.Repositories;
using DesktopLamour.Features.HomePage.Employees.Domain.Models;
using System.IO;

namespace DesktopLamour.Features.HomePage.Employees.Domain.UseCases;

public class ImportExcelEmployeesUseCase : IImportExcelEmployeesUseCase
{
    private readonly IEmployeeRepository _repo;

    public ImportExcelEmployeesUseCase(IEmployeeRepository repo) => _repo = repo;

    public Task<ImportEmployeeResult> ExecuteAsync(Stream fileStream, string fileName, CancellationToken ct = default)
        => _repo.ImportExcelAsync(fileStream, fileName, ct);
}
