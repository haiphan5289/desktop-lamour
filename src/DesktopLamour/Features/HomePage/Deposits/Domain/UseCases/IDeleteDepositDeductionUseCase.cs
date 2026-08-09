// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Deposits.Domain.UseCases;

public interface IDeleteDepositDeductionUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
