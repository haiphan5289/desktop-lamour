// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Customers.Data.Repositories;
namespace DesktopLamour.Features.HomePage.Customers.Domain.UseCases;

public sealed class DeleteCustomerUseCase : IDeleteCustomerUseCase
{
    private readonly ICustomerRepository _repository;
    public DeleteCustomerUseCase(ICustomerRepository repository) => _repository = repository;
    public Task ExecuteAsync(int customerId, CancellationToken ct = default)
        => _repository.DeleteAsync(customerId, ct);
}
