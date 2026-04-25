// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Customers.Data.Repositories;
using DesktopLamour.Features.HomePage.Customers.Domain.Models;
namespace DesktopLamour.Features.HomePage.Customers.Domain.UseCases;

public sealed class GetCustomersUseCase : IGetCustomersUseCase
{
    private readonly ICustomerRepository _repository;
    public GetCustomersUseCase(ICustomerRepository repository) => _repository = repository;
    public Task<IEnumerable<Customer>> ExecuteAsync(CancellationToken ct = default)
        => _repository.GetAllAsync(ct);
}
