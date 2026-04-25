// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Customers.Domain.Models;
namespace DesktopLamour.Features.HomePage.Customers.Domain.UseCases;

public interface IGetCustomersUseCase
{
    Task<IEnumerable<Customer>> ExecuteAsync(CancellationToken ct = default);
}
