// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.Data.Services.Dtos;
namespace DesktopLamour.Features.HomePage.Warehouses.Data.Services;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<DepartmentResponseDto> CreateAsync(CreateDepartmentRequestDto request, CancellationToken ct = default);
    Task<DepartmentResponseDto> UpdateAsync(int departmentId, UpdateDepartmentRequestDto request, CancellationToken ct = default);
    Task DeleteAsync(int departmentId, CancellationToken ct = default);
}
