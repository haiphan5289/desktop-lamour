// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.SalesReturn.Domain.UseCases;

public interface IGetNextSalesReturnCodeUseCase
{
    Task<string> ExecuteAsync(CancellationToken ct = default);
}
