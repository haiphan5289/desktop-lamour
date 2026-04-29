// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public interface IDeleteReceiptUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
