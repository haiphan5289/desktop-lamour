// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.ProductUnits.Domain.UseCases;

public interface IDeleteProductUnitUseCase
{
    Task ExecuteAsync(int unitId, CancellationToken ct = default);
}
