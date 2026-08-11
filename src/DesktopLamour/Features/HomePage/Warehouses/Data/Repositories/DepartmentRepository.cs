// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.Data.Services;
using DesktopLamour.Features.HomePage.Warehouses.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
namespace DesktopLamour.Features.HomePage.Warehouses.Data.Repositories;

public sealed class DepartmentRepository : IDepartmentRepository
{
    private readonly IDepartmentService _service;
    public DepartmentRepository(IDepartmentService service) => _service = service;

    public async Task<IEnumerable<Department>> GetAllAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetAllAsync(ct);
        return dtos.Select(MapToModel);
    }

    public async Task<Department> CreateAsync(CreateDepartmentInput input, CancellationToken ct = default)
    {
        var request = new CreateDepartmentRequestDto { Name = input.Name };
        var d = await _service.CreateAsync(request, ct);
        return MapToModel(d);
    }

    public async Task<Department> UpdateAsync(UpdateDepartmentInput input, CancellationToken ct = default)
    {
        var request = new UpdateDepartmentRequestDto { Name = input.Name };
        var d = await _service.UpdateAsync(input.Id, request, ct);
        return MapToModel(d);
    }

    public Task DeleteAsync(int departmentId, CancellationToken ct = default)
        => _service.DeleteAsync(departmentId, ct);

    private static Department MapToModel(DepartmentResponseDto d) => new()
    {
        Id   = d.Id,
        Name = d.Name,
    };
}
