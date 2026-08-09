// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductUnits.Domain.Models;
namespace DesktopLamour.Features.HomePage.ProductUnits.Domain.UseCases;

public interface IGetProductUnitsUseCase
{
    Task<IEnumerable<ProductUnit>> ExecuteAsync(CancellationToken ct = default);
}
