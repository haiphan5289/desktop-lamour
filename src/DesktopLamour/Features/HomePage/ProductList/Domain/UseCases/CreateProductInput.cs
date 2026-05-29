// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
namespace DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;

public record CreateProductInput(
    string Code, string Name, string Category, string Unit,
    decimal CostPrice, decimal SellingPrice, int StockQuantity, bool IsActive,
    VatRateType? VatRate = null, TaxReductionStatus? TaxReductionType = null,
    decimal? ImportTaxRate = null, decimal? ExportTaxRate = null, string? ExciseTaxGroup = null);
