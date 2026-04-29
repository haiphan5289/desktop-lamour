// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public interface IDeletePaymentUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
