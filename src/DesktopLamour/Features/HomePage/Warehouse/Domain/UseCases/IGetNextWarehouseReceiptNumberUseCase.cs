// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;

public interface IGetNextWarehouseReceiptNumberUseCase
{
    Task<string?> ExecuteAsync(CancellationToken ct = default);
}
