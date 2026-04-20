// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Domain.Models;
namespace DesktopLamour.Features.HomePage.Domain.UseCases;

public interface IGetSuppliersUseCase
{
    Task<IEnumerable<Supplier>> ExecuteAsync(CancellationToken ct = default);
}
