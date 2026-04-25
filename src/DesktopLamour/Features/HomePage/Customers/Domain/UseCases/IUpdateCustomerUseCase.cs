// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Customers.Domain.Models;
namespace DesktopLamour.Features.HomePage.Customers.Domain.UseCases;

public interface IUpdateCustomerUseCase
{
    Task<Customer> ExecuteAsync(UpdateCustomerInput input, CancellationToken ct = default);
}
