// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Employees.Data.Repositories;
namespace DesktopLamour.Features.HomePage.Employees.Domain.UseCases;

public sealed class DeleteEmployeeUseCase : IDeleteEmployeeUseCase
{
    private readonly IEmployeeRepository _repository;
    public DeleteEmployeeUseCase(IEmployeeRepository repository) => _repository = repository;

    public Task ExecuteAsync(int employeeId, CancellationToken ct = default)
        => _repository.DeleteAsync(employeeId, ct);
}
