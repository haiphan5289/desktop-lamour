// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;

public interface IDeleteWarehouseReceiptUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
