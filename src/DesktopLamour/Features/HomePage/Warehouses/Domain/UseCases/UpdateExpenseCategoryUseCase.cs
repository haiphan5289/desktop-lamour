// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Features.HomePage.Warehouses.Data.Repositories;
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;

public sealed class UpdateExpenseCategoryUseCase : IUpdateExpenseCategoryUseCase
{
    private readonly IExpenseCategoryRepository _repository;
    public UpdateExpenseCategoryUseCase(IExpenseCategoryRepository repository) => _repository = repository;

    public async Task<ExpenseCategory> ExecuteAsync(UpdateExpenseCategoryInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Code))
            throw new ValidationException("Code", "Mã khoản mục chi phí không được để trống.");
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new ValidationException("Name", "Tên khoản mục chi phí không được để trống.");

        var existing = await _repository.GetAllAsync(ct);
        if (existing.Any(e => e.Id != input.Id && e.Code.Equals(input.Code.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ValidationException("Code", $"Khoản mục chi phí '{input.Code}' đã tồn tại.");

        return await _repository.UpdateAsync(input, ct);
    }
}
