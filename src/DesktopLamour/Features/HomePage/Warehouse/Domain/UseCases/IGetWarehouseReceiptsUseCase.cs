// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;

public interface IGetWarehouseReceiptsUseCase
{
    Task<IEnumerable<WarehouseReceiptResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
