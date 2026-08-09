// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Deposits.Data.Services;
using DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Deposits.Domain.UseCases;

public sealed class CreateDepositDeductionUseCase : ICreateDepositDeductionUseCase
{
    private readonly IDepositDeductionService _service;

    public CreateDepositDeductionUseCase(IDepositDeductionService service) => _service = service;

    public Task<DepositDeductionResponseDto> ExecuteAsync(CreateDepositDeductionRequestDto request, CancellationToken ct = default)
        => _service.CreateAsync(request, ct);
}
