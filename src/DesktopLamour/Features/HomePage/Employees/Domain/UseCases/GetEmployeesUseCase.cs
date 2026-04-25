// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Employees.Data.Repositories;
using DesktopLamour.Features.HomePage.Employees.Domain.Models;
namespace DesktopLamour.Features.HomePage.Employees.Domain.UseCases;

public sealed class GetEmployeesUseCase : IGetEmployeesUseCase
{
    private readonly IEmployeeRepository _repository;
    public GetEmployeesUseCase(IEmployeeRepository repository) => _repository = repository;

    public Task<IEnumerable<Employee>> ExecuteAsync(CancellationToken ct = default)
        => _repository.GetAllAsync(ct);
}
