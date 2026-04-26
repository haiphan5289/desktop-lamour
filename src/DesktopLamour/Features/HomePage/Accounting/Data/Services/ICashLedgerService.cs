// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services;

public interface ICashLedgerService
{
    Task<CashLedgerResponseDto> GetCashLedgerAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken ct = default);
}
