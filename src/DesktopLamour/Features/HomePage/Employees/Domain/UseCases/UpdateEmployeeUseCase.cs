// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Features.HomePage.Employees.Data.Repositories;
using DesktopLamour.Features.HomePage.Employees.Domain.Models;
namespace DesktopLamour.Features.HomePage.Employees.Domain.UseCases;

public sealed class UpdateEmployeeUseCase : IUpdateEmployeeUseCase
{
    private readonly IEmployeeRepository _repository;
    public UpdateEmployeeUseCase(IEmployeeRepository repository) => _repository = repository;

    public async Task<Employee> ExecuteAsync(UpdateEmployeeInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new ValidationException("Name", "Tên nhân viên không được để trống.");
        if (string.IsNullOrWhiteSpace(input.Phone))
            throw new ValidationException("Phone", "Số điện thoại không được để trống.");

        return await _repository.UpdateAsync(input, ct);
    }
}
