// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Suppliers.Domain.UseCases;

public interface IDeleteSupplierUseCase
{
    Task ExecuteAsync(int supplierId, CancellationToken ct = default);
}
