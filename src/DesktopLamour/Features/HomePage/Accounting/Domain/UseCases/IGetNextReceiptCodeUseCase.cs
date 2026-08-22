// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public interface IGetNextReceiptCodeUseCase
{
    Task<string> ExecuteAsync(CancellationToken ct = default);
}
