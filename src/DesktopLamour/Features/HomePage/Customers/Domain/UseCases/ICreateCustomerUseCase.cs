// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Customers.Domain.Models;
namespace DesktopLamour.Features.HomePage.Customers.Domain.UseCases;

public interface ICreateCustomerUseCase
{
    Task<Customer> ExecuteAsync(CreateCustomerInput input, CancellationToken ct = default);
}
