// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Deposits.Data.Services;

public interface IDepositDeductionService
{
    Task<IEnumerable<DepositDeductionResponseDto>> GetAllAsync(
        int? customerId, int? employeeId, int? salesOrderId,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);

    // 1 lần trừ cọc có thể sinh nhiều DepositDeduction (BE tự phân bổ FIFO qua nhiều Deposit).
    Task<IEnumerable<DepositDeductionResponseDto>> CreateAsync(CreateDepositDeductionRequestDto request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
