// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Deposits.Data.Services;

namespace DesktopLamour.Features.HomePage.Deposits.Domain.UseCases;

public sealed class DeleteDepositUseCase : IDeleteDepositUseCase
{
    private readonly IDepositService _service;

    public DeleteDepositUseCase(IDepositService service) => _service = service;

    public Task ExecuteAsync(int id, CancellationToken ct = default)
        => _service.DeleteAsync(id, ct);
}
