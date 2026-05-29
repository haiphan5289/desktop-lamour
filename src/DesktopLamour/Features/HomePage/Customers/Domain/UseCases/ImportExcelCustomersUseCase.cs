// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Customers.Data.Repositories;
using DesktopLamour.Features.HomePage.Customers.Domain.Models;
using System.IO;

namespace DesktopLamour.Features.HomePage.Customers.Domain.UseCases;

public class ImportExcelCustomersUseCase : IImportExcelCustomersUseCase
{
    private readonly ICustomerRepository _repo;

    public ImportExcelCustomersUseCase(ICustomerRepository repo) => _repo = repo;

    public Task<ImportCustomerResult> ExecuteAsync(Stream fileStream, string fileName, CancellationToken ct = default)
        => _repo.ImportExcelAsync(fileStream, fileName, ct);
}
