// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Deposits.Data.Services;
using DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Deposits.Domain.UseCases;

public sealed class GetDepositsUseCase : IGetDepositsUseCase
{
    private readonly IDepositService _service;

    public GetDepositsUseCase(IDepositService service) => _service = service;

    public Task<IEnumerable<DepositResponseDto>> ExecuteAsync(CancellationToken ct = default)
        => _service.GetAllAsync(ct);
}
