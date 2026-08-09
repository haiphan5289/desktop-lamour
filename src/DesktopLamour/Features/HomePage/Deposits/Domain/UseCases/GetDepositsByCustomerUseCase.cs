// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Deposits.Data.Services;
using DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Deposits.Domain.UseCases;

public sealed class GetDepositsByCustomerUseCase : IGetDepositsByCustomerUseCase
{
    private readonly IDepositService _service;

    public GetDepositsByCustomerUseCase(IDepositService service) => _service = service;

    public Task<IEnumerable<DepositResponseDto>> ExecuteAsync(int customerId, CancellationToken ct = default)
        => _service.GetByCustomerAsync(customerId, ct);
}
