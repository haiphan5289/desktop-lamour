// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Employees.Data.Services;
using DesktopLamour.Features.HomePage.Employees.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Employees.Domain.Models;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
namespace DesktopLamour.Features.HomePage.Employees.Data.Repositories;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly IEmployeeService _service;
    public EmployeeRepository(IEmployeeService service) => _service = service;

    public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetAllAsync(ct);
        return dtos.Select(MapToModel);
    }

    public Task DeleteAsync(int employeeId, CancellationToken ct = default)
        => _service.DeleteAsync(employeeId, ct);

    public async Task<Employee> DuplicateAsync(int employeeId, CancellationToken ct = default)
        => MapToModel(await _service.DuplicateAsync(employeeId, ct));

    public async Task<Employee> CreateAsync(CreateEmployeeInput input, CancellationToken ct = default)
    {
        var request = new CreateEmployeeRequestDto
        {
            Name     = input.Name,
            Phone    = input.Phone,
            Role     = input.Role,
            Password = input.Password,
            IsActive = input.IsActive,
        };
        return MapToModel(await _service.CreateAsync(request, ct));
    }

    public async Task<Employee> UpdateAsync(UpdateEmployeeInput input, CancellationToken ct = default)
    {
        var request = new UpdateEmployeeRequestDto
        {
            Name     = input.Name,
            Phone    = input.Phone,
            Role     = input.Role,
            Password = input.Password,
            IsActive = input.IsActive,
        };
        return MapToModel(await _service.UpdateAsync(input.Id, request, ct));
    }

    private static Employee MapToModel(EmployeeResponseDto d) => new()
    {
        Id       = d.Id,
        Name     = d.Name,
        Phone    = d.Phone,
        Role     = d.Role,
        IsActive = d.IsActive,
    };
}
