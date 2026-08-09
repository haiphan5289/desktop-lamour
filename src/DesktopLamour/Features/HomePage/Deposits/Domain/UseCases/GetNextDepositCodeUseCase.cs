// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Deposits.Data.Services;

namespace DesktopLamour.Features.HomePage.Deposits.Domain.UseCases;

public sealed class GetNextDepositCodeUseCase : IGetNextDepositCodeUseCase
{
    private readonly IDepositService _service;

    public GetNextDepositCodeUseCase(IDepositService service) => _service = service;

    public Task<string> ExecuteAsync(CancellationToken ct = default)
        => _service.GetNextCodeAsync(ct);
}
