// IHomeRepository.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.HomePage.Domain.Models;

namespace DesktopLamour.Features.HomePage.Data.Repositories;

public interface IHomeRepository
{
    Task<IEnumerable<ProductSummary>> GetProductsAsync(CancellationToken ct = default);
    Task<IEnumerable<SupplierSummary>> GetSuppliersAsync(CancellationToken ct = default);
}
