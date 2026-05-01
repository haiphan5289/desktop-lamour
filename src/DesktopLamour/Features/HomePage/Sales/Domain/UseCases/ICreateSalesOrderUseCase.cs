// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Sales.Domain.UseCases;

public interface ICreateSalesOrderUseCase
{
    Task<SalesOrderResponseDto> ExecuteAsync(CreateSalesOrderRequestDto request, CancellationToken ct = default);
}
