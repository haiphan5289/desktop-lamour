// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.Data.Services;
using DesktopLamour.Features.HomePage.Warehouses.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
namespace DesktopLamour.Features.HomePage.Warehouses.Data.Repositories;

public sealed class ExpenseCategoryRepository : IExpenseCategoryRepository
{
    private readonly IExpenseCategoryService _service;
    public ExpenseCategoryRepository(IExpenseCategoryService service) => _service = service;

    public async Task<IEnumerable<ExpenseCategory>> GetAllAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetAllAsync(ct);
        return dtos.Select(MapToModel);
    }

    public async Task<ExpenseCategory> CreateAsync(CreateExpenseCategoryInput input, CancellationToken ct = default)
    {
        var request = new CreateExpenseCategoryRequestDto
        {
            Code         = input.Code,
            Name         = input.Name,
            DepartmentId = input.DepartmentId,
            Description  = input.Description,
        };
        var e = await _service.CreateAsync(request, ct);
        return MapToModel(e);
    }

    public async Task<ExpenseCategory> UpdateAsync(UpdateExpenseCategoryInput input, CancellationToken ct = default)
    {
        var request = new UpdateExpenseCategoryRequestDto
        {
            Code         = input.Code,
            Name         = input.Name,
            DepartmentId = input.DepartmentId,
            Description  = input.Description,
        };
        var e = await _service.UpdateAsync(input.Id, request, ct);
        return MapToModel(e);
    }

    public Task DeleteAsync(int categoryId, CancellationToken ct = default)
        => _service.DeleteAsync(categoryId, ct);

    private static ExpenseCategory MapToModel(ExpenseCategoryResponseDto e) => new()
    {
        Id             = e.Id,
        Code           = e.Code,
        Name           = e.Name,
        DepartmentId   = e.DepartmentId,
        DepartmentName = e.DepartmentName,
        Description    = e.Description,
    };
}
