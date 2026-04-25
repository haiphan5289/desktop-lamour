// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Employees.Domain.Models;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
namespace DesktopLamour.Features.HomePage.Employees.Data.Repositories;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default);
    Task DeleteAsync(int employeeId, CancellationToken ct = default);
    Task<Employee> DuplicateAsync(int employeeId, CancellationToken ct = default);
    Task<Employee> CreateAsync(CreateEmployeeInput input, CancellationToken ct = default);
    Task<Employee> UpdateAsync(UpdateEmployeeInput input, CancellationToken ct = default);
}
