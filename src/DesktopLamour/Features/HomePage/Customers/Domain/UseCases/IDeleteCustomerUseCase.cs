// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Customers.Domain.UseCases;

public interface IDeleteCustomerUseCase
{
    Task ExecuteAsync(int customerId, CancellationToken ct = default);
}
