// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Employees.Data.Repositories;
using DesktopLamour.Features.HomePage.Employees.Domain.Models;
namespace DesktopLamour.Features.HomePage.Employees.Domain.UseCases;

public sealed class DuplicateEmployeeUseCase : IDuplicateEmployeeUseCase
{
    private readonly IEmployeeRepository _repository;
    public DuplicateEmployeeUseCase(IEmployeeRepository repository) => _repository = repository;

    public Task<Employee> ExecuteAsync(int employeeId, CancellationToken ct = default)
        => _repository.DuplicateAsync(employeeId, ct);
}
