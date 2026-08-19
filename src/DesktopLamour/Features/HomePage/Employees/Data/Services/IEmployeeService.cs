// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Employees.Data.Services.Dtos;
using System.IO;
namespace DesktopLamour.Features.HomePage.Employees.Data.Services;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task DeleteAsync(int employeeId, CancellationToken ct = default);
    Task<EmployeeResponseDto> DuplicateAsync(int employeeId, CancellationToken ct = default);
    Task<EmployeeResponseDto> CreateAsync(CreateEmployeeRequestDto request, CancellationToken ct = default);
    Task<EmployeeResponseDto> UpdateAsync(int employeeId, UpdateEmployeeRequestDto request, CancellationToken ct = default);
    Task<ImportEmployeeResultDto> ImportExcelAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}
