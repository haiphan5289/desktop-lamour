// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Deposits.Data.Services;
using DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Deposits.Domain.UseCases;

public sealed class CreateDepositUseCase : ICreateDepositUseCase
{
    private readonly IDepositService _service;

    public CreateDepositUseCase(IDepositService service) => _service = service;

    public Task<DepositResponseDto> ExecuteAsync(CreateDepositRequestDto request, CancellationToken ct = default)
        => _service.CreateAsync(request, ct);
}
