// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;

public interface IDeleteProductUseCase
{
    Task ExecuteAsync(int productId, CancellationToken ct = default);
}
