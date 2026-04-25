// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Employees.Domain.Models;
namespace DesktopLamour.Features.HomePage.Employees.Domain.UseCases;

public interface IGetEmployeesUseCase
{
    Task<IEnumerable<Employee>> ExecuteAsync(CancellationToken ct = default);
}
