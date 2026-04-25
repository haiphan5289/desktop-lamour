// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Customers.Data.Repositories;
using DesktopLamour.Features.HomePage.Customers.Domain.Models;
namespace DesktopLamour.Features.HomePage.Customers.Domain.UseCases;

public sealed class DuplicateCustomerUseCase : IDuplicateCustomerUseCase
{
    private readonly ICustomerRepository _repository;
    public DuplicateCustomerUseCase(ICustomerRepository repository) => _repository = repository;
    public Task<Customer> ExecuteAsync(int customerId, CancellationToken ct = default)
        => _repository.DuplicateAsync(customerId, ct);
}
