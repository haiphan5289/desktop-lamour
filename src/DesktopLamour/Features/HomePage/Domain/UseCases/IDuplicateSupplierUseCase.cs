// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Domain.Models;
namespace DesktopLamour.Features.HomePage.Domain.UseCases;

public interface IDuplicateSupplierUseCase
{
    Task<Supplier> ExecuteAsync(int supplierId, CancellationToken ct = default);
}
