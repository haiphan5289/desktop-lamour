// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Deposits.Data.Services;
using DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Deposits.Domain.UseCases;

public sealed class GetDepositDeductionsUseCase : IGetDepositDeductionsUseCase
{
    private readonly IDepositDeductionService _service;

    public GetDepositDeductionsUseCase(IDepositDeductionService service) => _service = service;

    public Task<IEnumerable<DepositDeductionResponseDto>> ExecuteAsync(
        int? customerId, int? employeeId, int? salesOrderId,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
        => _service.GetAllAsync(customerId, employeeId, salesOrderId, fromDate, toDate, ct);
}
