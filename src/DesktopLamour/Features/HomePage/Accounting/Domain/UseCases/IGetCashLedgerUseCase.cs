// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public interface IGetCashLedgerUseCase
{
    Task<CashLedgerResponseDto> ExecuteAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken ct = default);
}
