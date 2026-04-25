// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;

public record CreateProductInput(
    string Code, string Name, string Category, string Unit,
    decimal CostPrice, decimal SellingPrice, int StockQuantity, bool IsActive);
