// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Deposits.Domain.UseCases;

public interface ICreateDepositDeductionUseCase
{
    Task<IEnumerable<DepositDeductionResponseDto>> ExecuteAsync(CreateDepositDeductionRequestDto request, CancellationToken ct = default);
}
