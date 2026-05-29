// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Sales.Domain.UseCases;

public interface IGetNextSalesOrderCodeUseCase
{
    Task<string> ExecuteAsync(CancellationToken ct = default);
}
