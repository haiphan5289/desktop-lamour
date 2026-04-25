// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Features.HomePage.Customers.Data.Repositories;
using DesktopLamour.Features.HomePage.Customers.Domain.Models;
namespace DesktopLamour.Features.HomePage.Customers.Domain.UseCases;

public sealed class CreateCustomerUseCase : ICreateCustomerUseCase
{
    private readonly ICustomerRepository _repository;
    public CreateCustomerUseCase(ICustomerRepository repository) => _repository = repository;

    public async Task<Customer> ExecuteAsync(CreateCustomerInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new ValidationException("Name", "Tên khách hàng không được để trống.");

        return await _repository.CreateAsync(input, ct);
    }
}
