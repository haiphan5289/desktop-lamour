// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.SalesReturn.Domain.UseCases;

public interface IGetSalesReturnsUseCase
{
    Task<IEnumerable<SalesReturnResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
