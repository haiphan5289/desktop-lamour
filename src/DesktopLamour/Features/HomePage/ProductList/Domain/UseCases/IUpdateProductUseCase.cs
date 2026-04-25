// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
namespace DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;

public interface IUpdateProductUseCase
{
    Task<Product> ExecuteAsync(UpdateProductInput input, CancellationToken ct = default);
}
