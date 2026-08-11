// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.Data.Repositories;
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;

public sealed class GetDepartmentsUseCase : IGetDepartmentsUseCase
{
    private readonly IDepartmentRepository _repository;
    public GetDepartmentsUseCase(IDepartmentRepository repository) => _repository = repository;

    public Task<IEnumerable<Department>> ExecuteAsync(CancellationToken ct = default)
        => _repository.GetAllAsync(ct);
}
