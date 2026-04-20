// IHomeService.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.HomePage.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Data.Services;

public interface IHomeService
{
    Task<IEnumerable<ProductResponseDto>> GetProductsAsync(CancellationToken ct = default);
    Task<IEnumerable<SupplierResponseDto>> GetSuppliersAsync(CancellationToken ct = default);
}
