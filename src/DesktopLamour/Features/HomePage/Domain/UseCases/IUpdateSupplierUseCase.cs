// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Domain.Models;
namespace DesktopLamour.Features.HomePage.Domain.UseCases;

public interface IUpdateSupplierUseCase
{
    Task<Supplier> ExecuteAsync(UpdateSupplierInput input, CancellationToken ct = default);
}
