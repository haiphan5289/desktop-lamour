// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Sales.Domain.UseCases;

public interface IGetSalesOrdersUseCase
{
    Task<IEnumerable<SalesOrderResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
