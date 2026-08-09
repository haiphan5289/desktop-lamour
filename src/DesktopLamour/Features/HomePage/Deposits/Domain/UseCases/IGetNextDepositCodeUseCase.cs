// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Deposits.Domain.UseCases;

public interface IGetNextDepositCodeUseCase
{
    Task<string> ExecuteAsync(CancellationToken ct = default);
}
