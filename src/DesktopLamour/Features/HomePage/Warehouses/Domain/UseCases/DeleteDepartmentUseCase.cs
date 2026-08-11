// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.Data.Repositories;
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;

public sealed class DeleteDepartmentUseCase : IDeleteDepartmentUseCase
{
    private readonly IDepartmentRepository _repository;
    public DeleteDepartmentUseCase(IDepartmentRepository repository) => _repository = repository;
    public Task ExecuteAsync(int departmentId, CancellationToken ct = default)
        => _repository.DeleteAsync(departmentId, ct);
}
