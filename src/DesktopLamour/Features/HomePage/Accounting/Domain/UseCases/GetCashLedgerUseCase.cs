// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public sealed class GetCashLedgerUseCase : IGetCashLedgerUseCase
{
    private readonly ICashLedgerService _service;

    public GetCashLedgerUseCase(ICashLedgerService service)
        => _service = service;

    public Task<CashLedgerResponseDto> ExecuteAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken ct = default)
        => _service.GetCashLedgerAsync(fromDate, toDate, ct);
}
