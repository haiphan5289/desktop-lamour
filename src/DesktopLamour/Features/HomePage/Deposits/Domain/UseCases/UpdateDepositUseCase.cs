// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Deposits.Data.Services;
using DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Deposits.Domain.UseCases;

public sealed class UpdateDepositUseCase : IUpdateDepositUseCase
{
    private readonly IDepositService _service;

    public UpdateDepositUseCase(IDepositService service) => _service = service;

    public Task<DepositResponseDto> ExecuteAsync(int id, UpdateDepositRequestDto request, CancellationToken ct = default)
        => _service.UpdateAsync(id, request, ct);
}
