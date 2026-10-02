// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Accounting.Domain.Models;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public interface IGetCashLedgerDetailReportUseCase
{
    Task<CashLedgerDetailReportDto> ExecuteAsync(CashLedgerReportFilter filter, CancellationToken ct = default);
}
