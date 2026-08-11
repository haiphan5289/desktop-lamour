// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;

public interface IDeleteDepartmentUseCase
{
    Task ExecuteAsync(int departmentId, CancellationToken ct = default);
}
