// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
namespace DesktopLamour.Features.HomePage.Warehouses.Data.Repositories;

public interface IDepartmentRepository
{
    Task<IEnumerable<Department>> GetAllAsync(CancellationToken ct = default);
    Task<Department> CreateAsync(CreateDepartmentInput input, CancellationToken ct = default);
    Task<Department> UpdateAsync(UpdateDepartmentInput input, CancellationToken ct = default);
    Task DeleteAsync(int departmentId, CancellationToken ct = default);
}
