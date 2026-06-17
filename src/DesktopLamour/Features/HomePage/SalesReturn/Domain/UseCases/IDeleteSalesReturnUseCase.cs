// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.SalesReturn.Domain.UseCases;

public interface IDeleteSalesReturnUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
