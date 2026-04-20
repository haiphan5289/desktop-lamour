// HomeDashboard.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Features.HomePage.Domain.Models;

public record HomeDashboard(
    IEnumerable<ProductSummary> Products,
    IEnumerable<SupplierSummary> Suppliers
);
