// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Deposits.Domain.UseCases;

public interface IGetDepositDeductionsUseCase
{
    Task<IEnumerable<DepositDeductionResponseDto>> ExecuteAsync(
        int? customerId, int? employeeId, int? salesOrderId,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
}
