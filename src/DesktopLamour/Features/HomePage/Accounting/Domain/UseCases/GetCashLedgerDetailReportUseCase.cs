// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Accounting.Domain.Models;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public sealed class GetCashLedgerDetailReportUseCase : IGetCashLedgerDetailReportUseCase
{
    private readonly ICashLedgerService _service;

    public GetCashLedgerDetailReportUseCase(ICashLedgerService service)
        => _service = service;

    public Task<CashLedgerDetailReportDto> ExecuteAsync(CashLedgerReportFilter filter, CancellationToken ct = default)
        => _service.GetDetailReportAsync(filter, ct);
}
