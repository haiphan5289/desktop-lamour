// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Employees.Domain.UseCases;

public interface IDeleteEmployeeUseCase
{
    Task ExecuteAsync(int employeeId, CancellationToken ct = default);
}
