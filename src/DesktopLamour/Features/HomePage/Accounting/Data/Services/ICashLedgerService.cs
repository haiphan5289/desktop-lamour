// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Accounting.Domain.Models;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services;

public interface ICashLedgerService
{
    Task<CashLedgerResponseDto> GetCashLedgerAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken ct = default);

    // Báo cáo "Sổ kế toán chi tiết quỹ tiền mặt".
    Task<CashLedgerDetailReportDto> GetDetailReportAsync(
        CashLedgerReportFilter filter,
        CancellationToken ct = default);
}
