// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Features.HomePage.Employees.Data.Repositories;
using DesktopLamour.Features.HomePage.Employees.Domain.Models;
namespace DesktopLamour.Features.HomePage.Employees.Domain.UseCases;

public sealed class CreateEmployeeUseCase : ICreateEmployeeUseCase
{
    private readonly IEmployeeRepository _repository;
    public CreateEmployeeUseCase(IEmployeeRepository repository) => _repository = repository;

    public async Task<Employee> ExecuteAsync(CreateEmployeeInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new ValidationException("Name", "Tên nhân viên không được để trống.");

        return await _repository.CreateAsync(input, ct);
    }
}
