// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Sales.Domain.UseCases;

public interface IDeleteSalesOrderUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
