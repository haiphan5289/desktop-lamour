// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Domain.Models;
namespace DesktopLamour.Features.HomePage.Domain.UseCases;

public interface ICreateSupplierUseCase
{
    Task<Supplier> ExecuteAsync(CreateSupplierInput input, CancellationToken ct = default);
}
