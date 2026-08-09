// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Deposits.Data.Services;

public interface IDepositDeductionService
{
    Task<IEnumerable<DepositDeductionResponseDto>> GetAllAsync(
        int? customerId, int? employeeId, int? salesOrderId,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);

    Task<DepositDeductionResponseDto> CreateAsync(CreateDepositDeductionRequestDto request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
